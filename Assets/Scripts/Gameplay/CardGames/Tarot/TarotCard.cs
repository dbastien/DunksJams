public class TarotCard : CardBase
{
    public enum Arcana { Major, Minor }

    public enum MajorArcana
    {
        TheFool = 0, TheMagician, TheHighPriestess, TheEmpress, TheEmperor,
        TheHierophant, TheLovers, TheChariot, Strength, TheHermit,
        WheelOfFortune, Justice, TheHangedMan, Death, Temperance,
        TheDevil, TheTower, TheStar, TheMoon, TheSun,
        Judgement, TheWorld
    }

    public enum Suit { Wands, Cups, Swords, Pentacles }
    public enum Rank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Page, Knight, Queen, King }

    public Arcana ArcanaType { get; }
    public MajorArcana? Major { get; }
    public Suit? MinorSuit { get; }
    public Rank? MinorRank { get; }

    public TarotCard(MajorArcana major) : base(major.ToString())
    {
        ArcanaType = Arcana.Major;
        Major = major;
    }

    public TarotCard(Suit suit, Rank rank) : base($"{rank} of {suit}")
    {
        ArcanaType = Arcana.Minor;
        MinorSuit = suit;
        MinorRank = rank;
    }
}
