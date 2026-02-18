using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogDatabase", menuName = "Interroband/Dialog/Database")]
public class DialogDatabase : ScriptableObject
{
    public List<DialogConversation> conversations = new();
    public List<DialogDatabase> includedDatabases = new();

    public DialogConversation GetConversation(string name)
    {
        foreach (DialogConversation conv in conversations)
            if (conv.conversationName == name)
                return conv;

        foreach (DialogDatabase db in includedDatabases)
        {
            DialogConversation found = db.GetConversation(name);
            if (found != null) return found;
        }

        return null;
    }
}