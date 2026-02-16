using System.Collections;
using UnityEngine;

/// <summary>
/// Wait(duration)
/// </summary>
public class SequencerCommandWait : SequencerCommand
{
    protected override void Start()
    {
        float duration = GetParameterFloat(0, 1f);
        StartCoroutine(WaitRoutine(duration));
    }

    private IEnumerator WaitRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        Stop();
    }
}