using UnityEngine;

public abstract class BaseScreen
{
    protected GameObject Panel { get; private set; }

    public BaseScreen(Transform parent, string name = "ScreenPanel") => 
        Panel = UIBuilder.CreatePanel(parent, name);

    public abstract void SetupScreen();
    public virtual void Show() => Panel.SetActive(true);
    public virtual void Hide() => Panel.SetActive(false);
}

public class TitleScreen : BaseScreen
{
    public TitleScreen(Transform parent) : base(parent, "TitleScreen") { }

    public override void SetupScreen()
    {
        UIBuilder.CreateButton(Panel.transform, "Play", () => DLog.Log("Play game"));
        UIBuilder.CreateButton(Panel.transform, "Options", () => DLog.Log("Open options"));
    }
}

public class OptionsScreen : BaseScreen
{
    public OptionsScreen(Transform parent) : base(parent, "OptionsScreen") { }

    public override void SetupScreen() => 
        UIBuilder.CreateButton(Panel.transform, "Back", () => DLog.Log("Back to title"));
}