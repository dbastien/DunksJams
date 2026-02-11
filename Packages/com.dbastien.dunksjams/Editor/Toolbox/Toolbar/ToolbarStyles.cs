using UnityEditor;
using UnityEngine;

/// <summary>Shared toolbar GUI styles.</summary>
public static class ToolbarStyles
{
    static GUIStyle errorStyle;
    static GUIStyle toolbarButtonLeftStyle;
    static GUIStyle toolbarButtonRightStyle;
    static GUIStyle toolbarPopupLeftStyle;
    static GUIStyle toolbarPopupRightStyle;
    static GUIStyle toolbarDropDownLeftStyle;
    static GUIStyle toolbarDropDownRightStyle;
    static GUIStyle toolbarDropDownToggleRightStyle;
    static GUIStyle toolbarCreateAddNewDropDownStyle;
    static GUIStyle toolbarLabelStyle;
    static GUIStyle toolbarTextAreaStyle;

    public static GUIStyle ToolbarButtonStyle => EditorStyles.toolbarButton;
    public static GUIStyle ToolbarButtonLeftStyle => toolbarButtonLeftStyle ??= GetStyle("toolbarbuttonLeft");
    public static GUIStyle ToolbarButtonRightStyle => toolbarButtonRightStyle ??= GetStyle("toolbarbuttonRight");
    public static GUIStyle ToolbarPopupStyle => EditorStyles.toolbarPopup;
    public static GUIStyle ToolbarPopupLeftStyle => toolbarPopupLeftStyle ??= GetStyle("toolbarPopupLeft");
    public static GUIStyle ToolbarPopupRightStyle => toolbarPopupRightStyle ??= GetStyle("toolbarPopupRight");
    public static GUIStyle ToolbarDropDownStyle => EditorStyles.toolbarDropDown;
    public static GUIStyle ToolbarDropDownLeftStyle => toolbarDropDownLeftStyle ??= GetStyle("toolbarDropDownLeft");
    public static GUIStyle ToolbarDropDownRightStyle => toolbarDropDownRightStyle ??= GetStyle("toolbarDropDownRight");
    public static GUIStyle ToolbarDropDownToggleRightStyle => toolbarDropDownToggleRightStyle ??= GetStyle("toolbarDropDownToggleRight");
    public static GUIStyle ToolbarCreateAddNewDropDownStyle => toolbarCreateAddNewDropDownStyle ??= GetStyle("toolbarCreateAddNewDropDown");

    public static GUIStyle ToolbarLabelStyle => toolbarLabelStyle ??= GetStyle("toolbarLabel");

    public static GUIStyle ToolbarTextAreaStyle =>
        toolbarTextAreaStyle ??= new GUIStyle(EditorStyles.textArea) { wordWrap = true };

    public static GUIStyle ErrorStyle => errorStyle ??= new GUIStyle { name = "StyleNotFoundError" };

    public static GUIStyle GetStyle(string styleName)
    {
        var style = GUI.skin.FindStyle(styleName)
            ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        if (style == null)
        {
            DLog.LogW($"Missing built-in GUI style: {styleName}");
            style = ErrorStyle;
        }
        return style;
    }

    public static void Shutdown()
    {
        errorStyle = null;
        toolbarButtonRightStyle = null;
        toolbarTextAreaStyle = null;
    }
}
