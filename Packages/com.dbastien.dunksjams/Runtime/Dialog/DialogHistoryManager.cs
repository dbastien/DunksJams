using System.Collections.Generic;
using UnityEngine;

public class DialogHistoryItem
{
    public string actorName;
    public string text;
    public float timestamp;
    public bool isChoice;

    public DialogHistoryItem(string actor, string content, bool choice = false)
    {
        actorName = actor;
        text = content;
        timestamp = Time.time;
        isChoice = choice;
    }
}

public class DialogHistoryManager : MonoBehaviour
{
    public static DialogHistoryManager Instance { get; private set; }

    private List<DialogHistoryItem> _history = new();
    public int maxHistory = 100;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddToHistory(string actor, string text, bool isChoice = false)
    {
        _history.Add(new DialogHistoryItem(actor, text, isChoice));
        if (_history.Count > maxHistory) _history.RemoveAt(0);
    }

    public List<DialogHistoryItem> GetHistory() => _history;

    public void Clear() => _history.Clear();
}