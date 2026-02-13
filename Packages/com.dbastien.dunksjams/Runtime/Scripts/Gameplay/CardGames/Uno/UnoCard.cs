public class UnoCard : CardBase
{
    public enum Color
    {
        Red,
        Blue,
        Green,
        Yellow,
        Wild
    }

    public enum Rank
    {
        Zero = 0,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Skip,
        Reverse,
        DrawTwo,
        Wild,
        DrawFour
    }

    public Color CardColor { get; }
    public Rank CardRank { get; }
    public bool IsWild => CardColor == Color.Wild;

    static string GetCardName(Color c, Rank rank) =>
        c == Color.Wild ? $"{rank} (Wild)" : $"{c} {rank}";

    public UnoCard(Color c, Rank r) : base(GetCardName(c, r)) => (CardColor, CardRank) = (c, r);
}