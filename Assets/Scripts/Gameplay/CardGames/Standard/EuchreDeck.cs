public static class EuchreDeck
{
    public static Deck<StandardCard> CreateDeck() =>
        StandardDeck.CreateDeck(card => card.CardRank >= StandardCard.Rank.Nine);
}
