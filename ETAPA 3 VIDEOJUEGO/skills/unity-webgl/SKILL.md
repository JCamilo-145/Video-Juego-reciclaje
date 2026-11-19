---
name: unity-webgl
description: "Trigger: WebGL build, web build, build target switching, browser testing, persistentDataPath, Brotli or Gzip compression, web input limits. Configure, build, and iterate Unity 6 WebGL builds."
license: Apache-2.0
metadata:
  author: "fase-3-web-project"
  version: "1.0"
---

## Activation Contract

Load this skill when switching the build target to WebGL, producing or publishing a web build, reducing build size, or handling browser platform limits (threads, file IO, persistence, input).

## Hard Rules

- Managed C# on Web is single-threaded. NEVER use `System.Threading.Thread` (thread delegates silently never execute), `Task.Run`, `Parallel`, or `Task.Delay` (they cause unrecoverable browser hangs). Use coroutines or async/await; continuations run on the main thread. Unity 6 multithreading on Web applies only to Burst-compiled jobs and engine code (requires the Enable Native C/C++ Multithreading setting and a SharedArrayBuffer-capable browser); this project uses no Burst.
- The browser is not a filesystem. Most `System.IO` file IO runs against Emscripten's in-memory virtual filesystem (MEMFS): writes vanish on page reload, asynchronous IO APIs hang the browser unrecoverably, and `DriveInfo` throws. Never rely on it for persistence.
- Native plug-ins on Web must be compiled to WebAssembly with Unity's bundled Emscripten toolchain (`.a`/`.bc` archives); desktop `.dll`/`.so` libraries do not load. For browser APIs, use JavaScript plug-ins instead of native code.
- `Application.persistentDataPath` maps to the browser's IndexedDB file system (`/idbfs/<hash>` derived from the page's directory URL): data is per-location and wiped when the user clears site data. Persist through that path or `PlayerPrefs`; never assume a real filesystem.
- Web input covers keyboard, mouse, touch, sensors, and gamepads/joysticks through the browser's HTML5 Gamepad API. Browsers expose input devices only after the user interacts with them while the application is in focus: prompt the user to press a gamepad button before reading gamepad state, and keep a keyboard/mouse fallback for every gamepad control.
- Scenes loaded at runtime MUST appear in the build's scene list (Build Profiles window). Load by build index (`SceneManager.LoadScene(1)`) and re-verify every index after reordering the scene list.

## Decision Gates

### Platform-safe replacements

| Need | WebGL-safe approach |
|---|---|
| Background work | coroutine or async/await on the main thread (managed threads are unsupported) |
| Save/load data | `Application.persistentDataPath` or `PlayerPrefs` (both land in IndexedDB) |
| Native plugin call | recompile the C/C++ with Unity's Emscripten toolchain (`.a`/`.bc`); otherwise reimplement the logic in C# or a JavaScript plug-in |
| Gamepad input | supported via the browser Gamepad API once the user presses a button while the app is in focus; keep a keyboard/mouse fallback |
| Exit the application | not possible — `Application.Quit()` is ignored in WebGL; show an end screen instead |
| Large scenes | browsers cap WebAssembly memory (commonly around 2 GB); profile before adding content |

### Compression format (Player Settings > Publishing Settings)

| Situation | Setting |
|---|---|
| Host serves Brotli (`Content-Encoding: br`) | Brotli Compression — smallest builds |
| Host serves Gzip (`Content-Encoding: gzip`) | Gzip Compression |
| Cannot configure server headers | enable Decompression Fallback (browser-side decompression) |
| Quick local iteration, no server setup | Compression Format: Disabled (largest build, works anywhere) |

A wrong compression setting shows up as a build that never finishes loading or fails on fetch: match the format to the server exactly.

### Asset size discipline — web players download everything

| Asset | Discipline |
|---|---|
| Textures | biggest lever: cap Max Size at the on-screen display size, keep compressed import formats, prefer Crunch for long-lived opaque textures; audit the build report before touching code |
| Audio | Vorbis encoding; short clips Decompress On Load, long clips Streaming; Force To Mono for 3D sounds |
| Meshes | Mesh Compression (Low/Medium) in import settings where silhouettes tolerate it |
| Scenes | remove developer/test content; anything referenced by an included scene ships in the build |

### Iteration tips

- First builds: enable Development Build for readable stack traces and Profiler connection; disable it for the shipped build.
- Debug runtime errors with the browser developer console (F12); Development Build makes those stack traces readable there.
- Build And Run starts a local web server automatically. Double-clicking `index.html` (file://) FAILS — WebGL must be served over HTTP.
- Manual test: run `python -m http.server` in the build folder and open `http://localhost:8000`.
- Iterate with Development Build + Compression Disabled; produce the release candidate with Development off and Brotli/Gzip matching the host.

## Execution Steps

1. Switch platform: File > Build Profiles > WebGL > Switch Platform (if the platform list is hidden: Edit > Project Settings > Editor > Build Profiles > disable Hide Classic Platforms), then wait for the full reimport.
2. Add every runtime-loaded scene to the scene list; record the build index each `SceneManager.LoadScene` call uses.
3. In Player Settings, set the Compression Format to match the hosting (Decision Gates table).
4. Build into a folder outside `Assets/` (for example `Builds/WebGL`, already gitignored).
5. Verify with Build And Run (local server), then on the real target server.
6. Before shipping: remove test scenes from the scene list, disable Development Build, re-enable compression.

## Output Contract

- Build loads and runs in the browser with no console errors.
- No `System.Threading.Thread`, ThreadPool-backed APIs (`Task.Run`, `Parallel`, `Task.Delay`), async file IO, or desktop-only native plug-ins in shipped scripts.
- All runtime scene loads use verified build indices.
- Build output sits outside `Assets/` and is not committed.

## References

- `../AGENTS.md` — project conventions
- `../unity-scripting/SKILL.md` — coroutine/async patterns for a threadless platform
- `../unity-testing/SKILL.md` — test scenes must leave the build list before shipping
- [Web platform development](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl.html) — Unity 6.6 manual, Web section index
- [Web technical limitations](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-technical-overview.html) — Unity 6.6 manual, Web platform restrictions
- [Build and distribute a Web application](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-building-distribution.html) — Unity 6.6 manual, building, optimizing, and distributing Web builds
- [Web Player settings](https://docs.unity3d.com/6000.6/Documentation/Manual/class-PlayerSettingsWebGL.html) — Unity 6.6 manual, Publishing Settings including Compression Format and Decompression Fallback
- [Deploy a Web application](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-deploying.html) — Unity 6.6 manual, server configuration and Content-Encoding headers
- [Input in Web](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-input.html) — Unity 6.6 manual, gamepad, touch, keyboard, and sensor input
- [Multithreading in Unity Web](https://docs.unity3d.com/6000.6/Documentation/Manual/web-multithreading.html) — Unity 6.6 manual, the managed single-thread limit and Burst multithreading
- [.NET API support on the Web platform](https://docs.unity3d.com/6000.6/Documentation/Manual/web-dotnet-api-support.html) — Unity 6.6 manual, System.IO, System.Threading, and networking limitations
- [Web native plug-ins for Emscripten](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-native-plugins-with-emscripten.html) — Unity 6.6 manual, Emscripten-compiled native plug-ins
- [Application.persistentDataPath](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-persistentDataPath.html) — Unity 6.6 Scripting API, IndexedDB file system mapping on Web
- [Introduction to build profiles](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html) — Unity 6.6 manual, the Build Profiles window
- Unity links target Unity 6.6 (6000.6), this project's exact editor version, verified October 2026. Unity's Spanish manual ends at 2019.4 LTS; there is no Unity 6 Spanish manual, so never cite Spanish Unity pages for this project.
