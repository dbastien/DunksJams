#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EditorGUIUtil;
using static Tabify;

public class TabifyPlaceholderWindow : EditorWindow
{
    public GlobalID objectGlobalID;

    public bool isSceneObject;
    public bool isPrefabObject;

    private void OnGUI()
    {
        var fontSize = 13;
        string assetName = objectGlobalID.guid.ToPath().GetFilename();
        Texture assetIcon = AssetDatabase.GetCachedIcon(objectGlobalID.guid.ToPath());

        GUILayout.Space(15);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical();

        GUI.skin.label.fontSize = fontSize;

        GUILayout.Label("This object is from      " + assetName + ", which isn't loaded");

        Rect iconRect1 = lastRect.MoveX("This object is from".GetLabelWidth()).
            SetWidth(20).
            SetSizeFromMid(16).
            MoveX(.5f);

        GUI.DrawTexture(position: iconRect1, image: assetIcon);

        GUI.skin.label.fontSize = 0;

        Space(10);
        GUI.skin.button.fontSize = fontSize;

        string buttonText = "Load      " + assetName;

        if (GUILayout.Button(text: buttonText, GUILayout.Height(30),
                GUILayout.Width(buttonText.GetLabelWidth(fontSize: fontSize) + 34)))
            if (isPrefabObject)
                PrefabStageUtility.OpenPrefab(objectGlobalID.guid.ToPath());
            else if (isSceneObject)
                EditorSceneManager.OpenScene(objectGlobalID.guid.ToPath());

        Rect iconRect = lastRect.MoveX("Load".GetLabelWidth()).SetWidth(20).SetSizeFromMid(16).MoveX(23 - 3);

        GUI.DrawTexture(position: iconRect, image: assetIcon);

        GUI.skin.button.fontSize = 0;

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if (isPrefabObject &&
            StageUtility.GetCurrentStage() is PrefabStage prefabStage &&
            prefabStage.assetPath == objectGlobalID.guid.ToPath() &&
            objectGlobalID.GetObject() is { } prefabAssetObject)
        {
            if (prefabAssetObject is Component assetComponent)
                if (prefabStage.prefabContentsRoot.GetComponentsInChildren(assetComponent.GetType()).
                        FirstOrDefault(r => GlobalID.GetForPrefabStageObject(o: r) == objectGlobalID) is
                    { } instanceComponent)
                    CloseAndOpenPropertyEditor(o: instanceComponent);

            if (prefabAssetObject is GameObject assetGo)
                if (prefabStage.prefabContentsRoot.GetComponentsInChildren<Transform>().
                        Select(r => r.gameObject).
                        FirstOrDefault(r => GlobalID.GetForPrefabStageObject(o: r) == objectGlobalID) is
                    { } instanceGo)
                    CloseAndOpenPropertyEditor(o: instanceGo);
        }

        if (!isSceneObject) return;

        IEnumerable<Scene> loadedScenes = Enumerable.Range(0, count: EditorSceneManager.sceneCount).
            Select(i => EditorSceneManager.GetSceneAt(index: i)).
            Where(r => r.isLoaded);
        if (!loadedScenes.Any(r => r.path == objectGlobalID.guid.ToPath())) return;

        if (objectGlobalID.GetObject() is not { } loadedObject) return;

        CloseAndOpenPropertyEditor(o: loadedObject);
    }

    public void CloseAndOpenPropertyEditor(Object o)
    {
        var dockArea = this.GetMemberValue<Object>("m_Parent");
        int tabIndex = dockArea.GetMemberValue<List<EditorWindow>>("m_Panes").IndexOf(this);

        var tabInfo = new TabInfo(lockTo: o)
        {
            originalTabIndex = tabIndex
        };

        GuisByDockArea[key: dockArea].AddTab(tabInfo: tabInfo, true);

        Close();
    }

    public void OpenAndReplacePropertyEditor(EditorWindow propertyEditorToReplace)
    {
        objectGlobalID = new GlobalID(propertyEditorToReplace.GetMemberValue<string>("m_GlobalObjectId"));

        isSceneObject = AssetDatabase.GetMainAssetTypeAtPath(objectGlobalID.guid.ToPath()) == typeof(SceneAsset);
        isPrefabObject = AssetDatabase.GetMainAssetTypeAtPath(objectGlobalID.guid.ToPath()) == typeof(GameObject);

        if (!isSceneObject && !isPrefabObject)
        {
            propertyEditorToReplace.Close();
            DestroyImmediate(this);
            return;
        }

        object dockArea = propertyEditorToReplace.GetMemberValue("m_Parent");

        int tabIndex = dockArea.GetMemberValue<List<EditorWindow>>("m_Panes").IndexOf(item: propertyEditorToReplace);

        dockArea.InvokeMethod("AddTab", tabIndex, this, true);

        titleContent = propertyEditorToReplace.titleContent;

        if (propertyEditorToReplace.hasFocus)
            Focus();

        propertyEditorToReplace.Close();
    }
}
#endif