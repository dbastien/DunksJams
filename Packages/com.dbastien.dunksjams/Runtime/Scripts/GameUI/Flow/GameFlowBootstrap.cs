using UnityEngine;

[DisallowMultipleComponent]
public abstract class GameFlowBootstrap : MonoBehaviour
{
    [SerializeField] bool _showStartScreen = true;

    protected abstract GameDefinition BuildDefinition();

    protected virtual void Awake() => Initialize();

    public void Initialize()
    {
        var definition = BuildDefinition();
        if (definition == null)
        {
            DLog.LogE($"{GetType().Name} BuildDefinition returned null.");
            return;
        }

        var flow = GameFlowManager.Instance;
        if (flow == null)
        {
            DLog.LogE("GameFlowManager missing.");
            return;
        }

        flow.SetDefinition(definition, _showStartScreen);
    }
}
