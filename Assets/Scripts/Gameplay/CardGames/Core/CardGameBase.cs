using System;
using System.Collections.Generic;
using System.Linq;

public abstract class CardGameBase<TCard> : IDisposable where TCard : CardBase
{
    protected int PlayerCount { get; }
    protected int MaxRounds { get; }
    protected Deck<TCard> Deck { get; set; }
    protected List<Hand<TCard>> PlayerHands;
    protected ICardGameIO IO { get; }
    protected int TurnIndex { get; set; }
    public string VariantName { get; protected set; } = "Standard";

    protected CardCollection<TCard> DiscardPile => Deck?.DiscardPile;
    protected virtual string GameId => GetType().Name;

    protected abstract Deck<TCard> CreateDeck();

    public virtual void PlayTurn(int playerIdx) => WriteLine($"Player {playerIdx + 1}'s turn.");
    public virtual bool IsGameOver() => false;
    public virtual void ShowScores() => WriteLine("Scores not implemented.");
    public virtual void Dispose() => DLog.Log("Game disposed.");

    protected CardGameBase(int playerCount = 2, int maxRounds = 100, ICardGameIO io = null)
    {
        PlayerCount = playerCount;
        MaxRounds = maxRounds;
        IO = io ?? CardGameIO.Default;
    }

    public virtual void RunGame()
    {
        Setup();
        EmitGameStarted();
        TurnIndex = 0;

        for (int round = 0; round < MaxRounds && !IsGameOver(); ++round)
        {
            int playerIdx = round % PlayerHands.Count;
            EmitTurnStarted(round, playerIdx);
            PlayTurn(playerIdx);
            EmitTurnEnded(round, playerIdx);
            TurnIndex = round + 1;
        }

        if (!IsGameOver() && MaxRounds > 0)
            DLog.LogW($"Max rounds reached ({MaxRounds}).");

        ShowScores();
        EmitGameEnded();
    }

    protected virtual void Setup()
    {
        Deck = CreateDeck();
        Deck.Shuffle();
        PlayerHands = Enumerable.Range(0, PlayerCount)
            .Select(_ => new Hand<TCard>())
            .ToList();

        DistributeDeck(Deck);
        DLog.Log("Game setup complete.");
    }

    protected virtual void DistributeDeck(Deck<TCard> deck)
    {
        int cardsPerPlayer = deck.Count / PlayerCount;
        for (int p = 0; p < PlayerHands.Count; ++p)
        {
            for (int i = 0; i < cardsPerPlayer; ++i)
                DealCardToPlayer(p, isFaceDown: true);
        }
    }

    protected virtual string GetPlayerName(int playerIndex) => $"Player {playerIndex + 1}";

    protected void WriteLine(string message) => IO?.WriteLine(message);
    protected void WriteLines(IEnumerable<string> lines) => IO?.WriteLines(lines);

    protected string ReadText(string prompt, string defaultValue = "") =>
        IO?.ReadText(prompt, defaultValue) ?? defaultValue;

    protected int ReadInt(string prompt, int min, int max, int defaultValue) =>
        IO?.ReadInt(prompt, min, max, defaultValue) ?? defaultValue;

    protected int ReadChoice(string prompt, IReadOnlyList<string> options, int defaultIndex = 0) =>
        IO?.ReadChoice(prompt, options, defaultIndex) ?? defaultIndex;

    protected TCard DealCardToPlayer(int playerIndex, bool isFaceDown)
    {
        var card = Deck.Draw();
        PlayerHands[playerIndex].Add(card);
        EmitCardDealt(playerIndex, card, isFaceDown);
        return card;
    }

    protected TCard DrawCardToPlayer(int playerIndex, bool isFaceDown)
    {
        var card = Deck.Draw();
        PlayerHands[playerIndex].Add(card);
        EmitCardDrawn(playerIndex, card, isFaceDown);
        return card;
    }

    protected TCard RemoveCardFromHand(int playerIndex, int cardIndex)
    {
        var card = PlayerHands[playerIndex][cardIndex];
        PlayerHands[playerIndex].RemoveAt(cardIndex);
        return card;
    }

    protected void DiscardCard(TCard card, int playerIndex, bool isFaceDown = false)
    {
        Deck.Discard(card);
        EmitCardDiscarded(playerIndex, card, isFaceDown);
    }

    protected void EmitGameStarted() =>
        EventManager.TriggerEvent(new CardGameStartedEvent
        {
            GameId = GameId,
            PlayerCount = PlayerCount,
            Variant = VariantName
        });

    protected void EmitGameEnded() =>
        EventManager.TriggerEvent(new CardGameEndedEvent
        {
            GameId = GameId,
            ResultSummary = GetResultSummary()
        });

    protected virtual string GetResultSummary() => null;

    protected void EmitTurnStarted(int turnIndex, int playerIndex) =>
        EventManager.TriggerEvent(new CardGameTurnStartedEvent
        {
            GameId = GameId,
            TurnIndex = turnIndex,
            PlayerIndex = playerIndex
        });

    protected void EmitTurnEnded(int turnIndex, int playerIndex) =>
        EventManager.TriggerEvent(new CardGameTurnEndedEvent
        {
            GameId = GameId,
            TurnIndex = turnIndex,
            PlayerIndex = playerIndex
        });

    protected void EmitCardDealt(int playerIndex, TCard card, bool isFaceDown) =>
        EventManager.TriggerEvent(new CardGameCardDealtEvent
        {
            GameId = GameId,
            PlayerIndex = playerIndex,
            Card = card,
            IsFaceDown = isFaceDown
        });

    protected void EmitCardDrawn(int playerIndex, TCard card, bool isFaceDown) =>
        EventManager.TriggerEvent(new CardGameCardDrawnEvent
        {
            GameId = GameId,
            PlayerIndex = playerIndex,
            Card = card,
            IsFaceDown = isFaceDown
        });

    protected void EmitCardPlayed(int playerIndex, TCard card, bool isFaceDown = false) =>
        EventManager.TriggerEvent(new CardGameCardPlayedEvent
        {
            GameId = GameId,
            PlayerIndex = playerIndex,
            Card = card,
            IsFaceDown = isFaceDown
        });

    protected void EmitCardDiscarded(int playerIndex, TCard card, bool isFaceDown = false) =>
        EventManager.TriggerEvent(new CardGameCardDiscardedEvent
        {
            GameId = GameId,
            PlayerIndex = playerIndex,
            Card = card,
            IsFaceDown = isFaceDown
        });

    protected void EmitPhaseChanged(string phase) =>
        EventManager.TriggerEvent(new CardGamePhaseChangedEvent
        {
            GameId = GameId,
            Phase = phase
        });
}
