# Instructions for AI Coding Assistants

## Project Overview

**Project Name**: DunksJams  
**Type**: Unity Game Development Project  
**Engine Version**: Unity (Universal Render Pipeline)

This is a Unity game development project with custom systems and patterns. When assisting with code, follow the established conventions and patterns found in this codebase.

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
- **Constants**: camelCase with const (`const float _fontSize = 12`)
- **Events**: PascalCase with `On` prefix (`OnScoreChanged`, `OnGameOver`)

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

## Important Notes

- **Don't use `Debug.Log()`**: Always use `DLog.Log()` instead
- **Avoid namespaces**: Most code is in global namespace unless necessary
- **Follow underscore convention**: Private fields must have underscore prefix
- **Use modern C#**: Prefer expression-bodied members and modern syntax
- **Unity 2021+ features**: Can use modern Unity APIs like `FindFirstObjectByType<T>()`
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

**Remember**: When in doubt, search the codebase for similar patterns and match the existing style. Consistency is more important than perfect adherence to external style guides.
