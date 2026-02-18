#if UNITY_EDITOR

#region

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EditorGUIUtil;
using static Tabify;

#endregion

public class TabifyPlaceholderWindow : EditorWindow
{
    public GlobalID objectGlobalID;

    public bool isSceneObject;
    public bool isPrefabObject;

    private void OnGUI()
    {
        // GUILayout.Label(objectGlobalID.ToString());
        // GUILayout.Label(objectGlobalID.guid.ToPath());

        // if (isSceneObject)
        //     GUILayout.Label("scene object");

        // if (isPrefabObject)
        //     GUILayout.Label("prefab object");

        var fontSize = 13;
        string assetName = objectGlobalID.guid.ToPath().GetFilename();
        Texture assetIcon = AssetDatabase.GetCachedIcon(objectGlobalID.guid.ToPath());

        void Label()
        {
            GUI.skin.label.fontSize = fontSize;

            GUILayout.Label("This object is from      " + assetName + ", which isn't loaded");

            Rect iconRect = lastRect.MoveX("This object is from".GetLabelWidth()).
                SetWidth(20).
                SetSizeFromMid(16).
                MoveX(.5f);

            GUI.DrawTexture(position: iconRect, image: assetIcon);

            GUI.skin.label.fontSize = 0;
        }

        void Button()
        {
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
        }

        GUILayout.Space(15);
        // BeginIndent(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical();

        Label();

        Space(10);
        Button();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        void TryLoadPrefabObject()
        {
            if (!isPrefabObject) return;
            if (StageUtility.GetCurrentStage() is not PrefabStage prefabStage) return;
            if (prefabStage.assetPath != objectGlobalID.guid.ToPath()) return;

            if (objectGlobalID.GetObject() is not { } prefabAssetObject) return;

            if (prefabAssetObject is Component assetComponent)
                if (prefabStage.prefabContentsRoot.GetComponentsInChildren(assetComponent.GetType()).
                        FirstOrDefault(r => GlobalID.GetForPrefabStageObject(o: r) == objectGlobalID) is { } instanceComoponent)
                    CloseAndOpenPropertyEditor(o: instanceComoponent);

            if (prefabAssetObject is GameObject assetGo)
                if (prefabStage.prefabContentsRoot.GetComponentsInChildren<Transform>().
                        Select(r => r.gameObject).
                        FirstOrDefault(r => GlobalID.GetForPrefabStageObject(o: r) == objectGlobalID) is { } isntanceGo)
                    CloseAndOpenPropertyEditor(o: isntanceGo);
        }

        void TryLoadSceneObject()
        {
            if (!isSceneObject) return;

            IEnumerable<Scene> loadedScenes = Enumerable.Range(0, count: EditorSceneManager.sceneCount).
                Select(i => EditorSceneManager.GetSceneAt(index: i)).
                Where(r => r.isLoaded);
            if (!loadedScenes.Any(r => r.path == objectGlobalID.guid.ToPath())) return;

            if (objectGlobalID.GetObject() is not { } loadedObject) return;

            CloseAndOpenPropertyEditor(o: loadedObject);
        }

        TryLoadPrefabObject();
        TryLoadSceneObject();
    }

    public void CloseAndOpenPropertyEditor(Object o)
    {
        var dockArea = this.GetMemberValue<Object>("m_Parent");
        int tabIndex = dockArea.GetMemberValue<List<EditorWindow>>("m_Panes").IndexOf(this);

        var tabInfo = new TabInfo(lockTo: o)
        {
            originalTabIndex = tabIndex
        };

        guis_byDockArea[key: dockArea].AddTab(tabInfo: tabInfo, true);

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