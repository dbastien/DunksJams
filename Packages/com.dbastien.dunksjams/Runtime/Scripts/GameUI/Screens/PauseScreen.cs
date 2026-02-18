using UnityEngine;

public class PauseScreen : MenuScreenBase
{
    public PauseScreen(Transform canvas) : base(canvas) { }

    protected override void OnShow()
    {
        PauseManager.PushPause();
        AudioSystem.Instance?.PauseAll();

        GameDefinition def = GameFlowManager.Instance?.ActiveDefinition;
        RebuildMenu(def?.PauseScreen ?? ScreenSpec.DefaultPause(), "Paused");
    }

    protected override void OnHide()
    {
        AudioSystem.Instance?.ResumeAll();
        PauseManager.PopPause();
    }
}