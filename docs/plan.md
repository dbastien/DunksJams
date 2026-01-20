# Comprehensive Codebase Analysis and Fix Plan

This document provides a thorough analysis of incomplete systems, bugs, refactoring opportunities, and Unity API redundancies in the DunksJams project.

---


## Node-Based Systems (GraphView)

**Location**: `Assets/Editor/GraphView/`

### HIGH Issues

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

#### 6. No Undo/Redo Support (DEFERRED - Feature Request)
**Problem**: Graph modifications don't register with Unity's undo system.
**Status**: Requires significant architectural changes. Track as future feature.

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

See `docs/plan-ai.md` for comprehensive AI and pathfinding analysis and improvement plan.

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

