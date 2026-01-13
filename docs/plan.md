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

### 1. EventManager.Update() Never Called
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

### 2. CardCollection.DrawFromTop() Doesn't Remove Card
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

#### 1. TweenParallel Removes During Iteration
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

#### 2. Missing Type Support
**File**: `Tween.cs:183`, `Tweening.cs`
**Missing Types**:
- `Vector2` (noted in TODO)
- `Vector4` (noted in TODO)
- `Rect`
- `RectTransform` (critical for UI)
- `int` (for discrete animations)

#### 3. No RectTransform Extensions
**File**: `TweenExtensions.cs`
**Problem**: Only has Transform extensions. UI tweening requires RectTransform:
```csharp
// Missing:
public static Tween<Vector2> AnchoredPositionTo(this RectTransform rt, Vector2 target, float d, EaseType e)
public static Tween<Vector2> SizeDeltaTo(this RectTransform rt, Vector2 target, float d, EaseType e)
```

#### 4. TweenManager Doesn't Handle Destroyed Objects
**File**: `TweenManager.cs`
**Problem**: If a tweened object is destroyed, the tween throws NullReferenceException.
**Fix**: Add null checks or target validation.

#### 5. No From() Methods
**Problem**: Only To() methods exist. DOTween-style From() is missing.

#### 6. Ease.cs Performance
**File**: `Ease.cs:6`
**TODO**: `[MethodImpl(MethodImplOptions.AggressiveInlining)]` should be added to simple functions.

### Recommended Actions
- [ ] Fix TweenParallel removal during iteration
- [ ] Add Vector2, Vector4, Rect, int interpolators
- [ ] Add RectTransform extension methods
- [ ] Add null target handling
- [ ] Add From() methods
- [ ] Add AggressiveInlining to Ease functions

---

## DLog System

**Location**: `Assets/Scripts/DLog.cs`

### Parity Issues with Debug.Log

| Feature | Debug.Log | DLog | Status |
|---------|-----------|------|--------|
| Log(object) | ✓ | ✗ | Commented out line 47 |
| Log(object, Object) | ✓ | ✗ | Missing |
| LogWarning(object) | ✓ | ✗ | LogW is string only |
| LogWarning(object, Object) | ✓ | ✗ | Missing |
| LogError(object) | ✓ | ✗ | LogE is string only |
| LogError(object, Object) | ✓ | ✗ | Missing |
| LogFormat(string, params) | ✓ | ✗ | Missing |
| LogAssertion(object) | ✓ | ✗ | Missing |
| Assert(bool) | ✓ | ✗ | Missing |
| Break() | ✓ | ✗ | Missing |
| Conditional compilation | ✓ | ✗ | No #if UNITY_EDITOR support |

### Specific Issues

#### 1. Unused Unity.Logging Import
**Line 6**: `using Unity.Logging;`
**Problem**: Package is imported but never used. Dead code.

#### 2. Time() Ignores IsLoggingEnabled
**Lines 141-150**: The `Time()` method performs expensive timing and formatting even when logging is disabled.

#### 3. Missing Features
- No `Log(object message)` overload (most common Unity pattern)
- No conditional logging by category/tag
- No remote logging
- No SettingsProvider integration

#### 4. Performance Issues
- `Colorize()` allocates strings even when `IsColorEnabled = false` could be checked earlier

#### 5. SaveGraph Uses LogW for Success
**File**: `SerializedGraphView.cs:83`
```csharp
DLog.LogW($"Graph saved to {FilePath}");
```
**Problem**: Success message logged as warning. Should be `DLog.Log()`.

### Recommended Actions
- [ ] Remove unused `using Unity.Logging;` import
- [ ] Unify Log and LogInternal methods into single implementation
- [ ] Add Log(object) overload
- [ ] Add LogWarning(object) / LogError(object) overloads
- [ ] Add IsLoggingEnabled check to Time() method
- [ ] Add LogFormat equivalent
- [ ] Add Assert methods
- [ ] Implement SettingsProvider
- [ ] Fix SaveGraph to use Log instead of LogW

---

## Node-Based Systems (GraphView)

**Location**: `Assets/Editor/GraphView/`

### CRITICAL Issues

#### 1. SerializedGraphNode.PropagateData() Never Works (CRITICAL)
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

#### 2. WaveformGraphView.OnGraphViewChanged Same Bug
**File**: `WaveformGraphView.cs:24-29`
```csharp
if (e.output is not IDataPort<object> outPort) continue;
var data = outPort.GetData();
(e.input as IDataPort<object>)?.SetData(data);
```
**Problem**: Same generic type mismatch. `DataPort<AnimationCurve>` is NOT `IDataPort<object>`.
**Impact**: CRITICAL - Edge data propagation never works.

### HIGH Issues

#### 3. AudioOutputNode Destructor Issues
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

#### 4. Type Resolution Fails
**File**: `SerializedGraphView.cs:152-158`
```csharp
var type = Type.GetType(typeName);
```
**Problem**: `Type.GetType()` without assembly-qualified name won't find types in different assemblies.
**Fix**: Search assemblies or store full type name.

#### 5. AnimationCurve Serialization Loss
**File**: `SerializedGraphView.cs:108`
```csharp
nd.fields.Add(new() { name = fi.Name, val = fi.GetValue(n)?.ToString() ?? "" });
```
**Problem**: AnimationCurve.ToString() doesn't serialize curve data. Curves are lost on save/load.
**Fix**: Use JsonUtility for complex types or custom serialization.

### MEDIUM Issues

#### 6. No Undo/Redo Support (DEFERRED - Feature Request)
**Problem**: Graph modifications don't register with Unity's undo system.
**Status**: Requires significant architectural changes. Track as future feature.

#### 7. Inconsistent Init() Call Order
**Problem**: Some nodes call `base.Init()` first, others call it last.
**Impact**: May cause subtle bugs depending on what base.Init() does.

#### 8. DataPort.SetData Logs Every Time
**File**: `SerializedGraphNode.cs:69-73`
```csharp
public void SetData(T data)
{
    _data = data;
    DLog.Log($"{portName} received data: {data}");
}
```
**Problem**: Logs on every data set - noisy and potential performance issue.

### Recommended Actions
- [ ] **CRITICAL**: Fix PropagateData() generic type mismatch - use non-generic interface or reflection
- [ ] **CRITICAL**: Fix OnGraphViewChanged generic type mismatch
- [ ] Replace AudioOutputNode destructor with proper cleanup
- [ ] Fix type resolution to search assemblies
- [ ] Implement proper AnimationCurve serialization
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
**Fix**: Cache delegate or use expression trees.

#### 2. NormalizedAnimationCurveDrawer Preset Loading
**File**: `Editor/NormalizedAnimationCurveDrawer.cs:84-91`
**Problem**: Loads presets only on script reload, not on asset database refresh.
**Fix**: Subscribe to AssetDatabase callbacks.

### Incomplete Features

#### 1. Relative Mode Not Implemented
**File**: `Scripts/ComponentMemberReferenceCurve.cs:13-14, 50-53, 72-74`
All relative mode code is commented out.

#### 2. Hardcoded Paths
**File**: `Scripts/CurveConstants.cs`
**Problem**: Paths are hardcoded, will break if folder structure changes.

### Recommended Actions
- [ ] Cache reflection delegates in TransformCurve
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
Should use `DLog.LogE()`.

#### 3. No Event Prioritization
**Problem**: Events process in FIFO order, no priority system.

#### 4. No Event Cancellation
**Problem**: Can't cancel an event mid-propagation.

### Recommended Actions
- [ ] Create EventManagerUpdater MonoBehaviour
- [ ] Replace Debug.LogError with DLog.LogE
- [ ] Add priority system (optional)
- [ ] Add event cancellation support (optional)

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
**Lines 54-59**: Defines own `StatusEffectInstance` class, but `StatusEffectSystem.cs` already has one with more features.

#### Inventory Capacity Bug
**File**: `Inventory.cs:18`
**Problem**: `ItemCount` counts unique items, not total quantity. Capacity should check total items.

### Recommended Actions
- [ ] Test all gameplay systems
- [ ] Fix CardCollection.DrawFromTop()
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
Placeholder buttons log to Debug instead of doing anything.

#### 2. ScreenManager Not Singleton
**Problem**: ScreenManager is a regular MonoBehaviour, not a singleton.

#### 3. UIComponents.CreateDialog Font Issue
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
**Problem**: Idle and Align are nested inside Separation - likely a copy-paste error.

### Pathfinding
**Location**: `Assets/AI/Pathfinding/`

#### FlowFieldPathfinder2D TODO
**File**: `FlowFieldPathFinder2D.cs:69`
Consider renting an array instead of allocating.

### Recommended Actions
- [ ] Move Idle and Align classes outside Separation
- [ ] Implement array pooling for pathfinding

---

## Data Structures

**Location**: `Assets/Scripts/DataStructures/`

### Note on Debug.Assert
Multiple data structures use `Debug.Assert` which only runs in editor. Consider:
- Keep as-is for debug builds
- Or add runtime validation for critical checks

---

## Utility Systems

### LocalizationManager
**Issue**: Hardcoded `<SHEET_ID>` placeholder

### DataUtils
**Issues**:
- Uses Debug.LogError
- Hardcoded fallback language "en"

### HapticsManager
**TODO**: Editor and visualizer needed

---

## Empty/Stub Systems

### Completely Empty Files

| File | Purpose | Action Needed |
|------|---------|---------------|
| `AudioSystem.cs` | Audio management | Implement or delete |
| `SaveSystem/SaveManager.cs` | Save management | Implement or delete |
| `DebugUtils.cs` | Debug utilities | Implement or delete |

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

---

## Unity API Redundancies

### Consider Using Unity Built-ins

| Custom System | Unity Alternative | Recommendation |
|---------------|-------------------|----------------|
| Noise generators | Unity.Mathematics.noise | Evaluate switching |

### Unity.Logging Integration Opportunity

The project includes `com.unity.logging 1.3.10` but DLog doesn't use it. Unity.Logging provides:
- Structured logging
- Multiple sinks
- Log levels
- Performance optimized

**Recommendation**: Refactor DLog to use Unity.Logging as backend while keeping DLog's API.

---

## Priority Summary

### CRITICAL (Fix Immediately)
1. EventManager.Update() never called
2. CardCollection.DrawFromTop() doesn't remove card
3. SerializedGraphNode.PropagateData() generic type mismatch
4. WaveformGraphView.OnGraphViewChanged same generic bug

### HIGH (Fix Soon)
5. AudioOutputNode destructor issues
6. TransformCurve reflection performance
7. Test all gameplay systems
8. Implement or delete empty files
9. AlignDistributeSnapWindow crashes when nothing selected
10. SimplexNoise uses uninitialized random
11. WorleyNoise uses wrong Random class
12. DLog Time() method ignores IsLoggingEnabled

### MEDIUM (Scheduled Work)
13. Complete DLog parity with Debug.Log
14. Add Vector2/RectTransform tween support
15. Fix GraphView type resolution
16. Fix AnimationCurve serialization
17. Replace all Debug.Log with DLog (17 instances)
18. Add proper Curves performance caching
19. TextEffects character index mismatch
20. DataPort.SetData logs on every call
21. Inconsistent Init() call order in nodes

### LOW (Nice to Have)
22. Add undo/redo to GraphView (deferred - major feature)
23. Implement HapticsManager editor
24. Add SettingsProvider for DLog
25. Add AggressiveInlining to Ease functions
26. Complete relative mode for Curves
27. Evaluate Unity.Mathematics.noise integration
28. Integrate Unity.Logging with DLog

---

## Implementation Checklist

### Phase 0: Critical Bug Fixes
- [ ] Fix SerializedGraphNode.PropagateData() generic type issue
- [ ] Fix WaveformGraphView.OnGraphViewChanged generic type issue

### Phase 1: Critical Fixes
- [ ] Create EventManagerUpdater MonoBehaviour
- [ ] Fix CardCollection.DrawFromTop()

### Phase 2: Core System Fixes
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
- [ ] Fix TextEffects character indexing

### Phase 4: Code Quality
- [ ] Replace 17+ Debug.Log instances with DLog
- [ ] Add tests for gameplay systems
- [ ] Delete or implement empty files
- [ ] Fix nested class issues in SteeringBehaviorSystem
- [ ] Make DataPort.SetData logging optional
- [ ] Standardize Init() call order in graph nodes

### Phase 5: Polish & Integration
- [ ] Add undo/redo to GraphView
- [ ] Add SettingsProvider
- [ ] Evaluate Unity.Mathematics.noise integration
- [ ] Integrate Unity.Logging as DLog backend

---

## Summary Statistics

| Category | Count |
|----------|-------|
| Critical Bugs | 4 |
| High Priority Issues | 8 |
| Medium Priority Issues | 9 |
| Low Priority Issues | 7 |
| **Total Issues** | **28** |
| **Fixed This Session** | **16** |

---

## Fixed Issues (This Session)

1. WaveformEditorWindow.ShowWindow() - wrong window type
2. LogW bypasses IsLoggingEnabled
3. Rand.Shuffle() corrupts data - double IntRanged call
4. EnumCache stores null strings
5. ReflectionUtils.SetValue always throws - missing else
6. TweenSequence double delta time calculation
7. TweenManager.GetByTag allocates every call
8. AsyncUtils.CompletedTask redundant
9. A* Pathfinding global static state
10. ComponentMemberReference reflection every frame
11. LogSinks system unused
12. IgnoredMethods list unused
13. InstantiateNode never calls Init()
14. Ports added twice in waveform nodes
15. Data propagation not automatic in AnimationCurveNode
16. DLog StackTrace performance (CallerInfo attributes)

---

*Document generated: 2024*
*Project: DunksJams*
*Unity Version: 6000.3.3f1*
