using UnityEngine;

public class PauseScreen : UIScreen
{
    public PauseScreen(Transform canvas) : base(canvas) { }

    public override void Setup()
    {
        UIBuilder.CreateButton(Panel.transform, "Resume", OnResume, size: new Vector2(200, 50));
        UIBuilder.CreateButton(Panel.transform, "Options", OnOptions, size: new Vector2(200, 50), position: new Vector2(0, -60));
        UIBuilder.CreateButton(Panel.transform, "Main Menu", OnMainMenu, size: new Vector2(200, 50), position: new Vector2(0, -120));
    }

    protected override void OnShow()
    {
        Time.timeScale = 0f;
        AudioSystem.Instance?.PauseAll();
    }

    protected override void OnHide()
    {
        Time.timeScale = 1f;
        AudioSystem.Instance?.ResumeAll();
    }

    void OnResume() => UIManager.Instance?.HideCurrentScreen();
    void OnOptions() => UIManager.Instance?.ShowScreen<ConfigScreen>();
    void OnMainMenu()
    {
        Time.timeScale = 1f;
        UIManager.Instance?.ShowScreen<StartScreen>(addToStack: false);
    }
}
