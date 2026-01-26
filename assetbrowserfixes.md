# Asset Browser Fixes (Unity 6)

Scope: `Assets/Editor/AssetBrowser/*.cs`

## 1) Unity 6 warning fixes to check

- Obsolete ShaderUtil APIs are used in Material/Shader browsers and are already suppressed with `#pragma` (CS0618). If Unity 6 emits new warnings or replaces these APIs, prefer wrappers or newer editor APIs for shader messages and data. `Assets/Editor/AssetBrowser/MaterialBrowser.cs:131` `Assets/Editor/AssetBrowser/ShaderBrowser.cs:67`
- `AssetBrowserTreeViewItem` initializes `Asset` and `AssetImporter` with `new` Unity objects. Unity commonly warns against `new` for `UnityEngine.Object` / `AssetImporter`. Safer default is `null` (and rely on overrides), or make the base properties abstract to force overrides. `Assets/Editor/AssetBrowser/AssetBrowserTreeViewItem.cs:14`
- `AssetImporter.assetTimeStamp` is accessed in `CreateTimestampColumn`. If Unity 6 deprecates or removes it, switch to `File.GetLastWriteTime` or `AssetDatabase.GetAssetDependencyHash` depending on intent. `Assets/Editor/AssetBrowser/AssetBrowserTreeView.cs:173`
- Scene-only gather logic uses `Object.FindObjectsByType` overloads with different inactive handling. If Unity 6 emits warnings or performance notes, standardize to the overload that explicitly includes/excludes inactive objects for consistency. `Assets/Editor/AssetBrowser/SceneBrowser.cs:61` `Assets/Editor/AssetBrowser/ScriptBrowser.cs:61`

## 2) Redundancies to remove (low-risk refactors)

- Window `Rebuild` boilerplate is duplicated across all browser windows. Move this into `AssetBrowserWindow` and have derived windows call a shared helper. `Assets/Editor/AssetBrowser/MaterialBrowser.cs:19` `Assets/Editor/AssetBrowser/ModelBrowser.cs:15` `Assets/Editor/AssetBrowser/SceneBrowser.cs:15` `Assets/Editor/AssetBrowser/ScriptBrowser.cs:15` `Assets/Editor/AssetBrowser/ShaderBrowser.cs:18` `Assets/Editor/AssetBrowser/TextureBrowser.cs:15`
- Scene-only `GatherGuids` logic for Shader/Texture/Material browsers repeats the same renderer/material scan pattern. Extract a shared helper that returns unique materials, shaders, or textures from renderers. `Assets/Editor/AssetBrowser/ShaderBrowser.cs:125` `Assets/Editor/AssetBrowser/TextureBrowser.cs:73` `Assets/Editor/AssetBrowser/MaterialBrowser.cs:148`
- `TreeViewItem` classes follow the same pattern (store asset + importer, set runtime memory, then call base `Rebuild`). A small generic helper in the base tree view could reduce repeated plumbing. `Assets/Editor/AssetBrowser/ModelBrowser.cs:22` `Assets/Editor/AssetBrowser/SceneBrowser.cs:24` `Assets/Editor/AssetBrowser/ScriptBrowser.cs:24` `Assets/Editor/AssetBrowser/ShaderBrowser.cs:27` `Assets/Editor/AssetBrowser/TextureBrowser.cs:24`
- Common column sets (Object, Path, Runtime, Storage, Refs/Deps, Written) are repeated with minor differences. A helper to build the base column list would reduce drift. `Assets/Editor/AssetBrowser/ModelBrowser.cs:70` `Assets/Editor/AssetBrowser/TextureBrowser.cs:101` `Assets/Editor/AssetBrowser/ShaderBrowser.cs:153`

## 3) Feature ideas to add (opt-in, keep scope contained)

- Filtering: by label, file extension, import settings, size ranges, or "unused in scene" toggles.
- Quick actions: ping/open asset, copy GUID, open importer settings, reimport selected, save import settings for selection.
- Export: JSON or TSV in addition to CSV, and include selected columns only.
- Dependency UX: show reverse dependencies (assets that reference the item) with caching, and allow expanding inline.
- Performance: incremental search updates, cached GUID lists per asset type, and progress cancellation for long scans.
