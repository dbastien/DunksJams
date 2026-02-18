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
    public string Name { get; }
    public ToolbarItemPosition Position { get; }
    public ToolbarItemAnchor Anchor { get; }
    public int Priority { get; }
    public bool Enabled { get; }
    public void Init();
    public SettingsProvider GetSettingsProvider();
    public void DrawInToolbar();
    public void DrawInWindow();
}

public interface IUpdatingToolbarItem : IToolbarItem
{
    public void Update(double timeDelta);
}