/// <summary>Drives the EventManager queue processing every frame. Add this to a persistent GameObject or let it auto-create via Instance access.</summary>
[SingletonAutoCreate]
public class EventManagerUpdater : SingletonEagerBehaviour<EventManagerUpdater>
{
    protected override bool PersistAcrossScenes => true;

    protected override void InitInternal() { }

    private void Update() => EventManager.Update();
}