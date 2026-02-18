using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[SingletonAutoCreate]
[DisallowMultipleComponent]
public class GameFlowManager : SingletonEagerBehaviour<GameFlowManager>
{
    [SerializeField] private bool _persistAcrossScenes;

    public GameDefinition ActiveDefinition { get; private set; }
    public GameFlowState State { get; private set; }

    private readonly Dictionary<string, int> _scores = new(StringComparer.Ordinal);
    private bool _screensRegistered;

    protected override bool PersistAcrossScenes => _persistAcrossScenes;
    protected override void InitInternal() => State = GameFlowState.None;

    public void SetDefinition(GameDefinition definition, bool showStartScreen = true)
    {
        if (definition == null)
        {
            DLog.LogE("GameFlowManager.SetDefinition failed: definition is null.");
            return;
        }

        ActiveDefinition = definition;
        InitializeScores();
        EnsureScreens();

        if (showStartScreen)
            ShowStartMenu();
    }

    public void ShowStartMenu()
    {
        State = GameFlowState.StartMenu;
        UIManager ui = UIManager.Instance;
        if (ui == null) return;
        ui.HideAllScreens();
        ui.ShowScreen<StartScreen>();
    }

    public void StartGame()
    {
        if (ActiveDefinition == null)
        {
            DLog.LogW("StartGame called with no active definition.");
            return;
        }

        InitializeScores();
        State = GameFlowState.Playing;

        UIManager ui = UIManager.Instance;
        if (ui != null)
        {
            ui.HideAllScreens();
            ScoreboardSpec scoreboard = ActiveDefinition.Scoreboard;
            if (scoreboard != null && scoreboard.Count > 0)
                ui.ShowScreen<HUDScreen>();
        }

        ActiveDefinition.OnGameStart(this);
        EventManager.QueueEvent<GameStartedEvent>(e => e.Title = ActiveDefinition.Title, true);
    }

    public void EndGame()
    {
        if (ActiveDefinition == null) return;

        State = GameFlowState.Ended;
        ActiveDefinition.OnGameEnd(this);

        UIManager ui = UIManager.Instance;
        if (ui != null)
        {
            ui.HideAllScreens();
            ui.ShowScreen<GameOverScreen>();
        }

        EventManager.QueueEvent<GameEndedEvent>(e => e.Title = ActiveDefinition.Title, true);
    }

    public void PauseGame()
    {
        if (State != GameFlowState.Playing) return;
        State = GameFlowState.Paused;
        UIManager.Instance?.ShowScreen<PauseScreen>();
        EventManager.QueueEvent<GamePausedEvent>(null, true);
    }

    public void ResumeGame()
    {
        if (State != GameFlowState.Paused) return;
        State = GameFlowState.Playing;
        UIManager.Instance?.HideCurrentScreen();
        EventManager.QueueEvent<GameResumedEvent>(null, true);
    }

    public void RestartGame()
    {
        ActiveDefinition?.OnGameRestart(this);
        StartGame();
    }

    public void ExecuteAction(ScreenAction action)
    {
        switch (action)
        {
            case ScreenAction.StartGame:
                StartGame();
                break;
            case ScreenAction.OpenConfig:
                UIManager.Instance?.ShowScreen<ConfigScreen>();
                break;
            case ScreenAction.CloseScreen:
                UIManager.Instance?.HideCurrentScreen();
                break;
            case ScreenAction.ResumeGame:
                ResumeGame();
                break;
            case ScreenAction.MainMenu:
                ShowStartMenu();
                break;
            case ScreenAction.Restart:
                RestartGame();
                break;
            case ScreenAction.Quit:
                QuitGame();
                break;
        }
    }

    public int GetScore(string id) => _scores.TryGetValue(id, out int value) ? value : 0;
    public void AddScore(string id, int delta) => SetScore(id, GetScore(id) + delta);

    public void SetScore(string id, int value)
    {
        if (!_scores.ContainsKey(id))
        {
            DLog.LogW($"Unknown score id '{id}'");
            return;
        }

        int prev = _scores[id];
        if (prev == value) return;

        _scores[id] = value;
        EventManager.QueueEvent<ScoreChangedEvent>(e =>
        {
            e.Id = id;
            e.Value = value;
            e.Delta = value - prev;
        }, true);
    }

    public string GetScoreLabel(string id) =>
        ActiveDefinition != null && ActiveDefinition.Scoreboard.TryGetField(id, out ScoreFieldDef field)
            ? field.Label
            : id;

    public string FormatScores()
    {
        ScoreboardSpec scoreboard = ActiveDefinition?.Scoreboard;
        if (scoreboard == null || scoreboard.Count == 0) return "Game Over";

        var sb = new StringBuilder();
        for (var i = 0; i < scoreboard.Fields.Count; ++i)
        {
            ScoreFieldDef field = scoreboard.Fields[i];
            if (i > 0) sb.Append('\n');
            sb.Append(field.Label).Append(": ").Append(GetScore(field.Id));
        }

        return sb.ToString();
    }

    private void InitializeScores()
    {
        _scores.Clear();
        ScoreboardSpec scoreboard = ActiveDefinition?.Scoreboard;
        if (scoreboard == null) return;

        for (var i = 0; i < scoreboard.Fields.Count; ++i)
        {
            ScoreFieldDef field = scoreboard.Fields[i];
            _scores[field.Id] = field.InitialValue;
        }
    }

    private void EnsureScreens()
    {
        if (_screensRegistered) return;

        UIManager ui = UIManager.Instance;
        if (ui == null)
        {
            DLog.LogE("GameFlowManager requires a UIManager in the scene.");
            return;
        }

        ui.RegisterScreen<StartScreen>();
        ui.RegisterScreen<ConfigScreen>();
        ui.RegisterScreen<PauseScreen>();
        ui.RegisterScreen<GameOverScreen>();
        ui.RegisterScreen<HUDScreen>();

        _screensRegistered = true;
    }

    private void QuitGame()
    {
        DLog.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}