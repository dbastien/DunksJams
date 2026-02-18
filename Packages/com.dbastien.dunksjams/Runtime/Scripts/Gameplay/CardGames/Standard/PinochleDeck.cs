using System.Collections.Generic;

public static class PinochleDeck
{
    public static Deck<StandardCard> CreateDeck()
    {
        StandardCard.Suit[] suits = EnumCache<StandardCard.Suit>.Values;
        StandardCard.Rank[] ranks =
        {
            StandardCard.Rank.Nine,
            StandardCard.Rank.Ten,
            StandardCard.Rank.Jack,
            StandardCard.Rank.Queen,
            StandardCard.Rank.King,
            StandardCard.Rank.Ace
        };

        var cards = new List<StandardCard>(48);
        foreach (StandardCard.Suit suit in suits)
        foreach (StandardCard.Rank rank in ranks)
        {
            cards.Add(new StandardCard(suit, rank));
            cards.Add(new StandardCard(suit, rank));
        }

        return new Deck<StandardCard>(cards);
    }
}