using UnityEngine;

/// <summary>
/// Camera(target, duration, [offset])
/// Moves the main camera to look at a target.
/// </summary>
public class SequencerCommandCamera : SequencerCommand
{
    private Transform _target;
    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _duration;
    private float _elapsed;

    protected override void Start()
    {
        string targetName = GetParameter(0);
        _target = DialogUtility.FindTransform(targetName);
        _duration = GetParameterFloat(1, 1f);

        if (_target == null)
        {
            DLog.LogW($"[Sequencer] Camera target '{targetName}' not found.");
            Stop();
            return;
        }

        _startPos = Camera.main.transform.position;
        // Simple offset for now, could be improved to handle camera relative positions
        _endPos = _target.position + new Vector3(0, 2, -5);

        if (_duration <= 0)
        {
            Camera.main.transform.position = _endPos;
            Camera.main.transform.LookAt(_target);
            Stop();
        }
    }

    private void Update()
    {
        if (_target == null) return;

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        // Smooth step
        t = t * t * (3f - 2f * t);

        Camera.main.transform.position = Vector3.Lerp(_startPos, _endPos, t);
        Camera.main.transform.LookAt(_target);

        if (_elapsed >= _duration) Stop();
    }
}