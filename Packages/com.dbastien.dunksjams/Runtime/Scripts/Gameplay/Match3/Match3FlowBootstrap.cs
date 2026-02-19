using UnityEngine;

/// <summary>
/// Bootstrap component to initialize Match3 game flow.
/// Add this to your Match3 scene to automatically set up the game.
/// </summary>
[RequireComponent(typeof(GameFlowManager))]
public class Match3FlowBootstrap : MonoBehaviour
{
    [SerializeField] private Match3LevelData _levelData;
    [SerializeField] private bool _autoStart = true;

    private void Start()
    {
        GameFlowManager flowManager = GetComponent<GameFlowManager>();

        if (flowManager == null)
        {
            DLog.LogE("Match3FlowBootstrap: GameFlowManager not found!");
            return;
        }

        // Create game definition
        var gameDefinition = new Match3GameDefinition();

        // Set level if provided
        if (_levelData != null)
            gameDefinition.CurrentLevel = _levelData;

        // Register with flow manager
        flowManager.SetDefinition(gameDefinition, _autoStart);

        DLog.Log("Match3FlowBootstrap: Game initialized");
    }
}
