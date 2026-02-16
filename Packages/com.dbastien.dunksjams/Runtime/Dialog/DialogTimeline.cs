using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.1f, 0.8f, 0.4f)]
[TrackClipType(typeof(DialogClip))]
public class DialogTrack : TrackAsset
{
}

public class DialogClip : PlayableAsset, ITimelineClipAsset
{
    public string actorName;
    [TextArea(3, 10)]
    public string text;
    public string sequence;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.actorName = actorName;
        behaviour.text = text;
        behaviour.sequence = sequence;
        return playable;
    }
}

public class DialogBehaviour : PlayableBehaviour
{
    public string actorName;
    public string text;
    public string sequence;

    private bool _hasStarted;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (_hasStarted) return;
        _hasStarted = true;

        if (DialogManager.Instance != null)
        {
            // We create a temporary line/entry to show
            var line = new DialogLine { actorName = actorName, text = text, sequence = sequence };
            var entry = ScriptableObject.CreateInstance<DialogEntry>();
            entry.lines.Add(line);

            // Note: This is a bit of a hack to show a single line via the UI
            // A better way would be a specific method in DialogManager for manual barks/lines.
            DialogManager.Instance.ManualShowLine(entry, line);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info) => _hasStarted = false;
}