using System;
using System.Collections.Generic;

public sealed class ScreenSpec
{
    public string Title { get; }
    public IReadOnlyList<ScreenButtonDef> Buttons { get; }

    public ScreenSpec(string title, IReadOnlyList<ScreenButtonDef> buttons)
    {
        Title = title;
        Buttons = buttons ?? Array.Empty<ScreenButtonDef>();
    }

    public static ScreenSpec DefaultStart(string title = "Game") => new(title, new[]
    {
        ScreenButtonDef.StartGame(),
        ScreenButtonDef.OpenConfig(),
        ScreenButtonDef.Quit()
    });

    public static ScreenSpec DefaultPause() => new("Paused", new[]
    {
        ScreenButtonDef.Resume(),
        ScreenButtonDef.OpenConfig(),
        ScreenButtonDef.MainMenu()
    });

    public static ScreenSpec DefaultEnd() => new("Game Over", new[]
    {
        ScreenButtonDef.Restart(),
        ScreenButtonDef.MainMenu()
    });

    public static ScreenSpec DefaultConfig() => new("Options", new[]
    {
        ScreenButtonDef.Back()
    });
}