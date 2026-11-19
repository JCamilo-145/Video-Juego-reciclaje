---
name: unity-scripting
description: "Trigger: Unity C# scripts, MonoBehaviours, lifecycle phases, SerializeField, component caching, C# events, coroutines vs async, asmdef, .meta GUIDs, URP materials. Project C# conventions for Unity 6 URP."
license: Apache-2.0
metadata:
  author: "fase-3-web-project"
  version: "1.0"
---

## Activation Contract

Load this skill when writing, reviewing, or refactoring any C# file under `Assets/`; when creating MonoBehaviours, ScriptableObjects, or editor scripts; when editing assembly definitions; and when touching materials, shaders, or any other asset file.

## Hard Rules

- NEVER create, move, rename, or delete assets or folders outside the Unity Editor. Every asset's identity is the GUID stored in its `.meta` file; filesystem-level operations break that link silently.
- ALWAYS commit each asset's `.meta` file in the same commit as the asset. An asset committed without its `.meta` is regenerated with a new GUID on the next machine, silently unlinking scenes, prefabs, and materials.
- Expose Inspector fields with `[SerializeField] private`, never `public`. Public fields turn internal state into an API any script can mutate.
- Cache component references once in `Awake`. `GetComponent`, `FindObjectOfType` (deprecated), `FindFirstObjectByType`, and `GameObject.Find` inside `Update`, `FixedUpdate`, or `LateUpdate` is a defect, not a style preference.
- Use only URP shaders: `URP/Lit`, `URP/Simple Lit`, `URP/Unlit`, and Shader Graph. Built-in `Standard` or HDRP shaders render magenta under URP.
- Materials reference shaders by GUID. Switching render pipelines invalidates every material in the project. This project is URP 17.6.0 — do not switch pipelines.

## Decision Gates

### MonoBehaviour lifecycle - what is safe where

| Phase | Runs | Safe here | Never here |
|---|---|---|---|
| `Awake` | once per instance, even if the component starts disabled (GameObject must be active) | cache `GetComponent` results, initialize own fields | assume other objects are initialized; `Awake` order across objects is not guaranteed |
| `OnEnable` | every time the component activates | subscribe to events, start observation | subscribe without a matching unsubscribe in `OnDisable` |
| `Start` | once, before the first `Update`, after all `Awake` calls | cross-object wiring, reading other objects | per-frame work |
| `FixedUpdate` | fixed timestep, 0 to N times per frame | `Rigidbody` forces, physics reads and writes | input polling, camera work |
| `Update` | once per frame | input, timers, gameplay logic | physics writes, order-dependent camera work |
| `LateUpdate` | after every `Update` | camera follow, reactions to final positions | physics writes |
| `OnDestroy` | teardown | stop coroutines, release resources | assume other objects still exist; destruction order is not guaranteed |

Pairing rule: subscribe in `OnEnable`, unsubscribe in `OnDisable`. `OnDisable` always runs before `OnDestroy`, which makes it the reliable cleanup point.

### Decoupling - how objects communicate

| Need | Use |
|---|---|
| Code raises, other code reacts | `public event Action<int> HealthChanged;` — no Inspector wiring, no held references |
| Designer wires listeners in the Inspector | `[SerializeField] private UnityEvent` |
| One object drives another every frame | direct reference, cached in `Awake` |

Prefer events over chains of drag-references. A script must not hold references to distant objects just to observe them; publishers raise events, subscribers decide what happens.

### Coroutines vs async/await

| Concern | Coroutine | async/await |
|---|---|---|
| Lifetime | tied to the GameObject: stops on deactivate or destroy, but NOT on `enabled = false` (common gotcha) | not tied: guard with `destroyCancellationToken` (Unity 6) or null checks |
| Wait one frame | `yield return null` | `await Awaitable.NextFrame()` |
| Timed wait | `WaitForSeconds` allocates; cache the instance when reused | `await Awaitable.WaitForSecondsAsync(t)` |
| Best for | scripted sequences bound to a GameObject (move, wait, fire, die) | `AsyncOperation` flows: scene loading, web requests |
| WebGL | safe | safe: continuations run on the main thread; managed C# on Web is single-threaded |

Default to coroutines for object-bound timing; use async/await for load and flow operations.

### Assembly definitions

| Situation | Action |
|---|---|
| Runtime code for a distinct feature | one `.asmdef` per feature folder (for example `Assets/Scripts/Player/Player.asmdef`) |
| Editor-only tooling | `.asmdef` with `includePlatforms: ["Editor"]` |
| Tests | test `.asmdef` referencing the runtime assembly (see unity-testing skill) |
| One-file prototype | default `Assembly-CSharp` is acceptable; add an `.asmdef` before the folder grows |

## Execution Steps

1. Create scripts inside the Unity Editor (Project window, Create > C# Script) so Unity generates the `.meta` GUID together with the asset.
2. Cache every component reference in `Awake`; wire to other objects in `Start`.
3. Subscribe in `OnEnable`; unsubscribe in `OnDisable`.
4. Keep `Update` lean: physics in `FixedUpdate`, camera in `LateUpdate`.
5. Expose Inspector fields as `[SerializeField] private` only.
6. Add an `.asmdef` when a script folder grows past a handful of files.

Canonical shape:

```csharp
public sealed class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private float _destroyDelay = 1.5f;

    public event Action<int> HealthChanged;  // HUD listens; no drag-references

    private int _health;

    private void Awake()
    {
        _health = _maxHealth;  // own state only; other objects may not be ready
    }

    public void TakeDamage(int amount)
    {
        _health = Mathf.Max(0, _health - amount);
        HealthChanged?.Invoke(_health);
        if (_health == 0) StartCoroutine(DieRoutine());
    }

    private System.Collections.IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(_destroyDelay);
        Destroy(gameObject);
    }
}
```

## Output Contract

- Script compiles with zero console errors.
- No per-frame lookups; every reference is cached in `Awake`.
- Every subscribe has a matching unsubscribe.
- The `.meta` file is committed together with the script.
- Materials use URP shaders only.

## References

- `../AGENTS.md` — project-wide conventions
- `../unity-testing/SKILL.md` — test assembly wiring for these scripts
- `../unity-webgl/SKILL.md` — platform constraints that shape script code
- [Gameplay programming](https://docs.unity3d.com/6000.6/Documentation/Manual/scripting.html) — Unity 6.6 manual, scripting section index
- [Event function execution order](https://docs.unity3d.com/6000.6/Documentation/Manual/execution-order.html) — Unity 6.6 manual, MonoBehaviour lifecycle and update order
- [MonoBehaviour](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.html) — Unity 6.6 Scripting API, base class reference including all event function messages
- [Organize scripts into assemblies](https://docs.unity3d.com/6000.6/Documentation/Manual/assembly-definition-files.html) — Unity 6.6 manual, assembly definition files
- [Split tasks across frames with coroutines](https://docs.unity3d.com/6000.6/Documentation/Manual/coroutines-section.html) — Unity 6.6 manual, coroutine authoring and yield instructions
- [Asynchronous programming with the Awaitable class](https://docs.unity3d.com/6000.6/Documentation/Manual/async-await-support.html) — Unity 6.6 manual, async/await and Awaitable
- Unity links target Unity 6.6 (6000.6), this project's exact editor version, verified October 2026. Unity's Spanish manual ends at 2019.4 LTS; there is no Unity 6 Spanish manual, so never cite Spanish Unity pages for this project.
