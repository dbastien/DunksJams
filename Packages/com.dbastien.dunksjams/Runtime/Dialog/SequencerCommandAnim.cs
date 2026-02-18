using UnityEngine;

/// <summary>
/// Anim(target, trigger)
/// </summary>
public class SequencerCommandAnim : SequencerCommand
{
    protected override void Start()
    {
        string targetName = GetParameter(0);
        string trigger = GetParameter(1);

        Transform target = DialogUtility.FindTransform(targetName);
        if (target != null)
        {
            var anim = target.GetComponentInChildren<Animator>();
            if (anim != null)
                anim.SetTrigger(trigger);
            else
                DLog.LogW($"[Sequencer] Animator not found on '{targetName}'.");
        }
        else { DLog.LogW($"[Sequencer] Anim target '{targetName}' not found."); }

        Stop();
    }
}