public class TarotGame : CardGameBase<TarotCard>
{
    public TarotGame(int playerCount = 2, int maxRounds = 50) : base(playerCount, maxRounds) { }

    protected override Deck<TarotCard> CreateDeck() => TarotDeck.CreateDeck();
}
