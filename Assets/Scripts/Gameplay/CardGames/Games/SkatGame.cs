public class SkatGame : CardGameBase<StandardCard>
{
    public SkatGame(int playerCount = 3, int maxRounds = 50) : base(playerCount, maxRounds) { }

    protected override Deck<StandardCard> CreateDeck() => SkatDeck.CreateDeck();
}
