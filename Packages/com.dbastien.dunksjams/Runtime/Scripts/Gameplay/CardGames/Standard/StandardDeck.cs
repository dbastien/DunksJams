using System;
using System.Collections.Generic;
using System.Linq;

public static class StandardDeck
{
    public static Deck<StandardCard> CreateDeck(Func<StandardCard, bool> predicate = null)
    {
        StandardCard.Suit[] suits = EnumCache<StandardCard.Suit>.Values;
        StandardCard.Rank[] ranks = EnumCache<StandardCard.Rank>.Values;

        IEnumerable<StandardCard> cards = from s in suits
            from r in ranks
            select new StandardCard(s, r);

        if (predicate != null) cards = cards.Where(predicate);
        return new Deck<StandardCard>(cards);
    }
}