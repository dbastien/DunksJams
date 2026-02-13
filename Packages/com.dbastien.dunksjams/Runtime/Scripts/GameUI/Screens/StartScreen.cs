using UnityEngine;

public class StartScreen : UIScreen
{
    public StartScreen(Transform canvas) : base(canvas)
    {
    }

    public override void Setup()
    {
        UIBuilder.CreateButton(Panel.transform, "Start Game", OnStartGame, size: new Vector2(200, 50));
        UIBuilder.CreateButton(Panel.transform, "Options", OnOptions, size: new Vector2(200, 50),
            position: new Vector2(0, -60));
        UIBuilder.CreateButton(Panel.transform, "Quit", OnQuit, size: new Vector2(200, 50),
            position: new Vector2(0, -120));
    }

    void OnStartGame()
    {
        DLog.Log("Starting game...");
        UIManager.Instance?.HideCurrentScreen();
    }

    void OnOptions()
    {
        UIManager.Instance?.ShowScreen<ConfigScreen>();
    }

    void OnQuit()
    {
        DLog.Log("Quitting game...");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}