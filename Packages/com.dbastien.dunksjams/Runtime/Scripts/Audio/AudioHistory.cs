using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioHistory
{
    readonly Queue<AudioHistoryEntry> queue = new();
    int maxSize = 50;
    string search = string.Empty;
    Vector2 scrollPos;

    public void Enqueue(AudioHistoryEntry entry)
    {
        queue.Enqueue(entry);
        if (queue.Count > maxSize) queue.Dequeue();
    }

    public AudioHistoryEntry Get(int index) => index < queue.Count ? queue.ElementAt(index) : queue.ElementAt(0);

    public void DoOnGui()
    {
        GUILayout.Label("------- Audio Debug -------");
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Search: ");
        GUILayout.FlexibleSpace();
        search = GUILayout.TextField(search, GUILayout.MinWidth(1000f));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Label($"{"Event"}\t{"Object"}\t{"Time"}");
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        foreach (var entry in queue)
        {
            if (!string.IsNullOrEmpty(entry.eventName) &&
                !entry.eventName.StartsWith(search) &&
                !entry.eventName.Contains(search))
                continue;

            GUILayout.BeginHorizontal();
            var prev = GUI.contentColor;
            if (entry.failed) GUI.contentColor = Color.red;

            GUILayout.Label(entry.eventName);
            GUILayout.FlexibleSpace();
            GUILayout.Label(entry.objectName);
            GUILayout.FlexibleSpace();
            GUILayout.Label(entry.time.ToString("F2"));
            GUI.contentColor = prev;
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
}

public struct AudioHistoryEntry
{
    public string eventName;
    public string objectName;
    public bool failed;
    public float time;

    public AudioHistoryEntry(string eventName, string objectName, bool failed = false)
    {
        this.eventName = eventName;
        this.objectName = objectName;
        this.failed = failed;
        time = Time.time;
    }
}
