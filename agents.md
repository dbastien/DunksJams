# Instructions for AI Coding Assistants

## Project Overview

**Project Name**: DunksJams
**Type**: Unity Game Development Project
**Engine Version**: Unity 6000 (Universal Render Pipeline)

This is a Unity game development project with custom systems and patterns. When assisting with code, follow the established conventions and patterns found in this codebase.

## ⚠️ Critical Agent Guidelines

**ALWAYS ASK BEFORE large refactors or architectural changes.**
- Don't reinvent the wheel - check if something already exists in the codebase before creating it.
- You're in Windows, use powershell.
- Read from the editor log when the user says there's an error message.
- Don't push without asking.
- You may create/edit/rename files as needed without asking each time.
- Don't say something works unless you've tested it.

## Best Practices for AI Assistants

### When Adding New Code
1. **Follow existing patterns**: Look for similar functionality and match the style
2. **Use established systems**: Prefer EventManager over direct references, use SingletonBehavior for managers
3. **Add logging**: Use `DLog.Log()` with descriptive messages
4. **Consider performance**: Use object pooling for frequently instantiated objects
5. **Make it Unity-friendly**: Use serialized fields, attributes, and Inspector-friendly code

### When Modifying Existing Code
1. **Preserve style**: Match existing code style and naming conventions
2. **Maintain interfaces**: Don't break existing public APIs without checking usages
3. **Update related code**: Check for other classes that might need updates
4. **Test impact**: Consider what systems depend on the code you're changing

### When Searching for Code
1. Use semantic search for finding functionality: "How does the score system work?"
2. Use grep for exact matches: class names, method names, specific strings
3. Check `Assets/Scripts/` structure to locate related files
4. Look for similar patterns in existing code before creating new ones

### Common Tasks
- **Creating a new manager**: Inherit from `SingletonBehavior<T>`, implement `InitInternal()`
- **Adding events**: Create event class inheriting `GameEvent`, use `EventManager.AddListener<T>()`
- **Creating UI**: Use `ScreenManager` patterns, check `UIComponents.cs` for utilities
- **Adding AI behavior**: Check `SteeringBehaviorSystem` and `FSM` patterns
- **Creating game objects procedurally**: See `AdvancedMeshMenu.cs` for mesh generation examples

## Code Style Conventions

### General C# Style
- Use modern C# syntax: expression-bodied members, pattern matching, null-conditional operators (`?.`, `??`)
- Private fields use underscore prefix: `_fieldName`, `_score`, `_isInitialized`
- Use `readonly` for immutable fields
- Prefer `var` for type inference when type is obvious
- Use expression-bodied methods for simple functions: `void Start() => ResetGame();`
- Public properties use PascalCase: `public int Score => _score;`

### Naming Conventions
- **Classes**: PascalCase (`ScoreManager`, `FiniteStateMachine`)
- **Methods**: PascalCase (`ResetGame`, `InitInternal`)
- **Private fields**: underscore prefix + camelCase (`_score`, `_isGameOver`)
- **Local variables**: camelCase (`meshFilter`, `angleStep`)
- **Constants**: camelCase
- **Unity Events**: PascalCase with `On` prefix (`OnScoreChanged`, `OnGameOver`)

### Unity-Specific Patterns
- Use `[DisallowMultipleComponent]` for singleton MonoBehaviour classes
- Manager classes should inherit from `SingletonBehavior<T>` pattern
- Use `DLog.Log()` for logging, not `Debug.Log()` directly
- Extension methods are preferred for Unity type utilities
- Use object initializer syntax: `var go = new GameObject(type.ToString()) { name = type.ToString() };`

## Architectural Patterns

### Singleton Pattern
The project uses a custom `SingletonBehavior<T>` base class for manager components:
- Inherit from `SingletonBehavior<T>` where T is the class itself
- Implement `InitInternal()` method instead of `Awake()`
- Access via static `Instance` property
- Automatically handles duplicate destruction and initialization

**Example Pattern**:
```csharp
public class ScoreManager : SingletonBehavior<ScoreManager>
{
    protected override void InitInternal()
    {
        // Initialization code here
    }
}
```

### Event System
Use the `EventManager` static class for decoupled communication:
- Define event classes inheriting from `GameEvent`
- Register listeners: `EventManager.AddListener<MyEvent>(OnMyEvent)`
- Fire events through EventManager (not direct UnityEvents)
- Supports one-shot listeners and event pooling

### Finite State Machine
The project includes an FSM system:
- States inherit from `FiniteState<T>` where T is the owner type
- Use `FiniteStateMachine<T>` for state management
- Implement `IHasFSM<T>` interface on owner classes
- States have `Enter()`, `Update()`, and `Exit()` lifecycle methods

## Common Systems and Utilities

### Logging
- **Always use `DLog.Log()`** instead of `Debug.Log()`
- DLog includes caller info, timestamps, and color coding
- Logging is automatically disabled in release builds

### Custom Attributes
Common custom attributes used in the project:
- `[ToggleHeader]` - Creates a foldout with a toggle
- `[ShowIf]` / `[HideIf]` - Conditional field visibility
- `[ConsoleCommand]` - Exposes methods to dev console
- `[AddComponentMenu("‽/...")]` - Adds to GameObject menu (often commented out)

### Extension Methods
The project has extensive extension methods for Unity types:
- `Vector2/3/4Extensions` - Vector utilities
- `TransformExtensions` - Transform helpers
- `GameObjectExtensions` - GameObject utilities
- `MaterialExtensions`, `RendererExtensions` - Rendering utilities
- `StringExtensions`, `ColorExtensions` - General utilities

### Data Structures
Custom data structures available:
- `RingBuffer<T>` - Circular buffer
- `PriorityQueue<T>` - Priority queue implementation
- `SpatialHash2D/3D` - Spatial partitioning
- `LRUCache<TKey, TValue>` - LRU cache
- `SerializableDictionary<TKey, TValue>` - Dictionary serialization
- `ArrayPool<T>`, `ConcurrentArrayPool<T>` - Object pooling

## Code Organization

### Directory Structure
- `Assets/Scripts/` - Main game scripts
  - `Gameplay/` - Game mechanics (ScoreManager, Health, Weapon, etc.)
  - `FSM/` - Finite State Machine implementation
  - `EventSystem/` - Event system
  - `UI/` - UI-related scripts
  - `AI/` - AI behaviors and pathfinding
  - `DataStructures/` - Custom data structures
  - `Extensions/` - Extension methods
  - `Attributes/` - Custom attributes
  - `Tween/` - Tweening system
  - `Noise/` - Procedural noise generation
- `Assets/Editor/` - Unity editor scripts
- `Assets/Settings/` - URP and project settings

### Namespace Policy
- Most scripts are in the global namespace (no namespace declaration)
- Only use namespaces when absolutely necessary or for third-party integrations

## Common Game Systems

### Score Management
- `ScoreManager` handles scoring, lives, timers, combos, and multipliers
- Uses events for score updates: `OnScoreChanged`, `OnLivesChanged`, `OnTimeChanged`
- Supports high score tracking and combo systems

### Health System
- `Health.cs` provides health management
- Status effects via `StatusEffectSystem`
- Projectiles via `Projectile.cs`

### AI Systems
- `SteeringAgent` and `SteeringBehaviorSystem` for steering behaviors
- Pathfinding systems in `AI/Pathfinding/`
- Targeting strategies via `TargetingStrategy`

### UI System
- `ScreenManager` handles UI screen transitions
- Custom UI components in `UIComponents.cs`
- Uses custom `GUIScope` utilities

### Input
- Uses Unity's Input System (`InputSystem_Actions.inputactions`)
- New Input System patterns preferred over legacy Input

## Important Notes

- **Don't use `Debug.Log()`**: Always use `DLog.Log()` instead
- **Avoid namespaces**: Most code is in global namespace unless necessary
- **Follow underscore convention**: Private fields must have underscore prefix
- **Use modern C#**: Prefer expression-bodied members and modern syntax
- **Unity 6000 features**: Can use modern Unity APIs like `FindFirstObjectByType<T>()`
- **Comment style**: Use `//` for comments, `//todo:` for todos
- **Custom menu items**: Use `‽` character in menu paths: `[MenuItem("GameObject/3D Object/‽/...")]`

## Testing and Validation

When implementing features:
- Consider edge cases and null checks
- Use Unity's null-conditional operators where appropriate
- Ensure proper cleanup in `OnDestroy()` methods
- Check for memory leaks with object pooling and event unsubscription
- Verify Inspector serialization works correctly

## Performance Considerations

- Use object pooling (`ObjectPoolEx`, `ArrayPool`) for frequently instantiated objects
- Prefer `struct` for small, frequently-copied data types
- Use `readonly` and `const` where possible
- Cache component references instead of repeated `GetComponent()` calls
- Use spatial data structures (`SpatialHash`) for spatial queries

---

## Unity Log Locations

### Editor Logs
- **Windows**: `%LOCALAPPDATA%\Unity\Editor\Editor.log`
- **Previous session**: `Editor-prev.log` in same directory

### Player Logs
- **Windows**: `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\Player.log`

### DLog File Output
- Custom logs written to: `Application.persistentDataPath/Logs/`
- Windows: `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\Logs\`

---

## Important Unity Paths

| Path | Purpose | Writable |
|------|---------|----------|
| `Application.persistentDataPath` | Save data, logs, user files | Yes |
| `Application.streamingAssetsPath` | Read-only bundled assets | No (build) |
| `Application.dataPath` | Assets folder (Editor only) | Editor only |
| `Application.temporaryCachePath` | Temporary files | Yes |

---

## Running Tests

### Edit Mode Tests
- Location: `Assets/Tests/Editor/`
- Run via: **Window > General > Test Runner** (Edit Mode tab)
- Tests use `TestBase` helper class for assertions

### Test Categories
- **Core**: Rand, EnumCache, Combinatorics, ReflectionUtils
- **DataStructures**: RingBuffer, PriorityQueue, Deque, LRUCache
- **Extensions**: Int, Float, String
- **Tween**: Ease functions

### Test Helpers
- `TestBase` - Base class with `Eq()`, `True()`, `False()`, `Approx()`, `InRange()`, `Throws<T>()`
- `H` (TestHelpers) - Collection helpers: `H.Seq()`, `H.Contains()`, `H.Count()`, `H.Empty()`

---

## Key Unity Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `com.unity.test-framework` | 1.4.6 | Unit testing |
| `com.unity.inputsystem` | 1.17.0 | New Input System |
| `com.unity.entities` | 1.4.2 | ECS (optional use) |
| `com.unity.logging` | 1.3.10 | Structured logging (not currently integrated) |
| `com.unity.render-pipelines.universal` | 17.3.0 | URP rendering |

---

## Editor vs Runtime Code

### Conditional Compilation
```csharp
#if UNITY_EDITOR
    // Editor-only code (menus, gizmos, inspector extensions)
    UnityEditor.EditorApplication.isPlaying = false;
#endif

#if !UNITY_EDITOR
    // Build-only code
#endif

#if UNITY_INCLUDE_TESTS
    // Test code only
#endif
```

### Editor Scripts Location
- `Assets/Editor/` - Custom inspectors, editor windows, menu items
- Not included in builds

---

## Triggering Recompilation & Checking Errors

### How Unity Recompiles
Unity automatically recompiles scripts when:
1. **Unity regains focus** after external file changes
2. **AssetDatabase.Refresh()** is called from editor code
3. **Files are saved** from within Unity's code editor

### Triggering Recompile from External Tools
Since Unity only auto-refreshes on focus:

**Normal workflow**: Edit a `.cs` file → ask user to switch to Unity → Unity detects changes and recompiles. File edits automatically update modification times.

**Force recompile without code changes** (rare):
```powershell
# Touch asmdef to trigger recompile when Unity gets focus
(Get-Item "Assets/Scripts/GameCode.asmdef").LastWriteTime = Get-Date
```

**Limitation**: Unity must have focus for recompilation to occur. There is no way to force a recompile while Unity is in the background without using batch mode.

### Checking for Compilation Errors

**Read the Unity Editor log and filter for errors:**
```powershell
# Check for recent C# compilation errors (Windows)
Get-Content "$env:LOCALAPPDATA\Unity\Editor\Editor.log" -Tail 200 | Select-String "error CS"

# Check for fresh compilation results
Get-Content "$env:LOCALAPPDATA\Unity\Editor\Editor.log" -Tail 100 | Select-String "##### ExitCode|error CS|Asset Pipeline Refresh"
```

**Exit codes in log:**
- `##### ExitCode\n0` - Compilation succeeded
- `##### ExitCode\n1` - Compilation failed (look for `error CS` lines above)

### Common Assembly Reference Issues
When using `.asmdef` files, ensure correct assembly names:
- Input System: `Unity.InputSystem`
- TextMeshPro: `Unity.TextMeshPro`
- Entities/DOTS: `Unity.Entities`
- Collections: `Unity.Collections`
- Mathematics: `Unity.Mathematics`

Find package assembly names:
```powershell
# List all asmdef files in a package
cmd /c "dir /s /b Library\PackageCache\com.unity.inputsystem*\*.asmdef"
```

---

## Debugging Tips

### Common Debug Commands
- `Debug.Break()` - Pause editor when hit
- `DLog.Time(() => { ... })` - Measure execution time

### Finding Objects at Runtime
```csharp
// Unity 6000+ preferred methods
FindFirstObjectByType<T>()      // Single object
FindObjectsByType<T>(sortMode)  // Multiple objects

// Avoid (deprecated in Unity 6000)
FindObjectOfType<T>()
FindObjectsOfType<T>()
```

### Inspector Debugging
- Click lock icon to keep Inspector on one object
- Right-click component header > Debug to see private fields

---

## Build Configuration

### Project Settings
- Company Name: (check ProjectSettings/ProjectSettings.asset)
- Product Name: DunksJams
- Scripting Backend: IL2CPP recommended for builds

### Scripting Define Symbols
Add via: **Project Settings > Player > Scripting Define Symbols**
- `ENABLE_LOGGING` - Enable DLog in builds (if implemented)

---

**Remember**: When in doubt, search the codebase for similar patterns and match the existing style. Consistency is more important than perfect adherence to external style guides.
