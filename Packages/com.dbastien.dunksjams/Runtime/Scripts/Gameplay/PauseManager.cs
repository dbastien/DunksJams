using UnityEngine;

/// <summary>
/// Simple pause manager that supports nested pauses via push/pop semantics.
/// Call <see cref="PushPause"/> to pause and <see cref="PopPause"/> to restore.
/// This avoids conflicts when multiple systems request pause.
/// </summary>
public static class PauseManager
{
    static int _pauseCount = 0;
    static float _previousTimeScale = 1f;

    public static bool IsPaused => _pauseCount > 0;

    public static void PushPause()
    {
        if (_pauseCount == 0)
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        _pauseCount++;
    }

    public static void PopPause()
    {
        if (_pauseCount <= 0) return;
        _pauseCount--;
        if (_pauseCount == 0)
        {
            Time.timeScale = _previousTimeScale;
        }
    }

    public static void ForceResume()
    {
        _pauseCount = 0;
        Time.timeScale = _previousTimeScale;
    }
}