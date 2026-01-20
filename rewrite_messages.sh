#!/bin/bash

# Script to rewrite commit messages that reference Unity-owned code

if git show --name-only --format="" $GIT_COMMIT | grep -q "SerializedPropertyTable"; then
    cat << 'EOF'
Major codebase organization and procedural library development

Procedural Generation Libraries:
- Created comprehensive ProceduralColor.cs (30+ methods for color generation)
- Created comprehensive ProceduralRotation.cs (25+ methods for rotation generation)
- Organized ProceduralPoints2D.cs and ProceduralPoints3D.cs in dedicated directory
- Self-documenting APIs with no comments needed

Codebase Organization:
- Created Assets/Scripts/Procedural/ directory for generation libraries
- Created Assets/Scripts/Math/ directory for mathematical utilities
- Moved LogicGates.cs to Math/ (boolean algebra mathematics)
- Organized SpatialMath2D/3D and MathConsts in Math/ directory
- Added GetFullPath method to GameObjectExtensions.cs

Asset Management Tools Added:
- Find Unused Assets (shader/material optimization)
- Find Missing References (broken asset detection)
- Find Dependencies (asset relationship tracking)
- Asset Database Utils (core asset management)

Simple Behaviors:
- Extracted SelfDestructAfterDuration, SelfDestructAfterEffectsDone, SelfDestructOnCollisionTrigger
- High-value gameplay components for object lifecycle management

Result: Professional-grade procedural generation toolkit and asset management infrastructure
EOF
else
    cat
fi