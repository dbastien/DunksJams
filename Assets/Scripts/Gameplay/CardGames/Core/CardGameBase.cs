using System;
using System.Collections.Generic;
using System.Linq;

public abstract class CardGameBase<TCard> : IDisposable where TCard : CardBase
{
    protected int PlayerCount { get; }
    protected int MaxRounds { get; }
    protected Deck<TCard> Deck { get; private set; }
    protected List<CardCollection<TCard>> PlayerHands;

    protected CardCollection<TCard> DiscardPile => Deck?.DiscardPile;

    protected abstract Deck<TCard> CreateDeck();

    public virtual void PlayTurn(int playerIdx) => DLog.Log($"Player {playerIdx + 1}'s turn.");
    public virtual bool IsGameOver() => false;
    public virtual void ShowScores() => DLog.Log("Scores not implemented.");
    public virtual void Dispose() => DLog.Log("Game disposed.");

    protected CardGameBase(int playerCount = 2, int maxRounds = 100)
    {
        PlayerCount = playerCount;
        MaxRounds = maxRounds;
    }

    public void RunGame()
    {
        Setup();
        for (int round = 0; round < MaxRounds && !IsGameOver(); ++round)
            PlayTurn(round % PlayerHands.Count);

        if (!IsGameOver() && MaxRounds > 0)
            DLog.LogW($"Max rounds reached ({MaxRounds}).");

        ShowScores();
    }

    protected virtual void Setup()
    {
        Deck = CreateDeck();
        Deck.Shuffle();
        PlayerHands = Enumerable.Range(0, PlayerCount)
            .Select(_ => new CardCollection<TCard>())
            .ToList();

        DistributeDeck(Deck);
        DLog.Log("Game setup complete.");
    }

    protected virtual void DistributeDeck(Deck<TCard> deck)
    {
        int cardsPerPlayer = deck.Count / PlayerCount;
        foreach (var playerHand in PlayerHands)
        {
            for (int i = 0; i < cardsPerPlayer; ++i)
                playerHand.Add(deck.Draw());
        }
    }
}
