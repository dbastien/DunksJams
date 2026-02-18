using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class DevConsole : SingletonEagerBehaviour<DevConsole>
{
    private const int _fontSize = 12;
    private readonly RingBuffer<string> _commandHistory = new(16);
    private readonly Dictionary<string, Action<string[]>> _commands = new();
    private readonly List<string> _history = new();
    private GUIStyle _consoleStyle, _inputStyle, _boxStyle;
    private Font _font;
    private int _historyIndex = -1;
    private string _input = "";
    private bool _isOpen, _shouldFocus;
    private Vector2 _scrollPos;

    protected override bool PersistAcrossScenes => true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
            Toggle();
    }

    private void OnGUI()
    {
        if (!_isOpen) return;
        SetupStylesIfNeeded();
        HandleInput();
        DrawConsole();
        DrawInputField();
    }

    protected override void InitInternal()
    {
        DiscoverCommands();
        RegisterBuiltInCommands();
        DLog.Log("DevConsole initialized. Press `~` to toggle.");
        DLog.Log($"Commands: {string.Join(", ", _commands.Keys)}");
    }

    private void Toggle()
    {
        _isOpen = !_isOpen;
        _input = _isOpen ? "" : _input;
        _shouldFocus = _isOpen;
    }

    private void SetupStylesIfNeeded()
    {
        _font ??= Resources.Load<Font>("FiraCode-Regular");

        _consoleStyle ??= GUI.skin.label.CreateStyle(_font);
        _inputStyle ??= GUI.skin.textField.CreateStyle(_font);
        _boxStyle ??= GUI.skin.box.CreateStyle(4, 5).
            WithBackground(TextureUtils.GetSolidColor(new Color(0.1f, 0.1f, 0.1f, 0.9f)));
    }

    private void HandleInput()
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

    private void NavigateCommandHistory(int direction)
    {
        if (_commandHistory.Count == 0) return;

        if (_historyIndex == -1 && direction == -1)
            _historyIndex = _commandHistory.Count - 1;
        else
            _historyIndex = Mathf.Clamp(_historyIndex + direction, -1, _commandHistory.Count - 1);

        // Only retrieve command if _historyIndex is valid
        string command = _historyIndex == -1 ? "" : _commandHistory.GetRecent(_historyIndex);
        DLog.Log($"NavigateCommandHistory: direction={direction}, _historyIndex={_historyIndex}, command='{command}'");

        _input = command; // Assign retrieved command to _input
        _shouldFocus = true;
    }

    private void AutocompleteCommand()
    {
        if (string.IsNullOrWhiteSpace(_input)) return;
        List<string> matches = _commands.Keys.Where(cmd => cmd.StartsWith(_input)).ToList();
        if (matches.Count == 1) _input = matches[0];
        else if (matches.Count > 1) Log($"Matching commands: {string.Join(", ", matches)}");
    }

    private void DrawConsole()
    {
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height / 3f), _boxStyle);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);
        foreach (string entry in _history) GUILayout.Label(entry ?? "", _consoleStyle);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawInputField()
    {
        GUILayout.BeginArea(new Rect(10, Screen.height / 3f + 20, Screen.width - 20, 30));
        GUI.SetNextControlName("ConsoleInput");
        _input = GUILayout.TextField(_input ?? "", _inputStyle);
        if (_shouldFocus && Event.current.type == EventType.Repaint)
        {
            GUI.FocusControl("ConsoleInput");
            _shouldFocus = false;
        }

        GUILayout.EndArea();
    }

    private void ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        _history.Add($"> {input}");
        _commandHistory.Add(input);
        _historyIndex = -1;

        string[] parts = input.Split(' ');

        if (_commands.TryGetValue(parts[0], out Action<string[]> action))
            TryInvoke(action, parts[1..]);
        else
            LogError($"Command '{parts[0]}' not recognized.");
    }

    private void RegisterCommand(string name, Action<string[]> action, string desc = "")
    {
        if (_commands.ContainsKey(name))
            DLog.LogW($"Command '{name}' already registered, overwriting.");
        _commands[name] = action ?? throw new ArgumentNullException(nameof(action));
    }

    private void RegisterBuiltInCommands()
    {
        RegisterCommand("help", _ => ShowHelp(), "Lists all available commands.");
        RegisterCommand("clear", _ => _history.Clear(), "Clears the console output.");
        RegisterCommand("randomcolor", _ => _history.Add(Rand.Color().ToString()), "Generates a random color.");
    }

    private void DiscoverCommands()
    {
        IEnumerable<MethodInfo> methods = GetType().GetMethodsWithAttribute<ConsoleCommandAttribute>();
        foreach (MethodInfo m in methods)
            RegisterCommand(m.Name.ToLower(), args => InvokeMethod(m, args), m.GetMethodSignature());
    }

    private static void TryInvoke(Action<string[]> action, string[] args)
    {
        try { action(args); }
        catch (Exception e) { DLog.LogE($"Error invoking command: {e.Message}"); }
    }

    private void InvokeMethod(MethodInfo mi, string[] args)
    {
        object[] parsedArgs = mi.GetParameters().
            Select((p, i) => ConvertArg(args.ElementAtOrDefault(i), p.ParameterType)).
            ToArray();
        ReflectionUtils.InvokeMethod(this, mi, parsedArgs);
    }

    private static object ConvertArg(string arg, Type t) =>
        t == typeof(string) ? arg : Convert.ChangeType(arg, t);

    private void ShowHelp() => Log($"Available Commands:\n- {string.Join("\n- ", _commands.Keys)}");
    private void Log(string message) => _history.Add(message);
    private void LogError(string msg) => _history.Add($"<color=red>Error:</color> {msg}");
}