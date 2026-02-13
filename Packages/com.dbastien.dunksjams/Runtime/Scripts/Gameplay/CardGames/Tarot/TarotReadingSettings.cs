public sealed class TarotReadingSettings
{
    public enum TarotSpread
    {
        SingleCard,
        ThreeCard
    }

    public TarotSpread Spread { get; set; } = TarotSpread.ThreeCard;
    public bool PromptForSpread { get; set; } = true;
}