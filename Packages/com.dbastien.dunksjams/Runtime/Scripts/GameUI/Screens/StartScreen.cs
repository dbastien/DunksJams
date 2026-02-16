using UnityEngine;

public class StartScreen : MenuScreenBase
{
    public StartScreen(Transform canvas) : base(canvas)
    {
    }

    protected override void OnShow()
    {
        var def = GameFlowManager.Instance?.ActiveDefinition;
        RebuildMenu(def?.StartScreen ?? ScreenSpec.DefaultStart(def?.Title ?? "Game"), "Game");
    }
}
