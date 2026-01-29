using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UIComponents
{
    public static GameObject CreateMenu(Transform parent, string[] options, UnityAction[] actions)
    {
        var menu = UIBuilder.CreatePanel(parent, "MenuPanel");
        var layout = UIBuilder.CreateLayout(menu.transform, vertical: true, spacing: 10);

        for (int i = 0; i < options.Length; i++)
            UIBuilder.CreateButton(layout.transform, options[i], actions[i]);

        return menu;
    }

    public static GameObject CreateDialog(Transform parent, string msg, UnityAction onConfirm)
    {
        var dialog = UIBuilder.CreatePanel(parent, "Dialog", new Vector2(400, 200));
        UIBuilder.InitText(UIBuilder.CreateUIElement("Message", dialog.transform, typeof(Text)), msg, font: null, color: Color.white);
        UIBuilder.CreateButton(dialog.transform, "OK", onConfirm, position: new Vector2(0, -50));
        return dialog;
    }

    public static GameObject CreateHUD(Transform parent, string[] labels, out Text[] texts)
    {
        var hud = UIBuilder.CreatePanel(parent, "HUDPanel");
        var layout = UIBuilder.CreateLayout(hud.transform, vertical: true, alignment: TextAnchor.UpperLeft);

        texts = new Text[labels.Length];
        for (int i = 0; i < labels.Length; ++i)
            texts[i] = UIBuilder.InitText(UIBuilder.CreateUIElement(labels[i], layout.transform, typeof(Text)), $"{labels[i]}: 0", font: null, color: Color.white);

        return hud;
    }
}