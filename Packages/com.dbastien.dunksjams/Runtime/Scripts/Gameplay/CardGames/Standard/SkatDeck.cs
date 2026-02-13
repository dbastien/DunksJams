public static class SkatDeck
{
    public static Deck<StandardCard> CreateDeck() =>
        StandardDeck.CreateDeck(card => card.CardRank >= StandardCard.Rank.Seven);
}