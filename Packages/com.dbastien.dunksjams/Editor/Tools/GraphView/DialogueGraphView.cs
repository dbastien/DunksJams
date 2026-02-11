using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using Utilities;

public class DialogueEditorWindow : SerializedGraphViewEditorWindow<DialogueGraphView, DialogueNode, Edge>
{
    [MenuItem("‽/Dialogue Editor")]
    public static void ShowWindow() => GetWindow<DialogueEditorWindow>();
    protected override string GetWindowTitle() => "Dialogue Editor";
    protected override string GetGraphViewName() => "DialogueGraph";
}

public class DialogueGraphView : SerializedGraphView<DialogueNode, Edge>
{
    protected override string FilePath => "Assets/DialogueGraph.json";
    public override IEnumerable<Type> GetNodeTypes() => ReflectionUtils.GetNonGenericDerivedTypes<DialogueNodeBase>();
}