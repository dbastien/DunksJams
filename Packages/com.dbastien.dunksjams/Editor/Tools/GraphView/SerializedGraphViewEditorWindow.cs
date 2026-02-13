using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SerializedGraphViewEditorWindow<TGraphView, TNode, TEdge> : EditorWindow
    where TGraphView : SerializedGraphView<TNode, TEdge>, new()
    where TNode : SerializedGraphNode, new()
    where TEdge : Edge, new()
{
    protected TGraphView _graphView;
    DropdownField _nodeTypeDropdown;

    protected virtual void OnEnable()
    {
        titleContent = new GUIContent(GetWindowTitle());
        _graphView = new TGraphView { name = GetGraphViewName() };

        _graphView.StretchToParentSize();
        rootVisualElement.Clear();
        rootVisualElement.Add(_graphView);

        SetupToolbar();
        LoadGraph();
    }

    protected virtual void OnDisable()
    {
        if (_graphView != null) SaveGraph();
        rootVisualElement.Remove(_graphView);
    }

    protected abstract string GetWindowTitle();
    protected abstract string GetGraphViewName();

    protected virtual void LoadGraph() => _graphView?.LoadGraph();
    protected virtual void SaveGraph() => _graphView?.SaveGraph();

    void SetupToolbar()
    {
        var toolbar = new Toolbar();
        toolbar.Add(new Button(SaveGraph) { text = "Save" });
        toolbar.Add(new Button(LoadGraph) { text = "Load" });

        var nodeTypes = _graphView.GetNodeTypes().Select(type => type.Name).ToList();
        _nodeTypeDropdown = new DropdownField("Create Node", nodeTypes, 0);
        toolbar.Add(_nodeTypeDropdown);

        toolbar.Add(new Button(CreateSelectedNode) { text = "Add Node" });

        rootVisualElement.Add(toolbar);
    }

    void CreateSelectedNode()
    {
        var selectedTypeName = _nodeTypeDropdown.value;
        var nodeType = _graphView.GetNodeTypes().FirstOrDefault(type => type.Name == selectedTypeName);
        if (nodeType != null) _graphView.AddNode(nodeType, SerializedGraphNode.DefaultSize);
    }
}