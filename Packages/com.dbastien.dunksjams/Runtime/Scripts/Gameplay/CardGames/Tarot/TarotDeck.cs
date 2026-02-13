using System.Collections.Generic;

public static class TarotDeck
{
    public static Deck<TarotCard> CreateDeck()
    {
        var cards = new List<TarotCard>(78);

        foreach (var major in EnumCache<TarotCard.MajorArcana>.Values)
            cards.Add(new TarotCard(major));

        foreach (var suit in EnumCache<TarotCard.Suit>.Values)
        {
            foreach (var rank in EnumCache<TarotCard.Rank>.Values)
                cards.Add(new TarotCard(suit, rank));
        }

        return new Deck<TarotCard>(cards);
    }
}