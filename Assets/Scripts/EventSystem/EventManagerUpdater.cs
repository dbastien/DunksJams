/// <summary>
/// Drives the EventManager queue processing every frame.
/// Add this to a persistent GameObject or let it auto-create via Instance access.
/// </summary>
public class EventManagerUpdater : SingletonBehavior<EventManagerUpdater>
{
    protected override void InitInternal() => DontDestroyOnLoad(gameObject);
    void Update() => EventManager.Update();
}
