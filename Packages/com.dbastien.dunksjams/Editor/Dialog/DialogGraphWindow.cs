using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

public class DialogGraphWindow : EditorWindow
{
    private DialogConversation _conversation;
    private DialogGraphView _graphView;
    private ObjectField _conversationField;
    private DialogCommandHistory _commandHistory;
    private Button _undoButton;
    private Button _redoButton;

    [MenuItem("Interroband/Dialog/Graph Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<DialogGraphWindow>("Dialog Graph");
        window.Show();
    }

    public static void Open(DialogConversation conversation)
    {
        var window = GetWindow<DialogGraphWindow>("Dialog Graph");
        window._conversation = conversation;
        window.LoadConversation();
        window.Show();
    }

    private void OnEnable()
    {
        rootVisualElement.Clear();

        // Initialize command history
        _commandHistory = new DialogCommandHistory();
        _commandHistory.OnHistoryChanged += UpdateUndoRedoButtons;

        GenerateToolbar();
        ConstructGraphView();

        // Register keyboard shortcuts
        rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
    }

    private void OnDisable()
    {
        if (_commandHistory != null)
        {
            _commandHistory.OnHistoryChanged -= UpdateUndoRedoButtons;
        }
        rootVisualElement.Remove(_graphView);
        rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown);
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        // Ctrl+Z for Undo
        if (evt.ctrlKey && evt.keyCode == KeyCode.Z && !evt.shiftKey)
        {
            if (_commandHistory.CanUndo)
            {
                _commandHistory.Undo();
                evt.StopPropagation();
            }
        }
        // Ctrl+Y or Ctrl+Shift+Z for Redo
        else if ((evt.ctrlKey && evt.keyCode == KeyCode.Y) || (evt.ctrlKey && evt.shiftKey && evt.keyCode == KeyCode.Z))
        {
            if (_commandHistory.CanRedo)
            {
                _commandHistory.Redo();
                evt.StopPropagation();
            }
        }
    }

    private void ConstructGraphView()
    {
        _graphView = new DialogGraphView(this, _commandHistory)
        {
            name = "Dialog Graph"
        };
        // Use flex grow instead of stretch to avoid covering toolbar
        _graphView.style.flexGrow = 1;
        rootVisualElement.Add(_graphView);
    }

    public DialogCommandHistory CommandHistory => _commandHistory;

    private void UpdateUndoRedoButtons()
    {
        if (_undoButton != null)
        {
            _undoButton.SetEnabled(_commandHistory.CanUndo);
            var undoName = _commandHistory.GetUndoCommandName();
            _undoButton.tooltip = undoName != null ? $"Undo {undoName} (Ctrl+Z)" : "Undo (Ctrl+Z)";
        }
        if (_redoButton != null)
        {
            _redoButton.SetEnabled(_commandHistory.CanRedo);
            var redoName = _commandHistory.GetRedoCommandName();
            _redoButton.tooltip = redoName != null ? $"Redo {redoName} (Ctrl+Y)" : "Redo (Ctrl+Y)";
        }
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var newButton = new Button(CreateNewConversation) { text = "New", tooltip = "Create a new conversation asset" };
        toolbar.Add(newButton);

        _undoButton = new Button(() => _commandHistory.Undo()) { text = "↶", tooltip = "Undo (Ctrl+Z)" };
        _undoButton.SetEnabled(false);
        toolbar.Add(_undoButton);

        _redoButton = new Button(() => _commandHistory.Redo()) { text = "↷", tooltip = "Redo (Ctrl+Y)" };
        _redoButton.SetEnabled(false);
        toolbar.Add(_redoButton);

        _conversationField = new ObjectField("Conversation")
        {
            objectType = typeof(DialogConversation),
            value = _conversation,
            tooltip = "Select a conversation asset to edit"
        };
        _conversationField.RegisterValueChangedCallback(evt =>
        {
            _conversation = evt.newValue as DialogConversation;
            LoadConversation();
        });
        toolbar.Add(_conversationField);

        toolbar.Add(new ToolbarSpacer());

        var createNodeMenu = new ToolbarMenu { text = "Create Node", tooltip = "Create a new node in the center of the view" };
        createNodeMenu.menu.AppendAction("Dialogue Node", action => _graphView.CreateNewNode(Vector2.zero, DialogNodeType.Dialogue));
        createNodeMenu.menu.AppendAction("Choice Node", action => _graphView.CreateNewNode(Vector2.zero, DialogNodeType.Choice));
        createNodeMenu.menu.AppendAction("Logic Node", action => _graphView.CreateNewNode(Vector2.zero, DialogNodeType.Logic));
        createNodeMenu.menu.AppendAction("Event Node", action => _graphView.CreateNewNode(Vector2.zero, DialogNodeType.Event));
        createNodeMenu.menu.AppendSeparator();
        createNodeMenu.menu.AppendAction("Random Node", action => _graphView.CreateNewNode(Vector2.zero, DialogNodeType.Random));
        createNodeMenu.menu.AppendAction("Jump Node", action => _graphView.CreateNewNode(Vector2.zero, DialogNodeType.Jump));
        createNodeMenu.menu.AppendAction("Comment Node", action => _graphView.CreateNewNode(Vector2.zero, DialogNodeType.Comment));
        createNodeMenu.menu.AppendSeparator();
        createNodeMenu.menu.AppendAction("Start Node", action => _graphView.CreateNewNode(Vector2.zero, DialogNodeType.Start));
        createNodeMenu.menu.AppendAction("End Node", action => _graphView.CreateNewNode(Vector2.zero, DialogNodeType.End));
        toolbar.Add(createNodeMenu);

        var refreshButton = new Button(LoadConversation) { text = "Refresh", tooltip = "Reload the conversation from the asset" };
        toolbar.Add(refreshButton);

        var saveButton = new Button(SaveConversation) { text = "Save", tooltip = "Save all changes to the conversation asset" };
        toolbar.Add(saveButton);

        rootVisualElement.Add(toolbar);
    }

    public void LoadConversation()
    {
        if (_conversationField != null) _conversationField.value = _conversation;
        if (_conversation == null) return;

        // Clear command history when loading a different conversation
        _commandHistory?.Clear();

        _graphView.PopulateView(_conversation);
    }

    private void SaveConversation()
    {
        if (_conversation == null) return;
        _graphView.SaveToAsset(_conversation);
        AssetDatabase.SaveAssets();
    }

    private void CreateNewConversation()
    {
        var path = EditorUtility.SaveFilePanelInProject("Create New Dialog Conversation", "NewConversation", "asset", "Save Conversation");
        if (string.IsNullOrEmpty(path)) return;

        var conversation = ScriptableObject.CreateInstance<DialogConversation>();
        conversation.conversationName = System.IO.Path.GetFileNameWithoutExtension(path);

        AssetDatabase.CreateAsset(conversation, path);
        AssetDatabase.SaveAssets();

        _conversation = conversation;
        LoadConversation();
    }
}