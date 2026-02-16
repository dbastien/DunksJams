using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(DialogConversation))]
public class DialogConversationEditor : Editor
    {
        private DialogConversation _conversation;

        private void OnEnable()
        {
            _conversation = (DialogConversation)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField("Conversation Settings", EditorStyles.boldLabel);
            _conversation.conversationName = EditorGUILayout.TextField("Name", _conversation.conversationName);
            _conversation.startNodeGUID = EditorGUILayout.TextField("Start Node GUID", _conversation.startNodeGUID);

            EditorGUILayout.Space();
            
            if (GUILayout.Button("Open Graph Editor", GUILayout.Height(40)))
            {
                DialogGraphWindow.Open(_conversation);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Nodes: {_conversation.entries.Count}", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_conversation);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }