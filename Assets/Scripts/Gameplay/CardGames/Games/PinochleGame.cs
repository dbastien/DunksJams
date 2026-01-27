public class PinochleGame : CardGameBase<StandardCard>
{
    public PinochleGame(int playerCount = 4, int maxRounds = 50) : base(playerCount, maxRounds) { }

    protected override Deck<StandardCard> CreateDeck() => PinochleDeck.CreateDeck();
}
