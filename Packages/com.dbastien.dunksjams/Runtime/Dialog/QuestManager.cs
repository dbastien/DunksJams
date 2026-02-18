using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    Unassigned,
    Active,
    Success,
    Failure,
    Abandoned
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private Dictionary<string, QuestState> _questStates = new();

    public event System.Action<string, QuestState> OnQuestStateChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetQuestState(string questName, QuestState state)
    {
        _questStates[questName] = state;
        DLog.Log($"Quest '{questName}' changed to {state}");
        OnQuestStateChanged?.Invoke(questName, state);

        // Also store as a dialog variable for easy access
        if (DialogManager.Instance != null)
            DialogManager.Instance.SetVariable($"Quest[\"{questName}\"].State", state.ToString());
    }

    public QuestState GetQuestState(string questName)
    {
        if (_questStates.TryGetValue(questName, out QuestState state)) return state;
        return QuestState.Unassigned;
    }

    public void SetQuestState(string questName, string stateName)
    {
        if (System.Enum.TryParse(stateName, true, out QuestState state)) SetQuestState(questName, state);
    }
}