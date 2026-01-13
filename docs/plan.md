# Comprehensive Codebase Analysis and Fix Plan

This document provides a thorough analysis of incomplete systems, bugs, refactoring opportunities, and Unity API redundancies in the DunksJams project.

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

---

## Node-Based Systems (GraphView)

**Location**: `Assets/Editor/GraphView/`

### HIGH Issues

#### 1. ~~AudioOutputNode Destructor Issues~~ ✅ FIXED
Replaced dangerous C# finalizer with `ICleanupNode` interface. `Cleanup()` is called by `SerializedGraphView.RemoveNode()` and `ClearGraph()`.

#### 2. Type Resolution Fails
**File**: `SerializedGraphView.cs:152-158`
```csharp
var type = Type.GetType(typeName);
```
**Problem**: `Type.GetType()` without assembly-qualified name won't find types in different assemblies.
**Fix**: Search assemblies or store full type name.

#### 3. AnimationCurve Serialization Loss
**File**: `SerializedGraphView.cs:108`
```csharp
nd.fields.Add(new() { name = fi.Name, val = fi.GetValue(n)?.ToString() ?? "" });
```
**Problem**: AnimationCurve.ToString() doesn't serialize curve data. Curves are lost on save/load.
**Fix**: Use JsonUtility for complex types or custom serialization.

### MEDIUM Issues

#### 4. No Undo/Redo Support (DEFERRED - Feature Request)
**Problem**: Graph modifications don't register with Unity's undo system.
**Status**: Requires significant architectural changes. Track as future feature.

#### 5. Inconsistent Init() Call Order
**Problem**: Some nodes call `base.Init()` first, others call it last.
**Impact**: May cause subtle bugs depending on what base.Init() does.

#### 6. DataPort.SetData Logs Every Time
**File**: `SerializedGraphNode.cs:69-73`
```csharp
public void SetData(T data)
{
    _data = data;
    DLog.Log($"{portName} received data: {data}");
}
```
**Problem**: Logs on every data set - noisy and potential performance issue.

---

## Curves System

**Location**: `Assets/Curves/`

### Performance Issues

#### 1. NormalizedAnimationCurveDrawer Preset Loading
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
- [ ] Subscribe to AssetDatabase.importPackageCompleted
- [ ] Implement relative mode
- [ ] Use AssetDatabase.FindAssets for curve paths

---

## Event System

**Location**: `Assets/Scripts/EventSystem/`

### Issues

#### 1. Uses Debug.LogError
**File**: `EventManager.cs:103`
Should use `DLog.LogE()`.

#### 2. No Event Prioritization
**Problem**: Events process in FIFO order, no priority system.

#### 3. No Event Cancellation
**Problem**: Can't cancel an event mid-propagation.

### Recommended Actions
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
| CardGameManager | CardGameManager.cs | Untested | Debug.Log usage |

### Specific Issues

#### Health.cs Duplicate Class
**Lines 54-59**: Defines own `StatusEffectInstance` class, but `StatusEffectSystem.cs` already has one with more features.

#### Inventory Capacity Bug
**File**: `Inventory.cs:18`
**Problem**: `ItemCount` counts unique items, not total quantity. Capacity should check total items.

### Recommended Actions
- [ ] Test all gameplay systems
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

### TreeView API Deprecation (18 warnings)

Unity 6 deprecated non-generic TreeView APIs. Need to migrate to generic versions:

| Old API | New API |
|---------|---------|
| `TreeView` | `TreeView<int>` |
| `TreeViewItem` | `TreeViewItem<int>` |
| `TreeViewState` | `TreeViewState<int>` |

**Affected Files**:
- `AssetBrowserTreeView.cs` (10 usages)
- `AssetBrowserTreeViewItem.cs` (1 usage)
- `AssetBrowserWindow.cs` (1 usage)
- `SceneBrowser.cs`, `ShaderBrowser.cs`, `ModelBrowser.cs`, `MaterialBrowser.cs`, `TextureBrowser.cs`, `ScriptBrowser.cs` (1 each)

**Effort**: Medium - requires updating class hierarchy and method signatures across AssetBrowser system.

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

