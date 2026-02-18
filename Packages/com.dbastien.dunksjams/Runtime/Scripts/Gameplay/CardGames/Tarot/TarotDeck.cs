using System.Collections.Generic;

public static class TarotDeck
{
    public static Deck<TarotCard> CreateDeck()
    {
        var cards = new List<TarotCard>(78);

        foreach (TarotCard.MajorArcana major in EnumCache<TarotCard.MajorArcana>.Values)
            cards.Add(new TarotCard(major));

        foreach (TarotCard.Suit suit in EnumCache<TarotCard.Suit>.Values)
        foreach (TarotCard.Rank rank in EnumCache<TarotCard.Rank>.Values)
            cards.Add(new TarotCard(suit, rank));

        return new Deck<TarotCard>(cards);
    }
}