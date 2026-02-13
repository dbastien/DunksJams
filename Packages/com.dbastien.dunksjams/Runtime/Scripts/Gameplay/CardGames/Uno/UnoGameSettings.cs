public sealed class UnoGameSettings
{
    public enum UnoMode
    {
        Standard,
        StackingDraws
    }

    public UnoMode Mode { get; set; } = UnoMode.Standard;
    public bool PromptForMode { get; set; } = true;
    public bool AllowDrawWhenPlayable { get; set; }
    public int StartingHandSize { get; set; } = 7;

    public bool AllowStackingDraws => Mode == UnoMode.StackingDraws;
}