using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Settings provider for Toolbox preferences (Edit > Preferences > Toolbox).</summary>
public class ToolboxUserSettingsProvider : SettingsProvider
{
    ToolboxUserSettings currentSettings;
    SerializedObject serializedObject;
    ToolbarCustomizeDrawer toolbarCustomize;

    [SettingsProvider]
    public static SettingsProvider CreateProvider()
    {
        return new ToolboxUserSettingsProvider("Preferences/Toolbox", SettingsScope.User)
        {
            keywords = new[] { "Toolbox" }
        };
    }

    public ToolboxUserSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        currentSettings = ToolboxUserSettings.GetOrCreateSettings();
        serializedObject = new SerializedObject(currentSettings);
        toolbarCustomize = new ToolbarCustomizeDrawer();
        toolbarCustomize.Setup(currentSettings);
        base.OnActivate(searchContext, rootElement);
    }

    public override void OnGUI(string searchContext) => toolbarCustomize.Draw();

    public override void OnDeactivate()
    {
        toolbarCustomize?.Teardown();
        toolbarCustomize = null;
        base.OnDeactivate();
    }
}
