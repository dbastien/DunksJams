using UnityEngine;

public abstract class GameEvent
{
    /// <summary>Set to true to stop event propagation to remaining listeners.</summary>
    public bool IsCancelled { get; set; }
}

//examples
public class PlayerDeathEvent : GameEvent
{
    public Vector3 Position { get; }
    public PlayerDeathEvent(Vector3 position) => Position = position;
}