public class EuchreGame : CardGameBase<StandardCard>
{
    public EuchreGame(int playerCount = 4, int maxRounds = 50) : base(playerCount, maxRounds) { }

    protected override Deck<StandardCard> CreateDeck() => EuchreDeck.CreateDeck();
}
