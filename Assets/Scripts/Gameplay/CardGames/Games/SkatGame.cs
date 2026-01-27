public class SkatGame : CardGameBase<StandardCard>
{
    public SkatGame(string variantName = "Standard", int playerCount = 3, int maxRounds = 50, ICardGameIO io = null)
        : base(playerCount, maxRounds, io) => VariantName = variantName;

    protected override Deck<StandardCard> CreateDeck() => SkatDeck.CreateDeck();
}
