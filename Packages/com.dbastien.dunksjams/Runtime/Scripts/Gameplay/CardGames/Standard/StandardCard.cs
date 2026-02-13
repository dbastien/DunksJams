using System;

public class StandardCard : CardBase, IComparable<StandardCard>
{
    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum Rank
    {
        Two = 2,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    public Suit CardSuit { get; }
    public Rank CardRank { get; }

    public StandardCard(Suit suit, Rank rank) : base($"{rank} of {suit}")
    {
        CardSuit = suit;
        CardRank = rank;
    }

    public int CompareTo(StandardCard other) => other == null ? 1 : CardRank.CompareTo(other.CardRank);
    public override string ToString() => $"{CardRank} of {CardSuit}";
}