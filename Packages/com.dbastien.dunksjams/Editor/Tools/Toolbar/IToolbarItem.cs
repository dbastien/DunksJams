using UnityEditor;

public enum ToolbarItemPosition
{
    Left = 0,
    Right = 1
}

public enum ToolbarItemAnchor
{
    Left = 0,
    Right = 1,
    Center = 2
}

public interface IToolbarItem
{
    string Name { get; }
    ToolbarItemPosition Position { get; }
    ToolbarItemAnchor Anchor { get; }
    int Priority { get; }
    bool Enabled { get; }
    void Init();
    SettingsProvider GetSettingsProvider();
    void DrawInToolbar();
    void DrawInWindow();
}

public interface IUpdatingToolbarItem : IToolbarItem
{
    void Update(double timeDelta);
}