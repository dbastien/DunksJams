public class PinochleGame : CardGameBase<StandardCard>
{
    public PinochleGame(string variantName = "Standard", int playerCount = 4, int maxRounds = 50, ICardGameIO io = null)
        : base(playerCount, maxRounds, io) => VariantName = variantName;

    protected override Deck<StandardCard> CreateDeck() => PinochleDeck.CreateDeck();
}
