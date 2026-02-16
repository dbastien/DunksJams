using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogDatabase", menuName = "Interroband/Dialog/Database")]
public class DialogDatabase : ScriptableObject
{
    public List<DialogConversation> conversations = new();
    public List<DialogDatabase> includedDatabases = new();

    public DialogConversation GetConversation(string name)
    {
        foreach (var conv in conversations)
            if (conv.conversationName == name) return conv;

        foreach (var db in includedDatabases)
        {
            var found = db.GetConversation(name);
            if (found != null) return found;
        }

        return null;
    }
}