using UnityEngine;
using TMPro;
using System.Text;

public class DialogHistoryView : MonoBehaviour
{
    public TextMeshProUGUI historyText;
    public GameObject viewPanel;

    public void Show()
    {
        if (viewPanel != null) viewPanel.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        if (viewPanel != null) viewPanel.SetActive(false);
    }

    public void Toggle()
    {
        if (viewPanel == null) return;
        if (viewPanel.activeSelf) Hide();
        else Show();
    }

    public void Refresh()
    {
        if (historyText == null || DialogHistoryManager.Instance == null) return;

        var history = DialogHistoryManager.Instance.GetHistory();
        StringBuilder sb = new StringBuilder();

        foreach (var item in history)
        {
            if (item.isChoice)
                sb.AppendLine($"<color=#AAAAAA><i>Choice: {item.text}</i></color>");
            else
                sb.AppendLine($"<b>{item.actorName}:</b> {item.text}");
            sb.AppendLine();
        }

        historyText.text = sb.ToString();
    }
}