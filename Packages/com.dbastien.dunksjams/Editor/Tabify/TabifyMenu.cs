#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public static class TabifyMenu
{
    public static bool dragndropEnabled
    {
        get => EditorPrefsCached.GetBool("tabify-dragndropEnabled", true);
        set => EditorPrefsCached.SetBool("tabify-dragndropEnabled", value: value);
    }

    public static bool addTabButtonEnabled
    {
        get => EditorPrefsCached.GetBool("tabify-addTabButtonEnabled");
        set => EditorPrefsCached.SetBool("tabify-addTabButtonEnabled", value: value);
    }

    public static bool closeTabButtonEnabled
    {
        get => EditorPrefsCached.GetBool("tabify-closeTabButtonEnabled");
        set => EditorPrefsCached.SetBool("tabify-closeTabButtonEnabled", value: value);
    }

    public static bool hideLockButtonEnabled
    {
        get => EditorPrefsCached.GetBool("tabify-hideLockButtonEnabled");
        set => EditorPrefsCached.SetBool("tabify-hideLockButtonEnabled", value: value);
    }

    public static int tabStyle
    {
        get => EditorPrefsCached.GetInt("tabify-tabStyle");
        set => EditorPrefsCached.SetInt("tabify-tabStyle", value: value);
    }

    public static bool defaultTabStyleEnabled => tabStyle == 0 || !Application.unityVersion.StartsWith("6000");
    public static bool largeTabStyleEnabled => tabStyle == 1 && Application.unityVersion.StartsWith("6000");
    public static bool neatTabStyleEnabled => tabStyle == 2 && Application.unityVersion.StartsWith("6000");

    public static int backgroundStyle
    {
        get => EditorPrefsCached.GetInt("tabify-backgroundStyle");
        set => EditorPrefsCached.SetInt("tabify-backgroundStyle", value: value);
    }

    public static bool switchTabShortcutEnabled
    {
        get => EditorPrefsCached.GetBool("tabify-switchTabShortcutEnabled", true);
        set => EditorPrefsCached.SetBool("tabify-switchTabShortcutEnabled", value: value);
    }

    public static bool addTabShortcutEnabled
    {
        get => EditorPrefsCached.GetBool("tabify-addTabShortcutEnabled", true);
        set => EditorPrefsCached.SetBool("tabify-addTabShortcutEnabled", value: value);
    }

    public static bool closeTabShortcutEnabled
    {
        get => EditorPrefsCached.GetBool("tabify-closeTabShortcutEnabled", true);
        set => EditorPrefsCached.SetBool("tabify-closeTabShortcutEnabled", value: value);
    }

    public static bool reopenTabShortcutEnabled
    {
        get => EditorPrefsCached.GetBool("tabify-reopenTabShortcutEnabled", true);
        set => EditorPrefsCached.SetBool("tabify-reopenTabShortcutEnabled", value: value);
    }

    public static bool sidescrollEnabled
    {
        get => EditorPrefsCached.GetBool("tabify-sidescrollEnabled", Application.platform == RuntimePlatform.OSXEditor);
        set => EditorPrefsCached.SetBool("tabify-sidescrollEnabled", value: value);
    }

    public static float sidescrollSensitivity
    {
        get => EditorPrefsCached.GetFloat("tabify-sidescrollSensitivity", 1);
        set => EditorPrefsCached.SetFloat("tabify-sidescrollSensitivity", value: value);
    }

    public static bool reverseScrollDirectionEnabled
    {
        get => EditorPrefs.GetBool("tabify-reverseScrollDirectionDirection", false);
        set => EditorPrefs.SetBool("tabify-reverseScrollDirectionDirection", value: value);
    }

    public static bool pluginDisabled
    {
        get => EditorPrefsCached.GetBool("tabify-pluginDisabled");
        set => EditorPrefsCached.SetBool("tabify-pluginDisabled", value: value);
    }

    private const string dir = "‽/Tabify/";
    private const string cmd = "Ctrl";

    private const string dragndrop = dir + "Create tabs with Drag-and-Drop";
    private const string reverseScrollDirection = dir + "Reverse direction";
    private const string addTabButton = dir + "Add Tab button";
    private const string closeTabButton = dir + "Close Tab button";
    private const string hideLockButton = dir + "Hide lock button";

    private const string defaultTabStyle = dir + "Tab style/Default";
    private const string largeTabs = dir + "Tab style/Large";
    private const string neatTabs = dir + "Tab style/Neat";

    private const string defaultBackgroundStyle = dir + "Background style/Default";
    private const string classicBackground = dir + "Background style/Classic";
    private const string greyBackground = dir + "Background style/Grey";

    private const string switchTabShortcut = dir + "Shift-Scroll to switch tab";
    private const string addTabShortcut = dir + cmd + "-T to add tab";
    private const string closeTabShortcut = dir + cmd + "-W to close tab";
    private const string reopenTabShortcut = dir + cmd + "-Shift-T to reopen closed tab";

    private const string sidescroll = dir + "Sidescroll to switch tab";
    private const string increaseSensitivity = dir + "Increase sensitivity";
    private const string decreaseSensitivity = dir + "Decrease sensitivity";

    private const string disablePlugin = dir + "Disable Tabify";

    [MenuItem(dir + "Features", false, 1)] private static void FeaturesHeader() { }
    [MenuItem(dir + "Features", true, 1)] private static bool FeaturesHeaderValidate() => false;

    [MenuItem(itemName: addTabButton, false, 3)]
    private static void ToggleAddTabButton()
    {
        addTabButtonEnabled = !addTabButtonEnabled;
        Tabify.RepaintAllDockAreas();
    }

    [MenuItem(itemName: addTabButton, true, 3)]
    private static bool ValidateAddTabButton()
    {
        Menu.SetChecked(menuPath: addTabButton, isChecked: addTabButtonEnabled);
        return !pluginDisabled;
    }

    [MenuItem(itemName: closeTabButton, false, 4)]
    private static void ToggleCloseTabButton()
    {
        closeTabButtonEnabled = !closeTabButtonEnabled;
        Tabify.RepaintAllDockAreas();
    }

    [MenuItem(itemName: closeTabButton, true, 4)]
    private static bool ValidateCloseTabButton()
    {
        Menu.SetChecked(menuPath: closeTabButton, isChecked: closeTabButtonEnabled);
        return !pluginDisabled;
    }

    [MenuItem(itemName: hideLockButton, false, 5)]
    private static void ToggleHideLockButton()
    {
        hideLockButtonEnabled = !hideLockButtonEnabled;
        Tabify.RepaintAllDockAreas();
    }

    [MenuItem(itemName: hideLockButton, true, 7)]
    private static bool ValidateHideLockButton()
    {
        Menu.SetChecked(menuPath: hideLockButton, isChecked: hideLockButtonEnabled);
        return !pluginDisabled;
    }

    [MenuItem(itemName: defaultTabStyle, false, 8)]
    private static void SetDefaultTabStyle()
    {
        tabStyle = 0;
    }

    [MenuItem(itemName: defaultTabStyle, true, 8)]
    private static bool ValidateDefaultTabStyle()
    {
        Menu.SetChecked(menuPath: defaultTabStyle, tabStyle == 0);
        return !pluginDisabled;
    }

    [MenuItem(itemName: largeTabs, false, 9)]
    private static void SetLargeTabStyle()
    {
        tabStyle = 1;
    }

    [MenuItem(itemName: largeTabs, true, 9)]
    private static bool ValidateLargeTabStyle()
    {
        Menu.SetChecked(menuPath: largeTabs, tabStyle == 1);
        return !pluginDisabled;
    }

    [MenuItem(itemName: neatTabs, false, 10)]
    private static void SetNeatTabStyle()
    {
        tabStyle = 2;
    }

    [MenuItem(itemName: neatTabs, true, 10)]
    private static bool ValidateNeatTabStyle()
    {
        Menu.SetChecked(menuPath: neatTabs, tabStyle == 2);
        return !pluginDisabled;
    }

    [MenuItem(itemName: defaultBackgroundStyle, false, 11)]
    private static void SetDefaultBackgroundStyle() => backgroundStyle = 0;

    [MenuItem(itemName: defaultBackgroundStyle, true, 11)]
    private static bool ValidateDefaultBackgroundStyle()
    {
        Menu.SetChecked(menuPath: defaultBackgroundStyle, backgroundStyle == 0);
        return !pluginDisabled;
    }

    [MenuItem(itemName: classicBackground, false, 12)]
    private static void SetClassicBackground() => backgroundStyle = 1;

    [MenuItem(itemName: classicBackground, true, 12)]
    private static bool ValidateClassicBackground()
    {
        Menu.SetChecked(menuPath: classicBackground, backgroundStyle == 1);
        return !pluginDisabled;
    }

    [MenuItem(itemName: greyBackground, false, 13)]
    private static void SetGreyBackground() => backgroundStyle = 2;

    [MenuItem(itemName: greyBackground, true, 13)]
    private static bool ValidateGreyBackground()
    {
        Menu.SetChecked(menuPath: greyBackground, backgroundStyle == 2);
        return !pluginDisabled;
    }

    [MenuItem(dir + "Shortcuts", false, 101)]
    private static void ShortcutsHeader() { }

    [MenuItem(dir + "Shortcuts", true, 101)]
    private static bool ShortcutsHeaderValidate() => false;

    [MenuItem(itemName: switchTabShortcut, false, 102)]
    private static void ToggleSwitchTabShortcut() => switchTabShortcutEnabled = !switchTabShortcutEnabled;

    [MenuItem(itemName: switchTabShortcut, true, 102)]
    private static bool ValidateSwitchTabShortcut()
    {
        Menu.SetChecked(menuPath: switchTabShortcut, isChecked: switchTabShortcutEnabled);
        return !pluginDisabled;
    }

    [MenuItem(itemName: addTabShortcut, false, 103)]
    private static void ToggleAddTabShortcut() => addTabShortcutEnabled = !addTabShortcutEnabled;

    [MenuItem(itemName: addTabShortcut, true, 103)]
    private static bool ValidateAddTabShortcut()
    {
        Menu.SetChecked(menuPath: addTabShortcut, isChecked: addTabShortcutEnabled);
        return !pluginDisabled;
    }

    [MenuItem(itemName: closeTabShortcut, false, 104)]
    private static void ToggleCloseTabShortcut() => closeTabShortcutEnabled = !closeTabShortcutEnabled;

    [MenuItem(itemName: closeTabShortcut, true, 104)]
    private static bool ValidateCloseTabShortcut()
    {
        Menu.SetChecked(menuPath: closeTabShortcut, isChecked: closeTabShortcutEnabled);
        return !pluginDisabled;
    }

    [MenuItem(itemName: reopenTabShortcut, false, 105)]
    private static void ToggleReopenTabShortcut() => reopenTabShortcutEnabled = !reopenTabShortcutEnabled;

    [MenuItem(itemName: reopenTabShortcut, true, 105)]
    private static bool ValidateReopenTabShortcut()
    {
        Menu.SetChecked(menuPath: reopenTabShortcut, isChecked: reopenTabShortcutEnabled);
        return !pluginDisabled;
    }

    [MenuItem(dir + "More", false, 10001)] private static void MoreHeader() { }
    [MenuItem(dir + "More", true, 10001)] private static bool MoreHeaderValidate() => false;

    [MenuItem(itemName: disablePlugin, false, 100001)]
    private static void ToggleDisablePlugin()
    {
        pluginDisabled = !pluginDisabled;
        CompilationPipeline.RequestScriptCompilation();
    }

    [MenuItem(itemName: disablePlugin, true, 100001)]
    private static bool ValidateDisablePlugin()
    {
        Menu.SetChecked(menuPath: disablePlugin, isChecked: pluginDisabled);
        return true;
    }
}
#endif