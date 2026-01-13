# Comprehensive Codebase Analysis and Fix Plan

This document provides a thorough analysis of incomplete systems, bugs, refactoring opportunities, and Unity API redundancies in the DunksJams project.

---

## Table of Contents
1. [Critical Bugs](#critical-bugs)
2. [Tween System](#tween-system)
3. [DLog System](#dlog-system)
4. [Node-Based Systems (GraphView)](#node-based-systems-graphview)
5. [Curves System](#curves-system)
6. [Event System](#event-system)
7. [Gameplay Systems](#gameplay-systems)
8. [UI System](#ui-system)
9. [AI and Pathfinding](#ai-and-pathfinding)
10. [Data Structures](#data-structures)
11. [Utility Systems](#utility-systems)
12. [Empty/Stub Systems](#emptystub-systems)
13. [Code Quality Issues](#code-quality-issues)
14. [Unity API Redundancies](#unity-api-redundancies)
15. [Priority Summary](#priority-summary)

---

## Critical Bugs

### 1. WaveformEditorWindow Opens Wrong Window
**File**: `Assets/Editor/GraphView/WaveformGraphView.cs:9`
**Issue**: `ShowWindow()` opens `DialogueEditorWindow` instead of `WaveformEditorWindow`
```csharp
// BUG: Opens wrong window
public static void ShowWindow() => GetWindow<DialogueEditorWindow>();
// SHOULD BE:
public static void ShowWindow() => GetWindow<WaveformEditorWindow>();
```
**Impact**: HIGH - Waveform editor is completely unusable

### 2. EventManager.Update() Never Called
**File**: `Assets/Scripts/EventSystem/EventManager.cs:117`
**Issue**: The `Update()` method exists but is never invoked. Queued events never process.
**Fix**: Create a MonoBehaviour singleton that calls `EventManager.Update()` every frame:
```csharp
public class EventManagerUpdater : SingletonBehavior<EventManagerUpdater>
{
    protected override void InitInternal() => DontDestroyOnLoad(gameObject);
    void Update() => EventManager.Update();
}
```
**Impact**: HIGH - Event queue system is non-functional

### 3. CardCollection.DrawFromTop() Doesn't Remove Card
**File**: `Assets/Scripts/Gameplay/CardGameManager.cs:103`
**Issue**: `DrawFromTop()` returns the last card but doesn't remove it from the collection
```csharp
// CURRENT (broken):
public T DrawFromTop() => _cards.Count > 0 ? _cards[^1] : throw new InvalidOperationException("No cards left.");

// SHOULD BE:
public T DrawFromTop()
{
    if (_cards.Count == 0) throw new InvalidOperationException("No cards left.");
    T card = _cards[^1];
    _cards.RemoveAt(_cards.Count - 1);
    return card;
}
```
**Impact**: HIGH - Card game logic is broken

---

## Tween System

**Location**: `Assets/Scripts/Tween/`

### Issues Found

#### 1. TweenSequence Double Delta Time Calculation
**File**: `TweenSequence.cs:60-62`
```csharp
deltaTime *= TimeScale;
deltaTime = IgnoreTimeScale ? Time.unscaledDeltaTime * TimeScale : deltaTime;
```
**Problem**: When `IgnoreTimeScale` is false, deltaTime is multiplied by TimeScale twice (once passed in, once here). When true, it ignores the already-scaled delta.
**Fix**: Remove the first multiplication or fix the logic.

#### 2. TweenParallel Removes During Iteration
**File**: `TweenParallel.cs:55-60`
```csharp
for (int i = _tweens.Count - 1; i >= 0; --i)
{
    ITween tween = _tweens[i];
    tween.Update(deltaTime);
    if (tween.IsComplete) _tweens.RemoveAt(i);  // Removes from list!
}
```
**Problem**: Removing tweens means they can't be rewound/restarted properly.
**Fix**: Mark complete but don't remove, or use separate completed list.

#### 3. Missing Type Support
**File**: `Tween.cs:183`, `Tweening.cs`
**Missing Types**:
- `Vector2` (noted in TODO)
- `Vector4` (noted in TODO)
- `Rect`
- `RectTransform` (critical for UI)
- `int` (for discrete animations)

#### 4. No RectTransform Extensions
**File**: `TweenExtensions.cs`
**Problem**: Only has Transform extensions. UI tweening requires RectTransform:
```csharp
// Missing:
public static Tween<Vector2> AnchoredPositionTo(this RectTransform rt, Vector2 target, float d, EaseType e)
public static Tween<Vector2> SizeDeltaTo(this RectTransform rt, Vector2 target, float d, EaseType e)
```

#### 5. TweenManager Doesn't Handle Destroyed Objects
**File**: `TweenManager.cs`
**Problem**: If a tweened object is destroyed, the tween throws NullReferenceException.
**Fix**: Add null checks or target validation.

#### 6. No From() Methods
**Problem**: Only To() methods exist. DOTween-style From() is missing.

#### 7. Ease.cs Performance
**File**: `Ease.cs:6`
**TODO**: `[MethodImpl(MethodImplOptions.AggressiveInlining)]` should be added to simple functions.

### Recommended Actions
- [ ] Fix TweenSequence delta time bug
- [ ] Fix TweenParallel removal during iteration
- [ ] Add Vector2, Vector4, Rect, int interpolators
- [ ] Add RectTransform extension methods
- [ ] Add null target handling
- [ ] Add From() methods
- [ ] Add AggressiveInlining to Ease functions

---

## DLog System

**Location**: `Assets/Scripts/DLog.cs`

### CRITICAL BUG: LogW Bypasses IsLoggingEnabled

**File**: `DLog.cs:54-55, 97-106`
```csharp
// LogW calls LogInternal:
public static void LogW(string msg, Object ctx = null, bool timestamp = false) =>
    LogInternal(LogType.Warning, msg, ctx, timestamp);

// LogInternal NEVER checks IsLoggingEnabled:
static void LogInternal(LogType logType, string msg, Object ctx, bool timestamp)
{
    // NO IsLoggingEnabled check here!
    string callerInfo = IsCallerInfoEnabled ? Colorize(GetCallerInfo(), CallerColor) : "";
    ...
}
```
**Problem**: Warnings are ALWAYS logged even when `IsLoggingEnabled = false`. This breaks the entire logging enable/disable system for warnings.
**Impact**: HIGH - Cannot disable warning logs in production builds.

### Parity Issues with Debug.Log

| Feature | Debug.Log | DLog | Status |
|---------|-----------|------|--------|
| Log(object) | ✓ | ✗ | Commented out line 47 |
| Log(object, Object) | ✓ | ✗ | Missing |
| Log(string) | ✓ | ✓ | Works (but string only) |
| LogWarning(object) | ✓ | ✗ | LogW is string only |
| LogWarning(object, Object) | ✓ | ✗ | Missing |
| LogError(object) | ✓ | ✗ | LogE is string only |
| LogError(object, Object) | ✓ | ✗ | Missing |
| LogFormat(string, params) | ✓ | ✗ | Missing |
| LogFormat(LogType, Object, string, params) | ✓ | ✗ | Missing |
| LogAssertion(object) | ✓ | ✗ | Missing |
| LogAssertionFormat | ✓ | ✗ | Missing |
| Assert(bool) | ✓ | ✗ | Missing |
| Assert(bool, string) | ✓ | ✗ | Missing |
| Assert(bool, object) | ✓ | ✗ | Missing |
| AssertFormat | ✓ | ✗ | Missing |
| Break() | ✓ | ✗ | Missing |
| ClearDeveloperConsole() | ✓ | ✗ | Missing |
| LogException | ✓ | ✓ | Works |
| Context object | ✓ | ✓ | Works |
| Conditional compilation | ✓ | ✗ | No #if UNITY_EDITOR support |

### Specific Issues

#### 1. Unused Unity.Logging Import
**Line 6**: `using Unity.Logging;`
**Problem**: Package is imported but never used. Dead code.

#### 2. Inconsistent Internal Logging Methods
```csharp
// Log and LogE call this (HAS IsLoggingEnabled check):
static void Log(LogType logType, string msg, Object ctx, bool timestamp)
{
    if (!IsLoggingEnabled) return;  // Checks here
    ...
}

// But LogW calls this (NO check):
static void LogInternal(LogType logType, string msg, Object ctx, bool timestamp)
{
    // No IsLoggingEnabled check!
    ...
}
```
**Problem**: Two nearly identical methods with different behavior. LogW bypasses logging disable.

#### 3. LogSinks System Unused
**Lines 36-45**: ILogSink interface and ConsoleSink defined, but line 93 is commented out:
```csharp
//foreach (ILogSink sink in LogSinks) sink.Log(logType, msg, context);
```

#### 4. IgnoredMethods List Unused
**Lines 19-24**: `IgnoredMethods` list defined but never referenced in code.

#### 5. Time() Ignores IsLoggingEnabled
**Lines 141-150**: The `Time()` method performs expensive timing and formatting even when logging is disabled:
```csharp
public static void Time(Action action, string label = null)
{
    string timingLabel = label ?? GetCallerInfo();  // Expensive even if logging disabled
    var stopwatch = Stopwatch.StartNew();
    action();
    stopwatch.Stop();
    Log($"{timingLabel} took {stopwatch.ElapsedMilliseconds}ms", timestamp: true);
}
```

#### 6. Missing Features
- No `Log(object message)` overload (most common Unity pattern)
- No conditional logging by category/tag
- No log levels beyond Unity's built-in types
- No file/remote logging (TODO noted line 36)
- No SettingsProvider integration (TODO noted line 13)

#### 7. Performance Issues
- `GetCallerInfo()` creates new StackTrace every call - very expensive
- `Colorize()` allocates strings even when `IsColorEnabled = false` could be checked earlier
- Consider using `[CallerMemberName]`, `[CallerFilePath]`, `[CallerLineNumber]` attributes

#### 8. SaveGraph Uses LogW for Success
**File**: `SerializedGraphView.cs:83`
```csharp
DLog.LogW($"Graph saved to {FilePath}");
```
**Problem**: Success message logged as warning. Should be `DLog.Log()`.

### Recommended Actions
- [ ] **CRITICAL**: Add IsLoggingEnabled check to LogInternal
- [ ] Remove unused `using Unity.Logging;` import
- [ ] Unify Log and LogInternal methods into single implementation
- [ ] Add Log(object) overload
- [ ] Add LogWarning(object) / LogError(object) overloads
- [ ] Add IsLoggingEnabled check to Time() method
- [ ] Enable LogSinks system
- [ ] Remove or use IgnoredMethods list
- [ ] Add LogFormat equivalent
- [ ] Add Assert methods
- [ ] Implement SettingsProvider
- [ ] Add [CallerMemberName] alternative for performance
- [ ] Consider buffering for remote/file sinks
- [ ] Fix SaveGraph to use Log instead of LogW

---

## Node-Based Systems (GraphView)

**Location**: `Assets/Editor/GraphView/`

### CRITICAL Issues

#### 1. Wrong Window Opens (CRITICAL)
**File**: `WaveformGraphView.cs:9`
Already documented above - opens DialogueEditorWindow instead.

#### 2. SerializedGraphNode.PropagateData() Never Works (CRITICAL)
**File**: `SerializedGraphNode.cs:23-30`
```csharp
public virtual void PropagateData()
{
    foreach (var output in outputContainer.Children().OfType<DataPort<object>>())
    {
        foreach (Edge edge in output.connections)
            (edge.input as IDataPort<object>)?.SetData(output.GetData());
    }
}
```
**Problem**: Casts to `DataPort<object>` which NEVER matches `DataPort<AnimationCurve>` or `DataPort<string>`!
Generic types are invariant in C# - `DataPort<AnimationCurve>` is NOT a `DataPort<object>`.
**Impact**: CRITICAL - Base PropagateData() never propagates any data. Only works if subclass overrides.

#### 3. WaveformGraphView.OnGraphViewChanged Same Bug
**File**: `WaveformGraphView.cs:24-29`
```csharp
if (e.output is not IDataPort<object> outPort) continue;
var data = outPort.GetData();
(e.input as IDataPort<object>)?.SetData(data);
```
**Problem**: Same generic type mismatch. `DataPort<AnimationCurve>` is NOT `IDataPort<object>`.
**Impact**: CRITICAL - Edge data propagation never works.

#### 4. InstantiateNode Never Calls Init() (CRITICAL)
**File**: `SerializedGraphView.cs:121-149`
```csharp
protected void InstantiateNode(NodeData nd)
{
    Node n = CreateNodeFromType(nd.nodeType);
    ...
    n.SetPosition(new Rect(nd.pos, SerializedGraphNode.DefaultSize));
    _nodes[n.viewDataKey] = n;
    ...
    AddElement(n);
    // NEVER calls n.Init()!
}
```
**Problem**: Nodes loaded from file never have `Init()` called.
**Impact**: CRITICAL - Loaded nodes are broken:
- AudioOutputNode: No AudioSource created
- SineWaveNode: No IMGUIContainer, no curve generated
- AnimationCurveNode: No UI, no curve
- All nodes: No ports created, no title set

#### 5. Ports Added Twice (Duplication Bug)
**File**: `WaveformGraphNodes.cs:56-57, 81-82, 141-142`
```csharp
// AnimationCurveNode.Init():
_outPort = CreatePort<AnimationCurve>("Out", Direction.Output);
outputContainer.Add(_outPort);  // CreatePort already added it!

// Same bug in SineWaveNode and AudioOutputNode
```
**Problem**: `CreatePort()` already adds the port to the container (lines 39-42 of SerializedGraphNode), then these nodes add it again.
**Impact**: MEDIUM - Ports appear duplicated in UI.

### HIGH Issues

#### 6. AudioOutputNode Destructor Issues
**File**: `WaveformGraphNodes.cs:122-125`
```csharp
~AudioOutputNode()
{
    if (_audioSource != null) Object.DestroyImmediate(_audioSource.gameObject);
}
```
**Problems**:
- Destructor in Unity is unreliable (finalizer thread)
- DestroyImmediate in finalizer is dangerous
- Can cause crashes
**Fix**: Implement proper cleanup via OnDestroy or disposal pattern.

#### 7. Type Resolution Fails
**File**: `SerializedGraphView.cs:152-158`
```csharp
var type = Type.GetType(typeName);
```
**Problem**: `Type.GetType()` without assembly-qualified name won't find types in different assemblies.
**Fix**: Search assemblies or store full type name.

#### 8. AnimationCurve Serialization Loss
**File**: `SerializedGraphView.cs:108`
```csharp
nd.fields.Add(new() { name = fi.Name, val = fi.GetValue(n)?.ToString() ?? "" });
```
**Problem**: AnimationCurve.ToString() doesn't serialize curve data. Curves are lost on save/load.
**Fix**: Use JsonUtility for complex types or custom serialization.

### MEDIUM Issues

#### 9. No Undo/Redo Support
**Problem**: Graph modifications don't register with Unity's undo system.
**Fix**: Use `Undo.RecordObject()` before changes.

#### 10. Data Propagation Not Automatic
**Problem**: Data only propagates when edges are created, not when source data changes.

#### 11. Inconsistent Init() Call Order
**Problem**: Some nodes call `base.Init()` first (DialogueNodeChoice), others call it last (AnimationCurveNode, SineWaveNode, AudioOutputNode).
**Impact**: May cause subtle bugs depending on what base.Init() does.

#### 12. DataPort.SetData Logs Every Time
**File**: `SerializedGraphNode.cs:69-73`
```csharp
public void SetData(T data)
{
    _data = data;
    DLog.Log($"{portName} received data: {data}");
}
```
**Problem**: Logs on every data set - noisy and potential performance issue.

### Node Types Analysis

| Node | Status | Issues |
|------|--------|--------|
| DialogueNode | Basic | PropagateData() is empty |
| DialogueNodeChoice | Partial | Works if manually propagated |
| AnimationCurveNode | Broken | Port duplicated, curve lost on reload, PropagateData fails |
| SineWaveNode | Broken | Port duplicated, Init not called on load, PropagateData fails |
| AudioOutputNode | Broken | Port duplicated, destructor issues, PropagateData fails |

### Recommended Actions
- [ ] **CRITICAL**: Fix PropagateData() generic type mismatch - use non-generic interface or reflection
- [ ] **CRITICAL**: Fix OnGraphViewChanged generic type mismatch
- [ ] **CRITICAL**: Call Init() on loaded nodes in InstantiateNode()
- [ ] **CRITICAL**: Remove duplicate port additions from node Init() methods
- [ ] Fix WaveformEditorWindow.ShowWindow()
- [ ] Replace AudioOutputNode destructor with proper cleanup
- [ ] Fix type resolution to search assemblies
- [ ] Implement proper AnimationCurve serialization
- [ ] Add undo/redo support
- [ ] Implement continuous data propagation
- [ ] Add cleanup interface for nodes
- [ ] Standardize Init() call order
- [ ] Remove or make optional the SetData logging

---

## Curves System

**Location**: `Assets/Curves/`

### Performance Issues

#### 1. TransformCurve Uses Reflection Every Frame
**File**: `Scripts/TransformCurve.cs:76`
```csharp
CurveTarget.SetValue(transform, interpolatedValue, null);
```
**Problem**: PropertyInfo.SetValue uses reflection, called every Update().
**Fix**: Cache delegate or use expression trees:
```csharp
Action<Transform, Vector3> _cachedSetter;
void Awake()
{
    var prop = typeof(Transform).GetProperty(curveTargetName);
    _cachedSetter = (Action<Transform, Vector3>)Delegate.CreateDelegate(
        typeof(Action<Transform, Vector3>), prop.GetSetMethod());
}
```

#### 2. ComponentMemberReference Reflection Every Frame
**File**: `Scripts/ComponentMemberReference.cs:52-85`
**Problem**: SetValue() uses reflection every call.
**Fix**: Cache delegates after first CacheInfo().

#### 3. NormalizedAnimationCurveDrawer Preset Loading
**File**: `Editor/NormalizedAnimationCurveDrawer.cs:84-91`
**Problem**: Loads presets only on script reload, not on asset database refresh.
**Fix**: Subscribe to AssetDatabase callbacks.

### Incomplete Features

#### 1. Relative Mode Not Implemented
**File**: `Scripts/ComponentMemberReferenceCurve.cs:13-14, 50-53, 72-74`
```csharp
//[SerializeField] public bool relativeMode;
//curveOffset - how to do, one for each type? :(
```
All relative mode code is commented out.

#### 2. Hardcoded Paths
**File**: `Scripts/CurveConstants.cs`
```csharp
public static string NormalizedCurvesPath = "/Curves/Editor/CurvesNormalized.curvesNormalized";
```
**Problem**: Paths are hardcoded, will break if folder structure changes.

### Recommended Actions
- [ ] Cache reflection delegates in TransformCurve
- [ ] Cache reflection delegates in ComponentMemberReference
- [ ] Subscribe to AssetDatabase.importPackageCompleted
- [ ] Implement relative mode
- [ ] Use AssetDatabase.FindAssets for curve paths

---

## Event System

**Location**: `Assets/Scripts/EventSystem/`

### Issues

#### 1. Update Never Called (CRITICAL)
Already documented above.

#### 2. Uses Debug.LogError
**File**: `EventManager.cs:103`
```csharp
Debug.LogError($"Error in event listener: {ex}");
```
Should use `DLog.LogE()`.

#### 3. No Event Prioritization
**Problem**: Events process in FIFO order, no priority system.

#### 4. No Event Cancellation
**Problem**: Can't cancel an event mid-propagation.

#### 5. Events.cs Has Only Example
**File**: `Events.cs`
Only contains `PlayerDeathEvent` example. No real game events defined.

### Recommended Actions
- [ ] Create EventManagerUpdater MonoBehaviour
- [ ] Replace Debug.LogError with DLog.LogE
- [ ] Add priority system (optional)
- [ ] Add event cancellation support (optional)
- [ ] Define actual game events

---

## Gameplay Systems

**Location**: `Assets/Scripts/Gameplay/`

All marked "largely untested":

| System | File | Status | Key Issues |
|--------|------|--------|------------|
| ScoreManager | ScoreManager.cs | Untested | Not using SingletonBehavior |
| Health | Health.cs | Untested | Duplicate StatusEffectInstance class |
| Weapon | Weapon.cs | Untested | Uses Debug.Log (3 places) |
| Projectile | Projectile.cs | Untested | No object pooling |
| StatusEffectSystem | StatusEffectSystem.cs | Untested | Commented MovementController code |
| WaveManager | WaveManager.cs | Untested | Uses Debug.Log, commented code |
| ObjectiveManager | ObjectiveManager.cs | Untested | Not a MonoBehaviour |
| Inventory | Inventory.cs | Untested | Capacity checks items not slots |
| SaveSystem | SaveSystem.cs | Untested | Uses Debug.Log for errors |
| CardGameManager | CardGameManager.cs | Broken | DrawFromTop bug, Debug.Log usage |

### Specific Issues

#### Health.cs Duplicate Class
**Lines 54-59**: Defines own `StatusEffectInstance` class, but `StatusEffectSystem.cs` already has one with more features (stacking, intensity, combining).

#### Inventory Capacity Bug
**File**: `Inventory.cs:18`
```csharp
if (quantity <= 0 || ItemCount >= Capacity) return false;
```
**Problem**: `ItemCount` counts unique items, not total quantity. Capacity should check total items.

#### WaveManager Commented Code
**Lines 103-104**: MovementController integration commented out.

### Recommended Actions
- [ ] Test all gameplay systems
- [ ] Fix CardGameManager.DrawFromTop()
- [ ] Remove duplicate StatusEffectInstance from Health.cs
- [ ] Fix Inventory capacity logic
- [ ] Add object pooling to Projectile
- [ ] Make ScoreManager use SingletonBehavior
- [ ] Replace all Debug.Log with DLog

---

## UI System

**Location**: `Assets/Scripts/UI/`

### Issues

#### 1. Screens.cs Uses Debug.Log
**File**: `Screens.cs:21-22, 31`
```csharp
UIBuilder.CreateButton(Panel.transform, "Play", () => Debug.Log("Play game"));
```
Placeholder buttons log to Debug instead of doing anything.

#### 2. ScreenManager Not Singleton
**Problem**: ScreenManager is a regular MonoBehaviour, not a singleton.

#### 3. UIComponents.CreateDialog Font Issue
**File**: `UIComponents.cs:21`
```csharp
UIBuilder.InitText(..., font: null, ...);
```
Passes null font, relies on default.

### Recommended Actions
- [ ] Replace Debug.Log with actual navigation
- [ ] Consider making ScreenManager a singleton
- [ ] Handle null font explicitly

---

## AI and Pathfinding

### Steering Behaviors
**Location**: `Assets/AI/`

#### Nested Class Issue
**File**: `SteeringBehaviorSystem.cs:166-182`
```csharp
public class Separation : NeighborBasedBehavior
{
    // ...
    [CreateAssetMenu(menuName = "Steering/Idle")]
    public class Idle : SteeringBehavior { }
    
    [CreateAssetMenu(menuName = "Steering/Align")]
    public class Align : SteeringBehavior { }
}
```
**Problem**: Idle and Align are nested inside Separation - likely a copy-paste error.

### Pathfinding
**Location**: `Assets/AI/Pathfinding/`

#### FlowFieldPathfinder2D TODO
**File**: `FlowFieldPathFinder2D.cs:69`
```csharp
//todo: adjust initial size, also consider renting an array
var path = new List<Vector2Int>(32) { start };
```

### Recommended Actions
- [ ] Move Idle and Align classes outside Separation
- [ ] Implement array pooling for pathfinding

---

## Data Structures

**Location**: `Assets/Scripts/DataStructures/`

### Status

| Structure | Status | Notes |
|-----------|--------|-------|
| RingBuffer | Works | Uses Debug.Assert |
| PriorityQueue | Works | Custom implementation |
| SpatialHash2D | Works | Good implementation |
| SpatialHash3D | Unknown | Not reviewed |
| LRUCache | Unknown | Not reviewed |
| SerializableDictionary | Works | Used by Health.cs |
| ArrayPool | Works | Uses Debug.Assert |
| ConcurrentArrayPool | Works | Uses Debug.Assert |
| Dequeue | Works | Uses Debug.Assert |
| MinimumQueue | Unknown | Not reviewed |

### Note on Debug.Assert
Multiple data structures use `Debug.Assert` which only runs in editor. Consider:
- Keep as-is for debug builds
- Or add runtime validation for critical checks

---

## Utility Systems

### LocalizationManager
**File**: `Assets/Scripts/LocalizationManager.cs`
**Issue**: Hardcoded `<SHEET_ID>` placeholder (lines 35, 43)

### DataUtils
**File**: `Assets/Scripts/DataUtils.cs`
**Issues**:
- Uses Debug.LogError (lines 56, 60)
- Hardcoded fallback language "en"

### OptionsManager
**File**: `Assets/Scripts/OptionsManager.cs`
**Status**: Complete, well-structured

### HapticsManager
**File**: `Assets/Scripts/HapticsManager.cs`
**TODO**: Line 7 notes editor and visualizer needed

---

## Empty/Stub Systems

### Completely Empty Files

| File | Purpose | Action Needed |
|------|---------|---------------|
| `AudioSystem.cs` | Audio management | Implement or delete |
| `SaveSystem/SaveManager.cs` | Save management | Implement or delete (SaveSystem.cs exists separately) |

### Near-Empty Files
- `Scripts/Dialogue/` folder exists but is empty

---

## Code Quality Issues

### Debug.Log Usage (Should Use DLog)

| File | Line(s) | Count |
|------|---------|-------|
| Weapon.cs | 62, 175, 185 | 3 |
| SaveSystem.cs | 30, 50 | 2 |
| CardGameManager.cs | 79, 132 | 2 |
| DataUtils.cs | 56, 60 | 2 |
| ReflectionUtils.cs | 279 | 1 |
| WaveManager.cs | 54 | 1 |
| Screens.cs | 21, 22, 31 | 3 |
| Health.cs | 112 | 1 |
| DevConsole.cs | 99 | 1 |
| EventManager.cs | 103 | 1 |
| **Total** | | **17** |

### Naming Convention Violations
- `WaveManager._currentWaveIndex` uses `private` keyword explicitly (inconsistent)
- Some private fields missing underscore prefix

### Code Style Issues
- `CardGameManager.cs:52` - Methods run together on one line
- Inconsistent use of expression-bodied members

---

## Unity API Redundancies

### Consider Using Unity Built-ins

| Custom System | Unity Alternative | Recommendation |
|---------------|-------------------|----------------|
| PriorityQueue | System.Collections.Generic.PriorityQueue (.NET 6+) | Keep custom (Unity may not have it) |
| Tween System | DOTween (paid), LeanTween, Unity Animation | Keep custom for simple cases |
| Event System | UnityEvents, C# events | Keep - provides pooling |
| FSM | Animator StateMachine | Keep - for logic, not animation |
| Noise generators | Unity.Mathematics.noise | Evaluate switching |

### Deprecated APIs
None found - code uses modern Unity 6000 APIs.

---

## Priority Summary

### CRITICAL (Fix Immediately)
1. WaveformEditorWindow opens wrong window
2. EventManager.Update() never called
3. CardCollection.DrawFromTop() doesn't remove card

### HIGH (Fix Soon)
4. TweenSequence double delta time calculation
5. AudioOutputNode destructor issues
6. TransformCurve reflection performance
7. Test all gameplay systems
8. Implement or delete empty files (AudioSystem, SaveManager)

### MEDIUM (Scheduled Work)
9. Complete DLog parity with Debug.Log
10. Add Vector2/RectTransform tween support
11. Fix GraphView type resolution
12. Fix AnimationCurve serialization
13. Replace all Debug.Log with DLog (17 instances)
14. Add proper Curves performance caching

### LOW (Nice to Have)
15. Add undo/redo to GraphView
16. Implement HapticsManager editor
17. Add LogSinks implementation
18. Add SettingsProvider for DLog
19. Add AggressiveInlining to Ease functions
20. Complete relative mode for Curves

---

## Implementation Checklist

### Phase 1: Critical Fixes
- [ ] Fix WaveformEditorWindow.ShowWindow()
- [ ] Create EventManagerUpdater MonoBehaviour
- [ ] Fix CardCollection.DrawFromTop()

### Phase 2: Core System Fixes
- [ ] Fix TweenSequence delta time
- [ ] Replace AudioOutputNode destructor
- [ ] Cache reflection in TransformCurve
- [ ] Fix Health.cs duplicate StatusEffectInstance

### Phase 3: Feature Completion
- [ ] Add Vector2 tween support
- [ ] Add RectTransform extensions
- [ ] Implement AnimationCurve serialization
- [ ] Add DLog.Log(object) overload

### Phase 4: Code Quality
- [ ] Replace 17 Debug.Log instances with DLog
- [ ] Add tests for gameplay systems
- [ ] Delete or implement empty files
- [ ] Fix nested class issues in SteeringBehaviorSystem

### Phase 5: Polish
- [ ] Add undo/redo to GraphView
- [ ] Implement LogSinks
- [ ] Add SettingsProvider
- [ ] Performance optimizations

---

## Additional Bugs Found (Second Pass)

### CRITICAL

#### 4. Rand.Shuffle() Corrupts Data
**File**: `Assets/Scripts/Rand.cs:92-93`
```csharp
for (int i = list.Count - 1; i > 0; --i)
    (list[i], list[IntRanged(0, i + 1)]) = (list[IntRanged(0, i + 1)], list[i]);
```
**Problem**: Fisher-Yates algorithm calls `IntRanged()` TWICE in the tuple swap - once for each side. This generates TWO different random indices, corrupting the shuffle completely.
**Fix**:
```csharp
for (int i = list.Count - 1; i > 0; --i)
{
    int j = IntRanged(0, i + 1);
    (list[i], list[j]) = (list[j], list[i]);
}
```
**Impact**: CRITICAL - Any shuffled data is corrupted. Affects card games, procedural generation, etc.

#### 5. EnumCache Stores Null Strings
**File**: `Assets/Scripts/EnumCache.cs:22-23`
```csharp
var valString = Strings[i];  // Strings[i] is null here!
Cache[val] = valString;      // Caches null
```
**Problem**: Reads from `Strings[i]` which was just allocated as `new string[Values.Length]` (all nulls). Never assigns the actual string value.
**Fix**:
```csharp
var valString = Values[i].ToString();
Strings[i] = valString;
Cache[val] = valString;
```
**Impact**: HIGH - EnumCache returns null for all enum values.

#### 6. ReflectionUtils.SetValue Always Throws
**File**: `Assets/Scripts/ReflectionUtils.cs:166-170`
```csharp
public static void SetValue(this MemberInfo mi, object inst, object val)
{
    if (mi is PropertyInfo prop) prop.SetValue(inst, val);
    else if (mi is FieldInfo field) field.SetValue(inst, val);
    throw new InvalidOperationException("Member is not a property or field.");
}
```
**Problem**: Missing `return` statements - always throws exception even on success.
**Fix**: Add `return;` after each SetValue call.

### HIGH

#### 7. AlignDistributeSnapWindow Crashes When Nothing Selected
**File**: `Assets/Editor/AlignDistributeSnapWindow.cs:42`
```csharp
var pos = Selection.transforms[0].position;
```
**Problem**: No null check. Crashes if user clicks Align with nothing selected.

#### 8. SimplexNoise Static Init Uses Uninitialized Random
**File**: `Assets/Scripts/Noise/SimplexNoise.cs:12`
```csharp
static SimplexNoise() => InitializePerm();
// InitializePerm() calls Rand.Shuffle(p);
```
**Problem**: Static constructor runs before `Rand.SetSeed()` is called. Noise results are non-deterministic across sessions.

#### 9. WorleyNoise Uses Wrong Random Class
**File**: `Assets/Scripts/Noise/WorleyNoise.cs:10-14`
```csharp
Random.InitState(seed);
precomputedOffsets[i, j] = new(Random.value, Random.value);
```
**Problem**: Uses `UnityEngine.Random` instead of project's `Rand` class. Inconsistent with codebase conventions and may interfere with other Random usage.

#### 10. TextEffects Uses Wrong Index for Character Animation
**File**: `Assets/Scripts/TextEffects.cs:56-64`
```csharp
StartBounceFade(i);  // i is index into _originalText
// But StartBounceFade uses it as index into textInfo.characterInfo
```
**Problem**: `i` is the index into `_originalText` string, but it's used to access `textInfo.characterInfo[idx]`. After tag parsing, these indices don't align.

### MEDIUM

#### 11. TweenManager.GetByTag Allocates Every Call
**File**: `Assets/Scripts/Tween/TweenManager.cs:59`
```csharp
public List<ITween> GetByTag(string tag) => _tweens.Where(t => t.Tag == tag).ToList();
```
**Problem**: Allocates new list every call. High-frequency calls cause GC pressure.
**Fix**: Add optional `List<ITween> result = null` parameter like `SpatialHash2D.Query()`.

#### 12. AsyncUtils.CompletedTask is Redundant
**File**: `Assets/Scripts/AsyncUtils.cs:12-14`
```csharp
static readonly Task _completedTask = Task.FromResult(true);
public static Task CompletedTask => _completedTask;
```
**Problem**: .NET already has `Task.CompletedTask`. This is unnecessary.

#### 13. DevConsole Uses Debug.Log
**File**: `Assets/Scripts/DevConsole.cs:99`
```csharp
Debug.Log($"NavigateCommandHistory: direction={direction}...");
```
Should use `DLog.Log()`.

#### 14. A* Pathfinding Uses Global Static State
**File**: `Assets/AI/Pathfinding/AStar2D.cs:7-9`
```csharp
static readonly PriorityQueue<AStarNode> GlobalOpenSet = new();
static readonly HashSet<Vector2Int> GlobalClosedSet = new();
static readonly Dictionary<Vector2Int, AStarNode> GlobalOpenSetLookup = new();
```
**Problem**: Global static state means only one pathfinding operation can run at a time. Not thread-safe, will break if used concurrently.

#### 15. DebugUtils.cs is Empty
**File**: `Assets/Scripts/DebugUtils.cs`
```csharp
public static class DebugUtils
{
}
```
Empty class - should implement or delete.

---

## Unity 6000 / Package Redundancies

Based on Unity 6000.3.3f1 and installed packages:

### Installed Packages Analysis

| Package | Version | Custom Code Overlap | Recommendation |
|---------|---------|---------------------|----------------|
| `com.unity.logging` | 1.3.10 | DLog | **Integrate** - DLog should leverage Unity.Logging for sinks |
| `com.unity.inputsystem` | 1.17.0 | HapticsManager | Already using correctly |
| `com.unity.entities` | 1.4.2 | None | Could use for high-perf systems |
| `com.unity.ugui` | 2.0.0 | UIBuilder | Keep custom - UIBuilder is convenience layer |

### .NET 6+ / C# 10+ Redundancies

| Custom Implementation | Built-in Alternative | Action |
|----------------------|---------------------|--------|
| `PriorityQueue<T>` | `System.Collections.Generic.PriorityQueue<TElement, TPriority>` | **Evaluate** - Built-in has different API (priority separate from element) |
| `AsyncUtils.CompletedTask` | `Task.CompletedTask` | **Replace** - Use built-in |
| `ArrayPool<T>` | `System.Buffers.ArrayPool<T>` | **Keep** - Custom has different bucket sizes |
| `ObjectPoolEx<T>` | `UnityEngine.Pool.ObjectPool<T>` | **Keep** - Already wraps Unity's pool |

### Unity API Redundancies

| Custom Feature | Unity Built-in | Notes |
|---------------|----------------|-------|
| Custom noise (Perlin, Simplex) | `Unity.Mathematics.noise` | Built-in is SIMD-optimized, Burst-compatible |
| `Rand` class | `Unity.Mathematics.Random` | Built-in is Burst-compatible, seedable per-instance |
| `TransformCurve` | `Animation` / `Animator` | Built-in supports curves on any animatable property |
| Custom FSM | `Animator` states | Keep custom - for logic, not just animation |
| `SpatialHash2D/3D` | Physics overlap queries | Keep custom - more flexible for custom data |

### Unity.Logging Integration Opportunity

The project includes `com.unity.logging 1.3.10` but DLog doesn't use it. Unity.Logging provides:
- Structured logging
- Multiple sinks (file, console, etc.)
- Log levels
- Performance optimized

**Recommendation**: Refactor DLog to use Unity.Logging as backend while keeping DLog's API.

---

## Future Feature Ideas

### Tween System Enhancements

| Feature | Description | Priority |
|---------|-------------|----------|
| From() methods | Start from target, tween to current value | High |
| Punch/Shake | DOTween-style punch position/rotation/scale | Medium |
| Path tweening | Tween along bezier/spline path | Medium |
| Visual debugger | Editor window showing active tweens | Low |
| Sequence callbacks | OnSequenceStart, OnSequenceComplete | Medium |
| Tween pooling | Pool Tween objects to reduce GC | High |

### Event System Enhancements

| Feature | Description | Priority |
|---------|-------------|----------|
| Event statistics | Track event frequency, listener count | Medium |
| Event visualization | Editor window showing event flow | Low |
| Typed event bus | Strongly-typed event channels | Medium |
| Network events | Serialize/deserialize events for multiplayer | Low |

### DLog Enhancements

| Feature | Description | Priority |
|---------|-------------|----------|
| Analytics sink | Send logs to analytics backend | Medium |
| Remote logging | Send logs to remote server | Low |
| Log categories | Filter by category (AI, Physics, UI, etc.) | High |
| Log file rotation | Auto-rotate log files by size/date | Medium |
| Rich console | Hyperlinks to code, collapsible stack traces | Medium |
| Performance mode | Disable expensive caller info in builds | High |

### GraphView Enhancements

| Feature | Description | Priority |
|---------|-------------|----------|
| Undo/redo | Full undo support for all operations | High |
| Copy/paste | Copy nodes between graphs | Medium |
| Node templates | Save/load node presets | Low |
| Minimap | Overview of large graphs | Low |
| Search | Find nodes by name/type | Medium |
| Comments | Add comment nodes to graphs | Low |
| Subgraphs | Group nodes into reusable subgraphs | Medium |

### AI Enhancements

| Feature | Description | Priority |
|---------|-------------|----------|
| 3D pathfinding | A*, Flow Field, D* Lite for 3D grids | Medium |
| NavMesh integration | Use Unity NavMesh with custom behaviors | High |
| Behavior Trees | Visual behavior tree editor | Medium |
| GOAP | Goal-Oriented Action Planning | Low |
| Utility AI | Score-based decision making | Medium |
| Flocking 3D | Full 3D flocking behaviors | Low |

### General System Ideas

| Feature | Description | Priority |
|---------|-------------|----------|
| Unit test framework | Integration with Unity Test Framework | High |
| Performance profiler | Custom profiler markers for all systems | Medium |
| Asset validation | Editor tool to validate asset references | Medium |
| Build pipeline | Automated build scripts | Low |
| Code generation | T4 templates for boilerplate | Low |
| Live reload | Hot reload of ScriptableObjects | Medium |

### Gameplay System Ideas

| Feature | Description | Priority |
|---------|-------------|----------|
| Quest system | Quest definition, tracking, completion | Medium |
| Achievement system | Track and unlock achievements | Low |
| Dialogue system | Visual dialogue editor with branching | Medium |
| Crafting system | Recipe-based item crafting | Low |
| Skill tree | Node-based skill progression | Medium |
| Procedural generation | Dungeon/level generation tools | Medium |

---

## Updated Priority Summary

### CRITICAL (Fix Immediately)
1. WaveformEditorWindow opens wrong window
2. EventManager.Update() never called
3. CardCollection.DrawFromTop() doesn't remove card
4. Rand.Shuffle() corrupts data (calls IntRanged twice)
5. EnumCache stores null strings
6. ReflectionUtils.SetValue always throws
7. **NEW (DLog)**: LogW bypasses IsLoggingEnabled - warnings always logged
8. **NEW (GraphView)**: SerializedGraphNode.PropagateData() generic type mismatch - never works
9. **NEW (GraphView)**: WaveformGraphView.OnGraphViewChanged same generic bug
10. **NEW (GraphView)**: InstantiateNode never calls Init() - loaded nodes are broken
11. **NEW (GraphView)**: Ports added twice in waveform nodes

### HIGH (Fix Soon)
12. TweenSequence double delta time calculation
13. AudioOutputNode destructor issues
14. TransformCurve reflection performance
15. Test all gameplay systems
16. Implement or delete empty files
17. AlignDistributeSnapWindow crashes when nothing selected
18. SimplexNoise uses uninitialized random
19. WorleyNoise uses wrong Random class
20. **NEW (DLog)**: Unity.Logging imported but never used
21. **NEW (DLog)**: Time() method ignores IsLoggingEnabled

### MEDIUM (Scheduled Work)
22. Complete DLog parity with Debug.Log (many missing overloads)
23. Add Vector2/RectTransform tween support
24. Fix GraphView type resolution
25. Fix AnimationCurve serialization
26. Replace all Debug.Log with DLog (17+ instances)
27. Add proper Curves performance caching
28. TextEffects character index mismatch
29. TweenManager.GetByTag allocates every call
30. A* uses global static state
31. **NEW (GraphView)**: DataPort.SetData logs on every call - noisy
32. **NEW (GraphView)**: Inconsistent Init() call order in nodes

### LOW (Nice to Have)
33. Add undo/redo to GraphView
34. Implement HapticsManager editor
35. Add LogSinks implementation
36. Add SettingsProvider for DLog
37. Add AggressiveInlining to Ease functions
38. Complete relative mode for Curves
39. Remove AsyncUtils.CompletedTask (use built-in)
40. Evaluate Unity.Mathematics.noise integration
41. Integrate Unity.Logging with DLog

---

## Updated Implementation Checklist

### Phase 0: Critical Bug Fixes
- [ ] Fix Rand.Shuffle() - store random index in variable
- [ ] Fix EnumCache - assign string values properly
- [ ] Fix ReflectionUtils.SetValue - add return statements
- [ ] **NEW**: Fix DLog.LogInternal to check IsLoggingEnabled
- [ ] **NEW**: Fix SerializedGraphNode.PropagateData() generic type issue
- [ ] **NEW**: Fix WaveformGraphView.OnGraphViewChanged generic type issue
- [ ] **NEW**: Call Init() on loaded nodes in InstantiateNode()
- [ ] **NEW**: Remove duplicate port additions in waveform nodes

### Phase 1: Critical Fixes
- [ ] Fix WaveformEditorWindow.ShowWindow()
- [ ] Create EventManagerUpdater MonoBehaviour
- [ ] Fix CardCollection.DrawFromTop()

### Phase 2: Core System Fixes
- [ ] Fix TweenSequence delta time
- [ ] Replace AudioOutputNode destructor
- [ ] Cache reflection in TransformCurve
- [ ] Fix Health.cs duplicate StatusEffectInstance
- [ ] Fix AlignDistributeSnapWindow null check
- [ ] Fix noise generators random usage
- [ ] Remove unused Unity.Logging import from DLog
- [ ] Add IsLoggingEnabled check to Time() method

### Phase 3: Feature Completion
- [ ] Add Vector2 tween support
- [ ] Add RectTransform extensions
- [ ] Implement AnimationCurve serialization for GraphView
- [ ] Add DLog.Log(object) overload
- [ ] Add DLog.LogWarning(object) / LogError(object) overloads
- [ ] Fix TextEffects character indexing

### Phase 4: Code Quality
- [ ] Replace 17+ Debug.Log instances with DLog
- [ ] Add tests for gameplay systems
- [ ] Delete or implement empty files (AudioSystem, SaveManager, DebugUtils)
- [ ] Fix nested class issues in SteeringBehaviorSystem
- [ ] Remove AsyncUtils.CompletedTask redundancy
- [ ] Unify DLog.Log and DLog.LogInternal methods
- [ ] Make DataPort.SetData logging optional or remove
- [ ] Standardize Init() call order in graph nodes

### Phase 5: Polish & Integration
- [ ] Add undo/redo to GraphView
- [ ] Implement LogSinks
- [ ] Add SettingsProvider
- [ ] Evaluate Unity.Mathematics.noise integration
- [ ] Integrate Unity.Logging as DLog backend
- [ ] Performance optimizations (DLog caller info, etc.)

### Phase 6: Future Features (Prioritized)
- [ ] Tween From() methods
- [ ] Tween object pooling
- [ ] DLog log categories
- [ ] DLog performance mode
- [ ] AI NavMesh integration
- [ ] Unit test framework integration

---

## Summary Statistics

| Category | Count |
|----------|-------|
| Critical Bugs | 11 |
| High Priority Issues | 10 |
| Medium Priority Issues | 11 |
| Low Priority Issues | 9 |
| **Total Issues** | **41** |

---

*Document generated: 2024*
*Project: DunksJams*
*Unity Version: 6000.3.3f1*
*Last updated: Deep analysis of DLog and GraphView systems*
