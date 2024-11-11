using UnityEngine;

//quick and easy UI from code
public class ScreenManager : MonoBehaviour
{
    public enum ScreenType { Title, Options, Game }
    private ScreenType _currentScreen;

    private GameObject _canvas;
    private GameObject _currentPanel;

    void Start()
    {
        _canvas = UIBuilder.CreateCanvas();
        ShowScreen(ScreenType.Title);
    }

    public void ShowScreen(ScreenType screen)
    {
        if (_currentPanel != null) Destroy(_currentPanel);
        _currentScreen = screen;

        _currentPanel = screen switch
        {
            ScreenType.Title => CreateTitleScreen(),
            ScreenType.Options => CreateOptionsScreen(),
            ScreenType.Game => CreateGameScreen(),
            _ => _currentPanel
        };
    }

    private GameObject CreateTitleScreen()
    {
        var panel = UIBuilder.CreatePanel(_canvas.transform, "TitleScreen", new Vector2(800, 600));
        UIBuilder.CreateButton(panel.transform, "Play", () => ShowScreen(ScreenType.Game), color: Color.white, size: new Vector2(160, 40));
        UIBuilder.CreateButton(panel.transform, "Options", () => ShowScreen(ScreenType.Options), color: Color.white, size: new Vector2(160, 40), position: new Vector2(0, -50));
        return panel;
    }

    private GameObject CreateOptionsScreen()
    {
        var panel = UIBuilder.CreatePanel(_canvas.transform, "OptionsScreen", new Vector2(800, 600));
        UIBuilder.CreateButton(panel.transform, "Back", () => ShowScreen(ScreenType.Title), color: Color.white, size: new Vector2(160, 40), position: new Vector2(0, -200));
        return panel;
    }

    private GameObject CreateGameScreen() => 
        UIBuilder.CreatePanel(_canvas.transform, "GameScreen", new Vector2(800, 600));
}
