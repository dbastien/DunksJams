public sealed class PokerGameSettings
{
    public enum PokerVariant
    {
        FiveCardDraw
    }

    public PokerVariant Variant { get; set; } = PokerVariant.FiveCardDraw;
    public int MaxDiscardCount { get; set; } = 3;
}
