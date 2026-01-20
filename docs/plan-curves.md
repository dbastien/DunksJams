# Curves & Tweening Systems Plan

This document covers the analysis and improvement plan for the AnimationCurves & Tweening functionality in the DunksJams project.

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

## Tweening System (PrimeTween)

**Status**: PrimeTween library is available in the project (see `.working/Logging/primetween.md`)

### Overview
PrimeTween is a high-performance, **allocation-free** animation library for Unity. It provides:
- Single-line animations for any property
- Inspector-tweakable animation properties
- Complex animation sequences
- Zero runtime memory allocations

### Current Integration
- **Documentation**: Available in `.working/Logging/primetween.md`
- **Usage**: Library appears to be available but integration status unclear
- **Examples**: `Assets/Scripts/Tween/TweenExample.cs` exists

### Potential Enhancements

#### 1. Full Integration Assessment
- [ ] Verify PrimeTween is properly imported and accessible
- [ ] Test basic tweening functionality
- [ ] Evaluate performance vs alternatives (DOTween, etc.)

#### 2. Inspector Integration
- [ ] Ensure animation properties are tweakable in Inspector
- [ ] Verify sequence creation tools work
- [ ] Test callback and event system integration

#### 3. Performance Optimization
- [ ] Confirm zero-allocation claims in practice
- [ ] Benchmark against current animation systems
- [ ] Profile memory usage in typical use cases

#### 4. Workflow Integration
- [ ] Create tweening utilities for common Unity operations
- [ ] Integrate with existing animation systems
- [ ] Add tweening presets for game-specific animations

### Migration Considerations
- **From DOTween**: PrimeTween includes DOTween adapter for migration
- **Performance**: Significantly better performance with zero allocations
- **API Differences**: Different method signatures and patterns

### Recommended Actions
- [ ] Complete PrimeTween integration assessment
- [ ] Create wrapper utilities for common tweening operations
- [ ] Document PrimeTween best practices for the team
- [ ] Migrate existing tweening code to PrimeTween where beneficial

---

## Integration Opportunities

### Curves + Tweening Synergy
- **Curve-driven animations**: Use Curves system to define tweening curves
- **Procedural easing**: Generate custom easing curves algorithmically
- **Animation blending**: Combine curve-based and tween-based animations

### Editor Tools
- **Curve editors**: Enhanced curve editing with tweening preview
- **Animation timelines**: Visual timeline editing for tween sequences
- **Preset systems**: Shareable animation presets across projects

---

## Implementation Plan

### Phase 1: Assessment & Integration
1. **Curves system fixes**:
   - Implement AssetDatabase callback subscriptions
   - Add relative mode support
   - Remove hardcoded paths

2. **Tweening evaluation**:
   - Verify PrimeTween functionality
   - Test performance characteristics
   - Assess integration requirements

### Phase 2: Enhancement & Tools
1. **Curve system improvements**:
   - Better preset management
   - Enhanced editor tools
   - Runtime curve evaluation optimizations

2. **Tweening utilities**:
   - Create common tweening helpers
   - Add animation presets
   - Integrate with existing systems

### Phase 3: Advanced Features
1. **Hybrid systems**: Combine curves and tweening
2. **Procedural animation**: Runtime-generated animations
3. **Performance monitoring**: Track animation system performance

---

## Risk Assessment

### Compatibility Issues
- **Unity version compatibility**: Ensure PrimeTween works with current Unity version
- **Existing code**: Check for conflicts with current animation systems
- **Performance impact**: Monitor for any performance regressions

### Integration Challenges
- **Learning curve**: Team needs to learn PrimeTween API
- **Migration effort**: Converting existing animations to PrimeTween
- **Debugging complexity**: New animation system may complicate debugging

### Testing Requirements
- **Animation quality**: Ensure smooth, predictable animations
- **Performance benchmarks**: Compare with existing systems
- **Edge cases**: Test complex animation sequences and interactions

---

## Success Metrics

- **Animation performance**: Smooth 60fps animations with minimal CPU usage
- **Developer productivity**: Faster animation creation and iteration
- **Code maintainability**: Clean, consistent animation code across the project
- **User experience**: Polished, responsive animations that enhance gameplay