using System;
using System.Collections.Generic;
using System.Linq;

public static class StandardDeck
{
    public static Deck<StandardCard> CreateDeck(Func<StandardCard, bool> predicate = null)
    {
        var suits = EnumCache<StandardCard.Suit>.Values;
        var ranks = EnumCache<StandardCard.Rank>.Values;

        var cards = from s in suits
            from r in ranks
            select new StandardCard(s, r);

        if (predicate != null) cards = cards.Where(predicate);
        return new Deck<StandardCard>(cards);
    }
}