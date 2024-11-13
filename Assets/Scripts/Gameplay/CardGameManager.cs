using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CardBase : IComparable<CardBase>
{
    public string Name { get; }
    protected CardBase(string name) => Name = name;
    public override string ToString() => Name;
    public int CompareTo(CardBase other) => other == null ? 1 : 2;
}

public class StandardCard : CardBase, IComparable<StandardCard>
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, Queen, King, Ace
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

public static class StandardCardDeck
{
    public static CardCollection<StandardCard> CreateDeck()
    {
        var suits = EnumCache<StandardCard.Suit>.Values;
        var ranks = EnumCache<StandardCard.Rank>.Values;
        return new CardCollection<StandardCard>(from s in suits from r in ranks select new StandardCard(s, r));
    }
}

public abstract class CardGameBase<TCard> : IDisposable where TCard : CardBase
{
    protected int PlayerCount { get; }
    protected int MaxRounds { get; }
    protected List<CardCollection<TCard>> PlayerDecks;
    protected abstract CardCollection<TCard> CreateDeck();    public virtual void PlayTurn(int playerIndex) => DLog.Log($"Player {playerIndex + 1}'s turn.");
    public virtual bool IsGameOver() => false;
    public virtual void ShowScores() => DLog.Log("Scores not implemented.");
    public virtual void Dispose() => DLog.Log("Game disposed.");
    
    public CardGameBase(int playerCount = 2, int maxRounds = 100)
    {
        PlayerCount = playerCount;
        MaxRounds = maxRounds;
    }
    
    public void RunGame()
    {
        Setup();
        for (int round = 0; !IsGameOver(); ++round) PlayTurn(round % PlayerDecks.Count);
        ShowScores();
    }
    
    protected virtual void Setup()
    {
        var deck = CreateDeck();
        deck.Shuffle();
        PlayerDecks = Enumerable.Range(0, PlayerCount)
            .Select(_ => new CardCollection<TCard>())
            .ToList();

        DistributeDeck(deck);
        Debug.Log("Game setup complete.");
    }
    
    protected virtual void DistributeDeck(CardCollection<TCard> deck)
    {
        int cardsPerPlayer = deck.Count / PlayerCount;
        foreach (var playerDeck in PlayerDecks) playerDeck.AddRange(deck.Take(cardsPerPlayer));
    }
}

public class CardCollection<T> : ICollection<T> where T : CardBase
{
    readonly List<T> _cards = new();

    public CardCollection() { }
    public CardCollection(IEnumerable<T> cards) => _cards.AddRange(cards);

    public void Add(T card) => _cards.Add(card);
    public void AddRange(IEnumerable<T> cards) => _cards.AddRange(cards);
    public bool Remove(T card) => _cards.Remove(card);

    public void Shuffle() => Rand.Shuffle(_cards);
    
    //todo: doesn't seem to really draw
    public T DrawFromTop() => _cards.Count > 0 ? _cards[^1] : throw new InvalidOperationException("No cards left.");
    public T PeekTop() => _cards.Count > 0 ? _cards[^1] : throw new InvalidOperationException("No cards left.");
    
    public int Count => _cards.Count;
    public bool IsReadOnly => false;

    public IEnumerator<T> GetEnumerator() => _cards.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => string.Join(", ", _cards);

    public bool Contains(T item) => _cards.Contains(item);
    public void CopyTo(T[] array, int index) => _cards.CopyTo(array, index);
    public void Clear() => _cards.Clear();
}

public class WarGame : CardGameBase<StandardCard>
{
    public WarGame() : base(playerCount: 2) { }

    protected override CardCollection<StandardCard> CreateDeck() => StandardCardDeck.CreateDeck();

    public override void PlayTurn(int playerIndex)
    {
        if (IsGameOver()) return;

        var p1Card = PlayerDecks[0].DrawFromTop();
        var p2Card = PlayerDecks[1].DrawFromTop();

        Debug.Log($"Player 1 plays {p1Card}; Player 2 plays {p2Card}");

        int result = p1Card.CardRank.CompareTo(p2Card.CardRank);
        if (result > 0)
            PlayerDecks[0].AddRange(new[] { p1Card, p2Card });
        else if (result < 0)
            PlayerDecks[1].AddRange(new[] { p1Card, p2Card });
        else
            HandleWar(p1Card, p2Card);
    }

    void HandleWar(StandardCard p1Card, StandardCard p2Card)
    {
        List<StandardCard> warCards = new() { p1Card, p2Card };
        for (int i = 0; i < 3; ++i)
        {
            if (PlayerDecks[0].Count > 0) warCards.Add(PlayerDecks[0].DrawFromTop());
            if (PlayerDecks[1].Count > 0) warCards.Add(PlayerDecks[1].DrawFromTop());
        }

        int result = warCards[^2].CompareTo(warCards[^1]);
        if (result > 0) PlayerDecks[0].AddRange(warCards.ToArray());
        else if (result < 0) PlayerDecks[1].AddRange(warCards.ToArray());
    }

    public override bool IsGameOver() => PlayerDecks[0].Count == 0 || PlayerDecks[1].Count == 0;
    public override void ShowScores()
    {
        DLog.Log($"Player 1 has {PlayerDecks[0].Count} cards; Player 2 has {PlayerDecks[1].Count} cards.");
        if (IsGameOver())
            DLog.Log(PlayerDecks[0].Count > PlayerDecks[1].Count ? "Player 1 wins!" :
                      PlayerDecks[1].Count > PlayerDecks[0].Count ? "Player 2 wins!" : "It's a draw.");
    }
}

public class UnoGame : CardGameBase<UnoGame.UnoCard>
{
    public class UnoCard : CardBase
    {
        public enum Color { Red, Blue, Green, Yellow, Wild }
        public enum Rank { Zero = 0, One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Skip, Reverse, DrawTwo, Wild, DrawFour }
        public Color CardColor { get; }
        public Rank CardRank { get; }
        public bool IsWild => CardColor == Color.Wild;
        
        static string GetCardName(Color color, Rank rank) => color == Color.Wild ? $"{rank} (Wild)" : $"{color} {rank}";
        
        public UnoCard(Color c, Rank r) : base(GetCardName(c, r)) => (CardColor, CardRank) = (c, r);
    }

    protected override CardCollection<UnoCard> CreateDeck()
    {
        var colors = EnumCache<UnoCard.Color>.Values.Where(c => c != UnoCard.Color.Wild);
        var ranks = EnumCache<UnoCard.Rank>.Values.Where(r => r <= UnoCard.Rank.DrawTwo);
        var cards = from c in colors from r in ranks select new UnoCard(c, r);
        var wilds = Enumerable.Repeat(new UnoCard(UnoCard.Color.Wild, UnoCard.Rank.DrawFour), 4);
        return new(cards.Concat(wilds));
    }
}

public class CardGameLauncher
{
    public static void Main()
    {
        var game = new WarGame();
        using (game) game.RunGame();        
    }
}