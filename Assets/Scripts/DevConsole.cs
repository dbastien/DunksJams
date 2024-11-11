using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.TerrainTools;

public class DevConsole : SingletonBehavior<DevConsole>
{
    readonly Dictionary<string, Action<string[]>> _commands = new();
    readonly List<string> _history = new();
    readonly RingBuffer<string> _commandHistory = new(16);
    int _historyIndex = -1;
    string _input = "";
    bool _isOpen, _shouldFocus;
    Vector2 _scrollPos;
    GUIStyle _consoleStyle, _inputStyle, _boxStyle;
    Font _font;
    const int _fontSize = 12;

    protected override void InitInternal()
    {
        DontDestroyOnLoad(gameObject);
        DiscoverCommands();
        RegisterBuiltInCommands();
        DLog.Log("DevConsole initialized. Press `~` to toggle.");
        DLog.Log($"Commands: {string.Join(", ", _commands.Keys)}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
            Toggle();
    }

    void Toggle()
    {
        _isOpen = !_isOpen;
        _input = _isOpen ? "" : _input;
        _shouldFocus = _isOpen;
    }
    
    void OnGUI()
    {
        if (!_isOpen) return;
        SetupStylesIfNeeded();
        HandleInput();
        DrawConsole();        
        DrawInputField();
    }
    
    void SetupStylesIfNeeded()
    {
        _font ??= Resources.Load<Font>("FiraCode-Regular");
        
        _consoleStyle ??= GUI.skin.label.CreateStyle(_font, _fontSize);
        _inputStyle ??= GUI.skin.textField.CreateStyle(_font, _fontSize);
        _boxStyle ??= GUI.skin.box.CreateStyle(4, 5).WithBackground(TextureUtils.GetSolidColor(new(0.1f, 0.1f, 0.1f, 0.9f)));
    }
    
    void HandleInput()
    {
        if (Event.current.rawType != EventType.KeyDown) return;

        switch (Event.current.keyCode)
        {
            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                ExecuteCommand(_input.Trim());
                _input = "";
                _historyIndex = -1;
                Event.current.Use();
                break;
            case KeyCode.UpArrow:
                NavigateCommandHistory(-1);
                Event.current.Use();
                break;
            case KeyCode.DownArrow:
                NavigateCommandHistory(1);
                Event.current.Use();
                break;
            case KeyCode.Tab:
                AutocompleteCommand();
                Event.current.Use();
                break;
        }
    }
    
    void NavigateCommandHistory(int direction)
    {
        if (_commandHistory.Count == 0) return;

        if (_historyIndex == -1 && direction == -1)
            _historyIndex = _commandHistory.Count - 1;
        else
            _historyIndex = Mathf.Clamp(_historyIndex + direction, -1, _commandHistory.Count - 1);

        // Only retrieve command if _historyIndex is valid
        string command = _historyIndex == -1 ? "" : _commandHistory.GetRecent(_historyIndex);
        Debug.Log($"NavigateCommandHistory: direction={direction}, _historyIndex={_historyIndex}, command='{command}'");

        _input = command; // Assign retrieved command to _input
        _shouldFocus = true;
    }
    
    void AutocompleteCommand()
    {
        if (string.IsNullOrWhiteSpace(_input)) return;
        var matches = _commands.Keys.Where(cmd => cmd.StartsWith(_input)).ToList();
        if (matches.Count == 1) _input = matches[0];
        else if (matches.Count > 1) Log($"Matching commands: {string.Join(", ", matches)}");
    }

    void DrawConsole()
    {
        GUILayout.BeginArea(new(10, 10, Screen.width - 20, Screen.height / 3f), _boxStyle);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);
        foreach (var entry in _history) GUILayout.Label(entry ?? "", _consoleStyle);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void DrawInputField()
    {
        GUILayout.BeginArea(new(10, Screen.height / 3f + 20, Screen.width - 20, 30));
        GUI.SetNextControlName("ConsoleInput");
        _input = GUILayout.TextField(_input ?? "", _inputStyle);
        if (_shouldFocus && Event.current.type == EventType.Repaint)
        {
            GUI.FocusControl("ConsoleInput");
            _shouldFocus = false;
        }
        GUILayout.EndArea();
    }
    
    void ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        
        _history.Add($"> {input}");
        _commandHistory.Add(input);
        _historyIndex = -1;
        
        var parts = input.Split(' ');
        
        if (_commands.TryGetValue(parts[0], out var action)) 
            TryInvoke(action, parts[1..]);
        else 
            LogError($"Command '{parts[0]}' not recognized.");
    }

    void RegisterCommand(string name, Action<string[]> action, string desc = "")
    {
        if (_commands.ContainsKey(name))
            DLog.LogW($"Command '{name}' already registered, overwriting.");
        _commands[name] = action ?? throw new ArgumentNullException(nameof(action));
    }
    
    void RegisterBuiltInCommands()
    {
        RegisterCommand("help", _ => ShowHelp(), "Lists all available commands.");
        RegisterCommand("clear", _ => _history.Clear(), "Clears the console output.");
        RegisterCommand("randomcolor", _ => _history.Add(Rand.Color().ToString()), "Generates a random color.");
    }

    void DiscoverCommands()
    {
        var methods = GetType().GetMethodsWithAttribute<ConsoleCommandAttribute>();
        foreach (var m in methods)
            RegisterCommand(m.Name.ToLower(), args => InvokeMethod(m, args), m.GetMethodSignature());
    }

    static void TryInvoke(Action<string[]> action, string[] args)
    {
        try { action(args); }
        catch (Exception e) { DLog.LogE($"Error invoking command: {e.Message}"); }
    }

    void InvokeMethod(MethodInfo mi, string[] args)
    {
        var parsedArgs = mi.GetParameters()
            .Select((p, i) => ConvertArg(args.ElementAtOrDefault(i), p.ParameterType)).ToArray();
        ReflectionUtils.InvokeMethod(this, mi, parsedArgs);
    }

    static object ConvertArg(string arg, Type t) =>
        t == typeof(string) ? arg : Convert.ChangeType(arg, t);
    
    void ShowHelp() => Log($"Available Commands:\n- {string.Join("\n- ", _commands.Keys)}");
    void Log(string message) => _history.Add(message);
    void LogError(string msg) => _history.Add($"<color=red>Error:</color> {msg}");
}