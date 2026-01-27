public class UnoGame : CardGameBase<UnoCard>
{
    public UnoGame(int playerCount = 2, int maxRounds = 200) : base(playerCount, maxRounds) { }

    protected override Deck<UnoCard> CreateDeck() => UnoDeck.CreateDeck();
}
