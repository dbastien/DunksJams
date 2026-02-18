public sealed class PongFlowBootstrap : GameFlowBootstrap
{
    protected override GameDefinition BuildDefinition() => new PongGameDefinition();
}