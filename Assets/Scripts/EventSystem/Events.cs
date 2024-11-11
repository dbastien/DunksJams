using UnityEngine;

public abstract class GameEvent { }

//examples
public class PlayerDeathEvent : GameEvent
{
    public Vector3 Position { get; }
    public PlayerDeathEvent(Vector3 position) => Position = position;
}
