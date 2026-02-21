---
apply: always
---

# DunksJams AI Instructions

## Project summary

Unity 6000 (URP) game project with custom systems and editor tooling.

## Critical rules

- Ask before large refactors or architectural changes.
- Don't push without asking.
- If multiple choices, ask all questions at once.
- Search for existing solutions before creating new ones.
- You may create/edit/rename files in the project without asking.
- Do not claim something works unless you tested it.
- Use PowerShell (Windows).
- When the user mentions an error message, read Unity Editor.log.
- Do not create .meta files; Unity generates them.
- We are Unity 6000+ and have no backward compatibility for Unity < 6000.

## Project patterns

- Use EventManager + GameEvent for messaging when appropriate.
- Managers inherit SingletonBehavior<T>, implement InitInternal(), and use [DisallowMultipleComponent].
- Use DLog.Log() for logging, never Debug.Log().
- Use the global namespace.
- Private fields: _camelCase; public members: PascalCase.

## Code style

- Keep code tight: prefer expression-bodied members for simple methods/properties.
- Use modern C# (pattern matching, null-conditional, target-typed new).
- Use serialized fields and inspector-friendly attributes.
- Cache components; pool frequently spawned objects.

## Systems available (use before reinventing)

- FSM (FiniteState<T>, FiniteStateMachine<T>), steering behaviors + flow-field pathfinding.
- Tweening/easing, math/noise/DSP utilities, editor graph/GUI helpers.
- Data structures: ring buffer, queues, LRU cache, spatial hashes, pools.
- 2d & 3d object generation

## Unity specifics

- Use FindFirstObjectByType<T>() / FindObjectsByType<T>() (Unity 6000).
- Cleanup in OnDestroy (unsubscribe events, release pools).
- Menu items use the interrobang character (U+203D) in paths.

## Logs and tests

- Editor log: %LOCALAPPDATA%\Unity\Editor\Editor.log (previous: Editor-prev.log).
- Tests: Assets/Tests/Editor via Test Runner (Edit Mode).