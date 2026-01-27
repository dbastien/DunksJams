using System.Collections.Generic;
using System.Linq;

public static class UnoDeck
{
    public static Deck<UnoCard> CreateDeck()
    {
        var colors = EnumCache<UnoCard.Color>.Values.Where(c => c != UnoCard.Color.Wild).ToArray();
        var cards = new List<UnoCard>(108);

        foreach (var color in colors)
        {
            cards.Add(new UnoCard(color, UnoCard.Rank.Zero));

            for (var rank = UnoCard.Rank.One; rank <= UnoCard.Rank.Nine; ++rank)
            {
                cards.Add(new UnoCard(color, rank));
                cards.Add(new UnoCard(color, rank));
            }

            var actionRanks = new[] { UnoCard.Rank.Skip, UnoCard.Rank.Reverse, UnoCard.Rank.DrawTwo };
            foreach (var rank in actionRanks)
            {
                cards.Add(new UnoCard(color, rank));
                cards.Add(new UnoCard(color, rank));
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            cards.Add(new UnoCard(UnoCard.Color.Wild, UnoCard.Rank.Wild));
            cards.Add(new UnoCard(UnoCard.Color.Wild, UnoCard.Rank.DrawFour));
        }

        return new Deck<UnoCard>(cards, autoRecycleDiscard: true, keepTopDiscardOnRecycle: true);
    }
}
