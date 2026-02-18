using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(SteeringAgent))]
public class SteeringAgentEditor : Editor
{
    private SerializedProperty _behaviors, _targetingStrategy;
    private static List<Type> _cachedBehaviorTypes;

    private void OnEnable()
    {
        _behaviors = serializedObject.FindProperty("behaviors");
        _targetingStrategy = serializedObject.FindProperty("targetingStrategy");

        _cachedBehaviorTypes ??= ReflectionUtils.GetNonGenericDerivedTypes<SteeringBehavior>();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_targetingStrategy, new GUIContent("Targeting Strategy"));
        EditorGUILayout.PropertyField(_behaviors, true);

        if (GUILayout.Button("Add Behavior"))
        {
            var menu = new GenericMenu();
            foreach (Type behaviorType in _cachedBehaviorTypes)
                menu.AddItem(new GUIContent(behaviorType.Name), false, () => AddBehavior(behaviorType));
            menu.ShowAsContext();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddBehavior(Type behaviorType)
    {
        var newBehavior = CreateInstance(behaviorType) as SteeringBehavior;
        AssetDatabase.CreateAsset(newBehavior, $"Assets/{behaviorType.Name}Behavior.asset");
        AssetDatabase.SaveAssets();

        var agent = (SteeringAgent)target;
        ArrayUtility.Add(ref agent.behaviors, newBehavior);
        EditorUtility.SetDirty(agent);
    }
}