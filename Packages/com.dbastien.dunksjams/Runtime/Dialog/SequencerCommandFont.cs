using UnityEngine;
using TMPro;

public class SequencerCommandFont : SequencerCommand
{
    public void Start()
    {
        string fontName = GetParameter(0);
        string targetName = GetParameter(1, "Dialog Text");

        Transform target = DialogUtility.FindTransform(targetName);
        if (target != null)
        {
            TMP_Text text = target.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/" + fontName);
                if (font != null)
                    text.font = font;
                else
                    DLog.LogW($"[Sequencer] Font 'Fonts/{fontName}' not found.");
            }
        }

        Stop();
    }
}