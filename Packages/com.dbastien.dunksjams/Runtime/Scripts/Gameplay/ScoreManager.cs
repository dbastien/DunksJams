using System;
using UnityEngine;

//todo: largely untested
//[AddComponentMenu("‽/ScoreManager")]
public class ScoreManager : MonoBehaviour
{
    public int initialLives = 3;

    [ToggleHeader("useTimer", "Timer")] public bool useTimer;
    [ShowIf("useTimer")] public float initialTime = 300f;

    [ToggleHeader("useHighScore", "High Score")]
    public bool useHighScore = true;

    [ToggleHeader("useCombos", "Combos")] public bool useCombos;
    [ShowIf("useCombos")] public int comboThreshold = 5;
    [ShowIf("useCombos")] public float comboResetTime = 2f;
    [ShowIf("useCombos")] public bool hasMultiplierSystem;
    [ShowIf("useCombos")] public int maxMultiplier = 5;

    private int _score;
    private int _highScore;
    private int _lives;
    private float _timeRemaining;
    private int _multiplier = 1;
    private int _comboStreak;
    private float _comboTimer;
    private bool _gameOver;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnLivesChanged;
    public event Action<float> OnTimeChanged;
    public event Action OnGameOver;

    public int Score => _score;
    public int Lives => _lives;
    public float TimeRemaining => _timeRemaining;
    public int HighScore => _highScore;
    public bool IsGameOver => _gameOver;

    private void Start() => ResetGame();

    private void Update()
    {
        if (_gameOver) return;

        if (useTimer)
        {
            _timeRemaining -= Time.deltaTime;
            OnTimeChanged?.Invoke(_timeRemaining);
            if (_timeRemaining <= 0) EndGame();
        }

        if (useCombos && _comboStreak > 0)
        {
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0) ResetCombo();
        }
    }

    public void ResetGame()
    {
        _score = 0;
        _lives = initialLives;
        _timeRemaining = initialTime;
        _comboStreak = 0;
        _multiplier = 1;
        _comboTimer = 0;
        _gameOver = false;

        OnScoreChanged?.Invoke(_score);
        OnLivesChanged?.Invoke(_lives);
        OnTimeChanged?.Invoke(_timeRemaining);
    }

    public void AddScore(int points, bool isCombo = false)
    {
        if (_gameOver) return;

        if (useCombos)
        {
            if (hasMultiplierSystem) _multiplier = Mathf.Min(1 + _comboStreak / comboThreshold, maxMultiplier);

            if (isCombo)
            {
                ++_comboStreak;
                _comboTimer = comboResetTime;
            }
        }
        else { ResetCombo(); }

        _score += points * _multiplier;
        OnScoreChanged?.Invoke(_score);
        if (useHighScore)
            if (_score > _highScore)
                _highScore = _score;
    }

    public void DeductLife()
    {
        if (_gameOver) return;

        --_lives;
        OnLivesChanged?.Invoke(_lives);
        if (_lives <= 0) EndGame();
    }

    public void SetTimedGame(float time) => useTimer = (_timeRemaining = time) > 0;
    public void SetMultiplier(int multiplier) => _multiplier = multiplier;

    public void SetCombo(bool enableCombo)
    {
        useCombos = enableCombo;
        if (!enableCombo) ResetCombo();
    }

    public void RestartGame()
    {
        ResetGame();
        _gameOver = false;
    }

    public void ResetCombo()
    {
        _comboStreak = 0;
        _multiplier = 1;
    }

    public void EndGame()
    {
        _gameOver = true;
        OnGameOver?.Invoke();
    }
}