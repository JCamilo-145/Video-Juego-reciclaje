---
name: unity-testing
description: "Trigger: Unity tests, NUnit, EditMode or PlayMode tests, Test Runner, UnityTest, test asmdef, com.unity.test-framework. Install UTF and write tests for this Unity 6 project."
license: Apache-2.0
metadata:
  author: "fase-3-web-project"
  version: "1.0"
---

## Activation Contract

Load this skill when asked to write, run, or review tests; when Test Runner, NUnit, EditMode, PlayMode, or coverage comes up; and before creating any `Tests` folder or adding Unity Test Framework to the project.

## Hard Rules

- `com.unity.test-framework` is REQUIRED for testing and is currently ABSENT from `Packages/manifest.json`. Before writing any test, add it first:
  - Package Manager: Window > Package Manager > Unity Registry > Test Framework, or
  - edit `Packages/manifest.json` directly: add `"com.unity.test-framework": "1.9.0"` (latest stable, verified October 2026; accept any newer stable the Editor resolves), then let Unity resolve.
- Every test assembly lives in its own `.asmdef` with the NUnit override (see Decision Gates). Tests dropped into `Assembly-CSharp` cannot compile against `nunit.framework.dll`.
- Tests must NEVER reach a player build. Keep every test `.asmdef` editor-only (`includePlatforms: ["Editor"]`) plus the `UNITY_INCLUDE_TESTS` define constraint.
- PlayMode tests never modify or save player scenes. Use dedicated test scenes and clean up in teardown.

## Decision Gates

### EditMode vs PlayMode

| Test target | Mode | Why |
|---|---|---|
| Pure C# logic: math, rules, plain data classes, ScriptableObject data | EditMode | fast, no play loop |
| Serialized defaults and plain class state | EditMode | runs against editor API without entering play |
| MonoBehaviour behavior across frames, coroutines, physics, time-based logic | PlayMode | needs the runtime loop |
| Scene loading, uGUI interaction, Cinemachine state | PlayMode | components must execute in play mode |

Rule of thumb: EditMode by default; drop to PlayMode only when the code needs the engine loop.

### Test assembly definition

Create `Assets/Tests/Editor/` and/or `Assets/Tests/PlayMode/` with one `.asmdef` each:

```json
{
    "name": "Game.Editor.Tests",
    "rootNamespace": "Game.Editor.Tests",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "Game"
    ],
    "includePlatforms": ["Editor"],
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "autoReferenced": false
}
```

- `UnityEngine.TestRunner` + `UnityEditor.TestRunner`: pull in the test runner APIs for the Edit mode assembly above. A Play mode test assembly must reference only `UnityEngine.TestRunner` — per the Unity 6.6 docs, the `UnityEditor.TestRunner` reference is only available for Edit mode tests.
- The "nunit" override is `overrideReferences: true` + `precompiledReferences: ["nunit.framework.dll"]`; without it, `[Test]` and `[UnityTest]` do not compile.
- Replace `"Game"` in `"references"` with the runtime `.asmdef` name under test. A test assembly cannot reference the predefined `Assembly-CSharp`, so code under test must live in a custom assembly.
- `UNITY_INCLUDE_TESTS` is defined by Unity only while compiling tests; keep `includePlatforms: ["Editor"]` as the guarantee that test code never enters player builds.
- Editor-only asmdefs cover both EditMode tests and PlayMode tests run in the Editor (the default for this project). Only drop `includePlatforms` if tests must run on a device player, and then exclude WebGL explicitly.

### Attribute quick reference

| Attribute | Use |
|---|---|
| `[Test]` | synchronous test (EditMode default) |
| `[UnityTest]` | returns `IEnumerator`; `yield return null` waits one frame (PlayMode) |
| `[SetUp]` / `[TearDown]` | run before/after EACH test |
| `[UnitySetUp]` / `[UnityTearDown]` | coroutine versions for PlayMode |
| `[TestCase(...)]` | run the same test with different inputs |

## Execution Steps

1. Add the test framework package and let Unity resolve dependencies.
2. Create the test folder and `.asmdef` per the Decision Gates; reference the runtime assembly under test.
3. Write tests in Arrange-Act-Assert order, one behavior per test, named `Method_Scenario_ExpectedResult`.
4. Run via Window > General > Test Runner; run EditMode and PlayMode tabs separately.
5. Fix failing tests before adding new ones; never delete a failing test to go green.

Canonical tests:

```csharp
public class HealthTests
{
    private Health _health;

    [SetUp]
    public void SetUp() => _health = new Health(100);   // Arrange per test

    [Test]
    public void TakeDamage_ThirtyDamage_ReducesToSeventy()
    {
        _health.TakeDamage(30);                          // Act
        Assert.AreEqual(70, _health.Value);              // Assert
    }
}

public class SpawnerTests
{
    private GameObject _root;

    [TearDown]
    public void TearDown() => UnityEngine.Object.Destroy(_root);

    [UnityTest]
    public IEnumerator Spawner_Activated_SpawnsOnNextFrame()
    {
        _root = new GameObject("root");                  // Arrange
        var spawner = _root.AddComponent<Spawner>();
        yield return null;                               // wait one frame
        Assert.AreEqual(1, spawner.SpawnedCount);        // Assert
    }
}
```

### PlayMode test scene hygiene

- Keep test scenes in `Assets/Tests/Scenes/` with a `Test` prefix; never open or modify game scenes in `Assets/Scenes` from a test.
- Load additively in `[UnitySetUp]` with `SceneManager.LoadScene("TestArena", LoadSceneMode.Additive)` and unload the same scene in `[UnityTearDown]`.
- Additive loading by name requires the scene to be in the build's scene list (Build Profiles window in Unity 6). Remove test scenes from that list before producing a WebGL build (see unity-webgl skill).
- Destroy every GameObject a test creates; leaked objects poison later tests.
- Never save a scene from a test. Dirt left on disk is test debt.

## Output Contract

- Test Framework package added; `Packages/manifest.json` change committed.
- Test assemblies compile and pass in Test Runner (both modes if both exist).
- No test code compiled into player builds.
- Player scenes untouched by test runs.

## References

- `../AGENTS.md` — project conventions
- `../unity-scripting/SKILL.md` — runtime asmdef structure these tests reference
- `../unity-webgl/SKILL.md` — build list hygiene before shipping
- [Unity Test Framework package documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html) — version-independent entry point for the com.unity.test-framework package docs (currently 1.9, scripting API reference)
- [Testing your code](https://docs.unity3d.com/6000.6/Documentation/Manual/test-framework/test-framework-introduction.html) — Unity 6.6 manual, Unity Test Framework user guide
- [Edit mode and Play mode tests](https://docs.unity3d.com/6000.6/Documentation/Manual/test-framework/edit-mode-vs-play-mode-tests.html) — Unity 6.6 manual, mode selection and test assembly requirements
- [Create a test assembly](https://docs.unity3d.com/6000.6/Documentation/Manual/test-framework/workflow-create-test-assembly.html) — Unity 6.6 manual, test assembly setup from the Test Runner window
- Unity links target Unity 6.6 (6000.6), this project's exact editor version, verified October 2026. Unity's Spanish manual ends at 2019.4 LTS; there is no Unity 6 Spanish manual, so never cite Spanish Unity pages for this project.
