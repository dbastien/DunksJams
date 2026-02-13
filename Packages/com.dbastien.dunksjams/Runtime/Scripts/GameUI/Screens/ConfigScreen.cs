using UnityEngine;

public class ConfigScreen : UIScreen
{
    public ConfigScreen(Transform canvas) : base(canvas)
    {
    }

    public override void Setup()
    {
        UIBuilder.CreateButton(Panel.transform, "Audio", () => DLog.Log("Audio settings"), size: new Vector2(200, 50));
        UIBuilder.CreateButton(Panel.transform, "Graphics", () => DLog.Log("Graphics settings"),
            size: new Vector2(200, 50), position: new Vector2(0, -60));
        UIBuilder.CreateButton(Panel.transform, "Controls", () => DLog.Log("Controls settings"),
            size: new Vector2(200, 50), position: new Vector2(0, -120));
        UIBuilder.CreateButton(Panel.transform, "Back", OnBack, size: new Vector2(200, 50),
            position: new Vector2(0, -180));
    }

    void OnBack() => UIManager.Instance?.HideCurrentScreen();
}