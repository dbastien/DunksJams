using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3Manager : MonoBehaviour
{
    public enum GameState
    {
        Initializing,
        WaitingForInput,
        ProcessingMatch,
        ApplyingGravity,
        CheckingForMatches,
        GameOver
    }

    [Header("References")]
    [SerializeField] private Match3InputHandler _inputHandler;
    [SerializeField] private Match3TilePool _tilePool;
    [SerializeField] private Transform _gridContainer;

    [Header("Grid Settings")]
    [SerializeField] private float _tileSize = 1f;
    [SerializeField] private Vector3 _gridOffset = Vector3.zero;

    private Match3Grid _grid;
    private Match3MatchDetector _matchDetector;
    private Match3GravitySimulator _gravitySimulator;
    private Match3LevelData _currentLevel;

    private GameState _state = GameState.Initializing;
    private int _currentMoves;
    private int _currentScore;
    private int _comboCount;

    private ObjectiveManager _objectiveManager;
    private GameFlowManager _gameFlowManager;

    public GameState State => _state;
    public Match3Grid Grid => _grid;
    public int RemainingMoves => _currentLevel != null ? _currentLevel.MaxMoves - _currentMoves : 0;
    public int CurrentScore => _currentScore;

    private void Awake()
    {
        if (_inputHandler == null)
            _inputHandler = GetComponent<Match3InputHandler>();

        if (_tilePool == null)
            _tilePool = GetComponent<Match3TilePool>();

        _gameFlowManager = GameFlowManager.Instance;
        _objectiveManager = new ObjectiveManager();
    }

    public void InitializeLevel(Match3LevelData levelData)
    {
        if (levelData == null)
        {
            DLog.LogE("Match3Manager.InitializeLevel: levelData is null");
            return;
        }

        _currentLevel = levelData;
        _state = GameState.Initializing;

        StartCoroutine(InitializeLevelCoroutine());
    }

    private IEnumerator InitializeLevelCoroutine()
    {
        // Reset game state
        _currentMoves = 0;
        _currentScore = 0;
        _comboCount = 0;

        // Create grid
        _grid = new Match3Grid(
            _currentLevel.Width,
            _currentLevel.Height,
            _tileSize,
            _gridOffset
        );

        // Initialize systems
        _matchDetector = new Match3MatchDetector(_grid, _currentLevel.MinMatchSize);
        _gravitySimulator = new Match3GravitySimulator(_grid, _tilePool, _currentLevel);

        if (_inputHandler != null)
        {
            _inputHandler.Initialize(_grid, _currentLevel.MinMatchSize);
            _inputHandler.OnMatchConfirmed += HandleMatchConfirmed;
        }

        // Initialize objectives
        InitializeObjectives();

        // Spawn initial tiles
        yield return SpawnInitialTiles();

        // Check for and remove any initial matches
        yield return ClearInitialMatches();

        // Ready to play
        _state = GameState.WaitingForInput;
        if (_inputHandler != null)
            _inputHandler.UnlockInput();

        DLog.Log($"Match3Manager: Level initialized. Grid: {_grid.Width}x{_grid.Height}, Moves: {_currentLevel.MaxMoves}");
    }

    private IEnumerator SpawnInitialTiles()
    {
        for (var y = 0; y < _grid.Height; y++)
        {
            for (var x = 0; x < _grid.Width; x++)
            {
                var pos = new Vector2Int(x, y);

                // Check if level has predefined tiles
                Match3LevelData.TileDefinition tileDef = _currentLevel.GetTileDefinition(x, y);

                Match3Tile.TileType type = tileDef?.Type ?? Match3Tile.TileType.Normal;
                Match3Tile.ColorType color = tileDef?.Color ?? _currentLevel.GetRandomColor();

                // Get tile from pool
                Match3Tile tile = _tilePool.GetTile(type, color);
                if (tile != null)
                {
                    tile.gameObject.SetActive(true);
                    tile.transform.position = _grid.GridToWorld(pos);
                    tile.Initialize(type, color, pos);
                    _grid.SetTile(pos, tile);
                }
            }
        }

        yield return null;
    }

    private IEnumerator ClearInitialMatches()
    {
        // Keep clearing matches until there are none
        int maxIterations = 10;
        var iteration = 0;

        while (iteration < maxIterations)
        {
            List<Match3MatchDetector.Match> matches = _matchDetector.FindAllMatches();

            if (matches.Count == 0)
                break;

            // Remove matched tiles and replace with new random ones
            foreach (Match3MatchDetector.Match match in matches)
            {
                foreach (Vector2Int pos in match.Positions)
                {
                    Match3Tile tile = _grid.GetTile(pos);
                    if (tile != null)
                    {
                        _tilePool.ReturnTile(tile);
                        _grid.RemoveTile(pos);

                        // Spawn a new random tile
                        Match3Tile.ColorType newColor = _currentLevel.GetRandomColor();
                        Match3Tile newTile = _tilePool.GetTile(Match3Tile.TileType.Normal, newColor);

                        if (newTile != null)
                        {
                            newTile.gameObject.SetActive(true);
                            newTile.transform.position = _grid.GridToWorld(pos);
                            newTile.Initialize(Match3Tile.TileType.Normal, newColor, pos);
                            _grid.SetTile(pos, newTile);
                        }
                    }
                }
            }

            iteration++;
            yield return null;
        }
    }

    private void HandleMatchConfirmed(List<Vector2Int> positions)
    {
        if (_state != GameState.WaitingForInput || positions == null || positions.Count == 0)
            return;

        StartCoroutine(ProcessMatchSequence(positions));
    }

    private IEnumerator ProcessMatchSequence(List<Vector2Int> matchPositions)
    {
        _state = GameState.ProcessingMatch;

        if (_inputHandler != null)
            _inputHandler.LockInput();

        // Increment move count
        _currentMoves++;
        UpdateGameFlowScore("Moves", RemainingMoves);

        // Remove matched tiles
        yield return _gravitySimulator.RemoveTiles(matchPositions, count =>
        {
            AddScore(count * _currentLevel.PointsPerTile);
        });

        // Start combo chain
        _comboCount = 1;

        // Keep processing until no more matches
        yield return ProcessCascadingMatches();

        // Check win/lose conditions
        CheckGameEndConditions();

        // Return to waiting for input
        if (_state != GameState.GameOver)
        {
            _state = GameState.WaitingForInput;

            if (_inputHandler != null)
                _inputHandler.UnlockInput();
        }
    }

    private IEnumerator ProcessCascadingMatches()
    {
        bool foundMatches;

        do
        {
            _state = GameState.ApplyingGravity;

            // Apply gravity and refill
            yield return _gravitySimulator.ApplyGravityAndRefill();

            _state = GameState.CheckingForMatches;

            // Check for new matches
            List<Match3MatchDetector.Match> matches = _matchDetector.FindAllMatches();
            foundMatches = matches.Count > 0;

            if (foundMatches)
            {
                _comboCount++;

                // Collect all positions to remove
                var positionsToRemove = new List<Vector2Int>();
                foreach (Match3MatchDetector.Match match in matches)
                    positionsToRemove.AddRange(match.Positions);

                // Remove tiles with combo bonus
                yield return _gravitySimulator.RemoveTiles(positionsToRemove, count =>
                {
                    int score = count * _currentLevel.PointsPerTile;
                    int comboBonus = (_comboCount - 1) * _currentLevel.ComboMultiplierBonus;
                    AddScore(score + comboBonus);
                });
            }

        } while (foundMatches);
    }

    private void AddScore(int points)
    {
        _currentScore += points;
        UpdateGameFlowScore("Score", _currentScore);
    }

    private void UpdateGameFlowScore(string scoreId, int value)
    {
        if (_gameFlowManager != null)
            _gameFlowManager.SetScore(scoreId, value);
    }

    private void InitializeObjectives()
    {
        _objectiveManager.ResetAllObjectives();

        if (_currentLevel == null || _currentLevel.WinConditions == null) return;

        foreach (Match3LevelData.WinCondition condition in _currentLevel.WinConditions)
        {
            var objective = new ObjectiveManager.Objective(
                condition.Type.ToString(),
                condition.GetDescription()
            );

            _objectiveManager.AddObjective(objective);
        }

        _objectiveManager.OnAllObjectivesCompleted += HandleLevelCompleted;
    }

    private void CheckGameEndConditions()
    {
        // Check lose condition (out of moves)
        if (RemainingMoves <= 0)
        {
            bool allObjectivesComplete = true;

            foreach (var objective in _objectiveManager.Objectives)
            {
                if (objective.Status != ObjectiveManager.ObjectiveStatus.Completed)
                {
                    allObjectivesComplete = false;
                    break;
                }
            }

            if (!allObjectivesComplete)
            {
                HandleLevelFailed();
                return;
            }
        }

        // Check win conditions
        CheckWinConditions();
    }

    private void CheckWinConditions()
    {
        if (_currentLevel == null) return;

        foreach (Match3LevelData.WinCondition condition in _currentLevel.WinConditions)
        {
            bool conditionMet = condition.Type switch
            {
                Match3LevelData.WinCondition.ConditionType.Score => _currentScore >= condition.TargetValue,
                _ => false
            };

            if (conditionMet)
            {
                // Find and complete the corresponding objective
                foreach (var objective in _objectiveManager.Objectives)
                {
                    if (objective.Title == condition.Type.ToString())
                    {
                        objective.Complete();
                        break;
                    }
                }
            }
        }
    }

    private void HandleLevelCompleted()
    {
        DLog.Log("Match3Manager: Level completed!");
        _state = GameState.GameOver;

        if (_gameFlowManager != null)
            _gameFlowManager.EndGame();
    }

    private void HandleLevelFailed()
    {
        DLog.Log("Match3Manager: Level failed!");
        _state = GameState.GameOver;

        if (_gameFlowManager != null)
            _gameFlowManager.EndGame();
    }

    public void CleanupLevel()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnMatchConfirmed -= HandleMatchConfirmed;
            _inputHandler.ClearSelection();
        }

        if (_tilePool != null)
            _tilePool.ReturnAllActiveTiles();

        _grid = null;
        _matchDetector = null;
        _gravitySimulator = null;
    }

    private void OnDestroy()
    {
        CleanupLevel();

        if (_objectiveManager != null)
            _objectiveManager.OnAllObjectivesCompleted -= HandleLevelCompleted;
    }
}
