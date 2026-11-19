# AGENTS.md

Conventions for AI agents working in this repository. Read before changing anything.

## Project Identity

- Unity 6 project on the Universal Render Pipeline (URP 17.6.0), targeting WebGL (browser) builds.
- Student course project: "Fase 3 - Proyecto Web" — UNAD, Semestre 4.
- Key packages: Input System 1.20.0, Cinemachine 6.6.0, uGUI 2.6.0 (includes TextMeshPro in Unity 6). Unity Test Framework is not installed yet; see `skills/unity-testing/SKILL.md` before writing tests.
- Main folders: `Assets/Scenes`, `Assets/Settings` (URP assets), `Assets/ExternalAssets`, `Assets/TutorialInfo`.

## Hard Rules

### Asset integrity — the #1 source of broken scenes and materials

- ALWAYS commit `.meta` files together with their assets, in the same commit. An asset's identity is the GUID inside its `.meta`; an asset committed without it gets regenerated with a different GUID.
- NEVER create, move, rename, or delete assets or folders in Explorer, a terminal, or a script. Do it in the Unity Editor's Project window so GUIDs survive.
- Broken GUIDs fail silently: "Missing (Mono Script)" on scenes and prefabs, pink materials, empty references. The breakage surfaces much later, not at commit time.

### Repository hygiene

- `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Obj/`, and `Builds/` are generated folders and are NEVER committed; they are already covered by `.gitignore`.
- `.config/kilo/` holds the workspace Gentle AI configuration and MUST stay tracked in git.

### Language

- Code artifacts, comments, identifiers, and technical documentation are written in English.

## Project Skills

| Skill | Path | Load when |
|---|---|---|
| unity-scripting | `skills/unity-scripting/SKILL.md` | Writing or reviewing any C# under `Assets/`; MonoBehaviour lifecycle, `[SerializeField]`, component caching, events, coroutines vs async, asmdef, materials |
| unity-testing | `skills/unity-testing/SKILL.md` | Adding Unity Test Framework, writing or running EditMode/PlayMode tests, test asmdef setup, test scene hygiene |
| unity-webgl | `skills/unity-webgl/SKILL.md` | Switching to WebGL, building for the browser, reducing build size, compression (Brotli/Gzip), browser limits (threads, file IO, persistence, input) |
