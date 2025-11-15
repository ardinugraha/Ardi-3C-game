# Copilot instructions for SampleAGC (Unity)

Summary
- This is a Unity project (SampleAGC) organized under `Assets/` with game code in `Assets/Game` and engine/package metadata at the repo root (`*.csproj`, `Packages/`, `ProjectSettings/`). Use the Unity Editor as the primary build/debug environment; code edits are C# MonoBehaviour scripts compiled by Unity.

Big picture (what to know quickly)
- Game code: `Assets/Game/Scripts/` — features are grouped by responsibility (e.g. `Input`, `Player`). Example: `Assets/Game/Scripts/Input/InputManager.cs` contains frame-driven input checks and exposes `Action<Vector2> OnMoveInput` used by other components.
- Resources: `Assets/Game/Resources/Player/Animation/...` contains animation clips for player states (Walk, Run, Jump, Climb, Punch, etc.).
- Integration pattern: components are mostly MonoBehaviours wired via serialized fields (e.g. `PlayerMovement` has `[SerializeField] private InputManager _input;`). Expect references to be set in the Unity Editor rather than created at runtime.
- Input: there is an `InputSystem_Actions.inputactions` asset present, but many scripts (e.g. `InputManager.cs`) still use Unity's legacy `Input.GetAxis` / `Input.GetKey` APIs — be careful if migrating input code.

Developer workflows (discoverable & actionable)
- Opening the project: open the folder in Unity (via Unity Hub) or open `SampleAGC.sln` to edit scripts in your IDE. From PowerShell you can open the solution with `Start-Process 'f:\\UnityProject\\SampleAGC\\SampleAGC.sln'`.
- Build & run: use the Unity Editor Play mode for iterative testing. There are no discoverable CI build scripts in the repo — do not assume a headless build helper exists.
- Debugging: attach the IDE debugger to the Unity Editor process (typical Visual Studio / Rider workflow). Inspect serialized field links on prefabs/scenes when behavior is missing.

Project-specific conventions & patterns
- File layout: feature folders under `Assets/Game/Scripts/<Feature>/` (e.g. `Player`, `Input`). Keep new gameplay classes under these feature folders.
- MonoBehaviour wiring: prefer `[SerializeField]` private fields for references (observed pattern in `PlayerMovement.cs`). Avoid relying on `FindObjectOfType` unless explicitly needed.
- Input handling: Input is polled each frame in `InputManager.Update()` using small `CheckXInput()` helpers. If adding new input, follow the same helper-per-action pattern and keep input-to-event translation in `InputManager`.
- Events: components expose actions/events rather than coupling to other components directly. Example: `InputManager` exposes `Action<Vector2> OnMoveInput` — other components subscribe to those events.

Integration points & dependencies to watch
- `InputSystem_Actions.inputactions` (Assets root) — if you change input, check both this asset and legacy input usage in scripts.
- `ProjectSettings/` and `Packages/` control engine/package versions — changing package APIs can break many scripts; cross-check `.csproj` files for package references when editing low-level systems.
- Many Unity package projects exist in the workspace (e.g., `Unity.AI.Navigation`, `Unity.Burst`, `Unity.VisualScripting.*`) — these indicate runtime or package-level features might be in use; search usages before refactoring package APIs.

What an AI agent should do first (practical checklist)
- 1) Open `Assets/Game/Scripts/Input/InputManager.cs` and `Assets/Game/Scripts/Player/PlayerMovement.cs` to understand input→player flow.
- 2) Search `Assets/Game/Resources/Player/Animation/` to see how animation names map to states (use exact filenames when referencing animation clips).
- 3) When modifying serialized references, remind the developer that they must re-link fields in the Editor or update prefab/scene assets.
- 4) If changing input APIs, flag both the `InputSystem_Actions.inputactions` asset and legacy `Input` usage for manual verification in the Editor.

Examples to cite in PRs/issues
- "InputManager currently polls `Input.GetAxis` and raises `OnMoveInput` (see `Assets/Game/Scripts/Input/InputManager.cs`)."
- "PlayerMovement depends on an `InputManager` serialized reference (see `Assets/Game/Scripts/Player/PlayerMovement.cs`); any API change must preserve that contract or update scene prefabs."

Notes & limitations (what the repo does not show)
- No automated test framework or CI build scripts were found — assume manual testing in Editor is required.
- No consistent namespace usage in scripts; classes live in global namespace. Keep new code consistent with this pattern unless performing a broader refactor.

If anything in this short guide is unclear or you want me to expand a specific section (build automation, a migration checklist to the new Input System, or a code-level example for event wiring), tell me which area to expand and I will iterate.
