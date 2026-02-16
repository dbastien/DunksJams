using System;

public sealed class ScreenButtonDef
{
    public string Label { get; }
    public ScreenAction Action { get; }
    public Action<GameFlowManager> CustomAction { get; }

    public ScreenButtonDef(string label, ScreenAction action = ScreenAction.None,
        Action<GameFlowManager> customAction = null)
    {
        Label = label;
        Action = action;
        CustomAction = customAction;
    }

    public void Execute(GameFlowManager flow)
    {
        if (CustomAction != null)
        {
            CustomAction(flow);
            return;
        }

        flow?.ExecuteAction(Action);
    }

    public static ScreenButtonDef StartGame(string label = "Start Game") => new(label, ScreenAction.StartGame);
    public static ScreenButtonDef OpenConfig(string label = "Options") => new(label, ScreenAction.OpenConfig);
    public static ScreenButtonDef Back(string label = "Back") => new(label, ScreenAction.CloseScreen);
    public static ScreenButtonDef Resume(string label = "Resume") => new(label, ScreenAction.ResumeGame);
    public static ScreenButtonDef MainMenu(string label = "Main Menu") => new(label, ScreenAction.MainMenu);
    public static ScreenButtonDef Restart(string label = "Restart") => new(label, ScreenAction.Restart);
    public static ScreenButtonDef Quit(string label = "Quit") => new(label, ScreenAction.Quit);
}
