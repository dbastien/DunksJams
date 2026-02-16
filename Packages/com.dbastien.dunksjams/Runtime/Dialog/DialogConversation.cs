using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConversation", menuName = "Interroband/Dialog/Conversation")]
public class DialogConversation : ScriptableObject
    {
    public string conversationName;
    public List<DialogEntry> entries = new();
    public string startNodeGUID;

    public DialogEntry GetEntry(string guid) => entries.Find(e => e.nodeID == guid);

    public DialogEntry GetStartEntry()
    {
        if (string.IsNullOrEmpty(startNodeGUID))
            return entries.Count > 0 ? entries[0] : null;
        return GetEntry(startNodeGUID);
    }
}