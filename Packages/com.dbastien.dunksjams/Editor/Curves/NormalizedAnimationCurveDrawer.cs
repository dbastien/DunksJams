using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(NormalizedAnimationCurveAttribute))]
public class NormalizedAnimationCurveDrawer : PropertyDrawer
{
    private enum WrapModeUIFriendly
    {
        Loop = 2,
        PingPong = 4,
        ClampForever = 8
    }

    private static ScriptableObject presets;

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.PrefixLabel(position, label);
        EditorGUI.BeginProperty(position, label, prop);

        AnimationCurve oldCurve = prop.animationCurveValue;

        var wrapMode = (int)oldCurve.preWrapMode;

        prop.animationCurveValue =
            EditorGUILayout.CurveField(prop.animationCurveValue, GUILayout.Height(100f), GUILayout.Width(100f));

        EditorGUI.BeginChangeCheck();

        wrapMode = (int)(WrapModeUIFriendly)EditorGUILayout.EnumPopup((WrapModeUIFriendly)wrapMode);

        if (EditorGUI.EndChangeCheck())
        {
            AnimationCurve tempCurve = prop.animationCurveValue;
            tempCurve.preWrapMode = (WrapMode)wrapMode;
            tempCurve.postWrapMode = (WrapMode)wrapMode;
            prop.animationCurveValue = tempCurve;
            prop.serializedObject.ApplyModifiedProperties();
        }

        var curveItemSize = new Vector2(40f, 40f);
        var curveItemPadding = new Vector2(5f, 5f);

        int presetCount = CurvePresetLibraryWrapper.Count(presets);

        int rowItems = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - curveItemPadding.x) /
                                        (curveItemSize.x + curveItemPadding.x));

        var p = 0;
        while (p < presetCount)
        {
            EditorGUILayout.BeginHorizontal();
            int itemsThisRow = Mathf.Min(presetCount - p, rowItems);
            for (var i = 0; i < itemsThisRow; ++i)
            {
                Rect rect = GUILayoutUtility.GetRect(curveItemSize.x,
                    curveItemSize.y,
                    GUILayout.Height(curveItemSize.x),
                    GUILayout.Width(curveItemSize.y));

                string curveName = CurvePresetLibraryWrapper.GetName(presets, p);

                if (GUI.Button(rect, new GUIContent(string.Empty, curveName)))
                {
                    AnimationCurve animationCurve = CurvePresetLibraryWrapper.GetPreset(presets, p);
                    animationCurve.preWrapMode = (WrapMode)wrapMode;
                    animationCurve.postWrapMode = (WrapMode)wrapMode;
                    prop.animationCurveValue = animationCurve;
                }

                if (Event.current.type == EventType.Repaint) CurvePresetLibraryWrapper.Draw(presets, rect, p);
                if (i != itemsThisRow - 1) GUILayout.Space(curveItemPadding.x);
                ++p;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(curveItemPadding.y);
        }

        prop.serializedObject.ApplyModifiedProperties();

        EditorGUI.EndProperty();
    }

    //todo: ideally also triggers when asset database refreshes
    [DidReloadScripts]
    private static void LoadPresets()
    {
        string path = Application.dataPath + CurveConstants.NormalizedCurvesPath;
        Object[] objs = InternalEditorUtility.LoadSerializedFileAndForget(path);

        if (objs.Length > 0) presets = objs[0] as ScriptableObject;
    }
}