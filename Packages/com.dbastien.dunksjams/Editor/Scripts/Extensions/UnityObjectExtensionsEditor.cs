#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class UnityObjectExtensionsEditor
{
    public static void RecordUndo(this Object o, string operationName = "") => Undo.RecordObject(o, operationName);
    public static void Dirty(this Object o) => EditorUtility.SetDirty(o);
    public static void Save(this Object o) => AssetDatabase.SaveAssetIfDirty(o);

    public static void SelectInInspector
        (this Object[] objects, bool frameInHierarchy = false, bool frameInProject = false)
    {
        void setHierarchyLocked
            (bool isLocked) => allHierarchies.ForEach(r =>
            r?.GetMemberValue("m_SceneHierarchy")?.SetMemberValue("m_RectSelectInProgress", true));

        void setProjectLocked
            (bool isLocked) =>
            allProjectBrowsers.ForEach(r => r?.SetMemberValue("m_InternalSelectionChange", isLocked));

        if (!frameInHierarchy) setHierarchyLocked(true);
        if (!frameInProject) setProjectLocked(true);

        Selection.objects = objects?.ToArray();

        if (!frameInHierarchy) EditorApplication.delayCall += () => setHierarchyLocked(false);
        if (!frameInProject) EditorApplication.delayCall += () => setProjectLocked(false);
    }

    public static void SelectInInspector(this Object obj, bool frameInHierarchy = false, bool frameInProject = false) =>
        new[] { obj }.SelectInInspector(frameInHierarchy, frameInProject);

    private static IEnumerable<EditorWindow> allHierarchies => _allHierarchies ??= typeof(Editor).Assembly.
        GetType("UnityEditor.SceneHierarchyWindow").
        GetFieldValue<IList>("s_SceneHierarchyWindows").
        Cast<EditorWindow>();

    private static IEnumerable<EditorWindow> _allHierarchies;

    private static IEnumerable<EditorWindow> allProjectBrowsers => _allProjectBrowsers ??= typeof(Editor).Assembly.
        GetType("UnityEditor.ProjectBrowser").
        GetFieldValue<IList>("s_ProjectBrowsers").
        Cast<EditorWindow>();

    private static IEnumerable<EditorWindow> _allProjectBrowsers;
}
#endif