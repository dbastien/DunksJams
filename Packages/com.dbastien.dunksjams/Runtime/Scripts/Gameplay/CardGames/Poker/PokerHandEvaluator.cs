using System;
using System.Collections.Generic;

public static class PokerHandEvaluator
{
    private readonly struct RankGroup
    {
        public readonly int Rank;
        public readonly int Count;

        public RankGroup(int rank, int count)
        {
            Rank = rank;
            Count = count;
        }
    }

    public static PokerHand Evaluate(IReadOnlyList<StandardCard> cards)
    {
        if (cards == null || cards.Count != 5)
            throw new ArgumentException("Poker hand must contain exactly 5 cards.");

        var ranks = new int[5];
        var suits = new StandardCard.Suit[5];
        for (var i = 0; i < cards.Count; ++i)
        {
            ranks[i] = (int)cards[i].CardRank;
            suits[i] = cards[i].CardSuit;
        }

        Array.Sort(ranks);
        bool isFlush = IsFlush(suits);
        bool isStraight = IsStraight(ranks, out int straightHigh);

        var rankCounts = new Dictionary<int, int>();
        for (var i = 0; i < ranks.Length; ++i)
            if (rankCounts.ContainsKey(ranks[i])) rankCounts[ranks[i]]++;
            else rankCounts[ranks[i]] = 1;

        var groups = new List<RankGroup>(rankCounts.Count);
        foreach (KeyValuePair<int, int> kvp in rankCounts) groups.Add(new RankGroup(kvp.Key, kvp.Value));
        groups.Sort((a, b) => a.Count != b.Count ? b.Count.CompareTo(a.Count) : b.Rank.CompareTo(a.Rank));

        if (isStraight && isFlush)
            return new PokerHand(
                PokerHandCategory.StraightFlush,
                new[] { straightHigh },
                $"Straight Flush ({RankName(straightHigh)} high)");

        if (groups[0].Count == 4)
        {
            int fourRank = groups[0].Rank;
            int kicker = groups[1].Rank;
            return new PokerHand(
                PokerHandCategory.FourOfAKind,
                new[] { fourRank, kicker },
                $"Four of a Kind ({RankPluralName(fourRank)})");
        }

        if (groups[0].Count == 3 && groups.Count == 2)
        {
            int trips = groups[0].Rank;
            int pair = groups[1].Rank;
            return new PokerHand(
                PokerHandCategory.FullHouse,
                new[] { trips, pair },
                $"Full House ({RankPluralName(trips)} over {RankPluralName(pair)})");
        }

        if (isFlush)
        {
            int[] sorted = SortDescending(ranks);
            return new PokerHand(
                PokerHandCategory.Flush,
                sorted,
                $"Flush ({RankName(sorted[0])} high)");
        }

        if (isStraight)
            return new PokerHand(
                PokerHandCategory.Straight,
                new[] { straightHigh },
                $"Straight ({RankName(straightHigh)} high)");

        if (groups[0].Count == 3)
        {
            int trips = groups[0].Rank;
            List<int> kickers = CollectRanks(groups, 1);
            var values = new List<int>(1 + kickers.Count) { trips };
            values.AddRange(kickers);
            return new PokerHand(
                PokerHandCategory.ThreeOfAKind,
                values.ToArray(),
                $"Three of a Kind ({RankPluralName(trips)})");
        }

        if (groups[0].Count == 2 && groups[1].Count == 2)
        {
            int highPair = Math.Max(groups[0].Rank, groups[1].Rank);
            int lowPair = Math.Min(groups[0].Rank, groups[1].Rank);
            int kicker = groups.Count > 2 ? groups[2].Rank : 0;
            return new PokerHand(
                PokerHandCategory.TwoPair,
                new[] { highPair, lowPair, kicker },
                $"Two Pair ({RankPluralName(highPair)} and {RankPluralName(lowPair)})");
        }

        if (groups[0].Count == 2)
        {
            int pair = groups[0].Rank;
            List<int> kickers = CollectRanks(groups, 1);
            var values = new List<int>(1 + kickers.Count) { pair };
            values.AddRange(kickers);
            return new PokerHand(
                PokerHandCategory.OnePair,
                values.ToArray(),
                $"One Pair ({RankPluralName(pair)})");
        }

        int[] highCards = SortDescending(ranks);
        return new PokerHand(
            PokerHandCategory.HighCard,
            highCards,
            $"High Card ({RankName(highCards[0])} high)");
    }

    private static bool IsFlush(StandardCard.Suit[] suits)
    {
        for (var i = 1; i < suits.Length; ++i)
            if (suits[i] != suits[0])
                return false;

        return true;
    }

    private static bool IsStraight(int[] ranks, out int high)
    {
        high = 0;
        for (var i = 1; i < ranks.Length; ++i)
            if (ranks[i] == ranks[i - 1])
                return false;

        bool sequential = ranks[4] - ranks[0] == 4;
        if (sequential)
        {
            high = ranks[4];
            return true;
        }

        bool wheel = ranks[0] == 2 && ranks[1] == 3 && ranks[2] == 4 && ranks[3] == 5 && ranks[4] == 14;
        if (wheel)
        {
            high = 5;
            return true;
        }

        return false;
    }

    private static int[] SortDescending(int[] ranks)
    {
        var copy = (int[])ranks.Clone();
        Array.Reverse(copy);
        return copy;
    }

    private static List<int> CollectRanks(List<RankGroup> groups, int count)
    {
        var ranks = new List<int>();
        for (var i = 0; i < groups.Count; ++i)
            if (groups[i].Count == count)
                ranks.Add(groups[i].Rank);

        ranks.Sort((a, b) => b.CompareTo(a));
        return ranks;
    }

    private static string RankName(int rank) => ((StandardCard.Rank)rank).ToString();

    private static string RankPluralName(int rank)
    {
        string name = RankName(rank);
        return name == "Six" ? "Sixes" : $"{name}s";
    }
}