using System;
using System.Collections.Generic;

public abstract class GameDefinition
{
    public abstract string Title { get; }

    public virtual ScreenSpec StartScreen => ScreenSpec.DefaultStart(Title);
    public virtual ScreenSpec PauseScreen => ScreenSpec.DefaultPause();
    public virtual ScreenSpec EndScreen => ScreenSpec.DefaultEnd();
    public virtual ScreenSpec ConfigScreen => ScreenSpec.DefaultConfig();

    public virtual ScoreboardSpec Scoreboard => ScoreboardSpec.Empty;
    public virtual IReadOnlyList<ConfigOptionDef> ConfigOptions => Array.Empty<ConfigOptionDef>();

    public virtual void OnGameStart(GameFlowManager flow) { }

    public virtual void OnGameEnd(GameFlowManager flow) { }

    public virtual void OnGameRestart(GameFlowManager flow) { }

    public virtual string BuildGameOverSummary(GameFlowManager flow) =>
        flow == null ? "Game Over" : flow.FormatScores();
}