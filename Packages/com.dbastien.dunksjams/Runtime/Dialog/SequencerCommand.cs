using UnityEngine;

public abstract class SequencerCommand : MonoBehaviour
{
    protected string[] Parameters { get; private set; }
    protected bool IsFinished { get; private set; }

    public void Initialize(string[] parameters) { Parameters = parameters; }

    protected virtual void Start()
    {
        // Override in sub-classes
    }

    protected void Stop()
    {
        IsFinished = true;
        Destroy(this);
    }

    public bool IsRunning() => !IsFinished;

    protected string GetParameter(int index, string fallback = "")
    {
        if (Parameters == null || index >= Parameters.Length) return fallback;
        return Parameters[index];
    }

    protected float GetParameterFloat(int index, float fallback = 0f)
    {
        string val = GetParameter(index);
        if (float.TryParse(val, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out float result))
            return result;
        return fallback;
    }

    protected bool GetParameterBool(int index, bool fallback = false)
    {
        string val = GetParameter(index);
        if (string.IsNullOrEmpty(val)) return fallback;
        return val.ToLower() == "true" || val == "1";
    }
}