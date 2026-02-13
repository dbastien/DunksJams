using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : UIScreen
{
    Text scoreText;

    public GameOverScreen(Transform canvas) : base(canvas)
    {
    }

    public override void Setup()
    {
        var scoreObj = UIBuilder.CreateUIElement("ScoreText", Panel.transform, typeof(Text));
        UIBuilder.SetupRectTransform(scoreObj.GetComponent<RectTransform>(), new Vector2(400, 100),
            new Vector2(0, 100));
        scoreText = UIBuilder.InitText(scoreObj, "Game Over", Resources.GetBuiltinResource<Font>("Arial.ttf"),
            Color.white);

        UIBuilder.CreateButton(Panel.transform, "Restart", OnRestart, size: new Vector2(200, 50),
            position: new Vector2(0, -50));
        UIBuilder.CreateButton(Panel.transform, "Main Menu", OnMainMenu, size: new Vector2(200, 50),
            position: new Vector2(0, -110));
    }

    public void SetScore(int score) => scoreText.text = $"Game Over\nScore: {score}";

    void OnRestart()
    {
        DLog.Log("Restarting game...");
        UIManager.Instance?.HideCurrentScreen();
    }

    void OnMainMenu() => UIManager.Instance?.ShowScreen<StartScreen>(false);
}