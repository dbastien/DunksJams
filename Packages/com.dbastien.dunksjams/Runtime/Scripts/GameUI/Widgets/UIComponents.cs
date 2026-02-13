using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UIComponents
{
    public static GameObject CreateMenu(Transform parent, string[] options, UnityAction[] actions)
    {
        var menu = UIBuilder.CreatePanel(parent, "MenuPanel");
        var layout = UIBuilder.CreateLayout(menu.transform, true, 10);

        for (var i = 0; i < options.Length; i++)
            UIBuilder.CreateButton(layout.transform, options[i], actions[i]);

        return menu;
    }

    public static GameObject CreateDialog(Transform parent, string msg, UnityAction onConfirm)
    {
        var dialog = UIBuilder.CreatePanel(parent, "Dialog", new Vector2(400, 200));
        UIBuilder.InitText(UIBuilder.CreateUIElement("Message", dialog.transform, typeof(Text)), msg, null,
            Color.white);
        UIBuilder.CreateButton(dialog.transform, "OK", onConfirm, position: new Vector2(0, -50));
        return dialog;
    }

    public static GameObject CreateHUD(Transform parent, string[] labels, out Text[] texts)
    {
        var hud = UIBuilder.CreatePanel(parent, "HUDPanel");
        var layout = UIBuilder.CreateLayout(hud.transform, true, alignment: TextAnchor.UpperLeft);

        texts = new Text[labels.Length];
        for (var i = 0; i < labels.Length; ++i)
        {
            texts[i] = UIBuilder.InitText(UIBuilder.CreateUIElement(labels[i], layout.transform, typeof(Text)),
                $"{labels[i]}: 0", null, Color.white);
        }

        return hud;
    }
}