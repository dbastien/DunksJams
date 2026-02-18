using System.Collections.Generic;
using UnityEngine;

public enum BarkPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public class BarkManager : MonoBehaviour
{
    public static BarkManager Instance { get; private set; }

    private Dictionary<DialogueActor, float> _actorBarkCooldowns = new();
    private Dictionary<DialogueActor, List<string>> _actorBarkHistory = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Bark
        (DialogueActor actor, string[] possibleLines, BarkPriority priority = BarkPriority.Normal, float duration = 3f)
    {
        if (actor == null || possibleLines == null || possibleLines.Length == 0) return;

        // Simple cooldown check per actor
        if (_actorBarkCooldowns.TryGetValue(actor, out float cooldown) &&
            Time.time < cooldown &&
            priority != BarkPriority.Urgent) return;

        // History randomization: avoid repeating the same line immediately if possible
        string line = PickRandomLine(actor, possibleLines);

        actor.Bark(line, duration);
        _actorBarkCooldowns[actor] = Time.time + duration + 1f; // cooldown = duration + 1s buffer
    }

    private string PickRandomLine(DialogueActor actor, string[] lines)
    {
        if (lines.Length <= 1) return lines[0];

        if (!_actorBarkHistory.TryGetValue(actor, out List<string> history))
        {
            history = new List<string>();
            _actorBarkHistory[actor] = history;
        }

        // Filter out lines that were recently used
        var availableLines = new List<string>(lines);
        if (history.Count > 0)
            foreach (string used in history)
                if (availableLines.Count > 1)
                    availableLines.Remove(used);

        string picked = availableLines[Random.Range(0, availableLines.Count)];

        // Update history
        history.Add(picked);
        if (history.Count > lines.Length / 2 + 1) history.RemoveAt(0);

        return picked;
    }
}