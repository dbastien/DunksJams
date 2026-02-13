using System;
using System.Collections.Generic;

public enum PokerHandCategory
{
    HighCard = 1,
    OnePair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush
}

public readonly struct PokerHand : IComparable<PokerHand>
{
    public PokerHandCategory Category { get; }
    public IReadOnlyList<int> Values { get; }
    public string Description { get; }

    public PokerHand(PokerHandCategory category, IReadOnlyList<int> values, string description)
    {
        Category = category;
        Values = values;
        Description = description;
    }

    public int CompareTo(PokerHand other)
    {
        var categoryCompare = Category.CompareTo(other.Category);
        if (categoryCompare != 0) return categoryCompare;

        var count = Math.Min(Values.Count, other.Values.Count);
        for (var i = 0; i < count; ++i)
        {
            var valueCompare = Values[i].CompareTo(other.Values[i]);
            if (valueCompare != 0) return valueCompare;
        }

        return Values.Count.CompareTo(other.Values.Count);
    }

    public override string ToString() => string.IsNullOrEmpty(Description) ? Category.ToString() : Description;
}