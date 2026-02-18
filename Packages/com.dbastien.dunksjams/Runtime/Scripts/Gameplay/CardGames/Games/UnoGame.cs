using System.Collections.Generic;

public class UnoGame : CardGameBase<UnoCard>
{
    private readonly UnoGameSettings _settings;
    private int _currentPlayerIndex;
    private int _direction = 1;
    private int _pendingDraw;
    private bool _skipNext;
    private bool _gameOver;
    private int _winnerIndex = -1;
    private UnoCard.Color _currentColor;

    private static readonly UnoCard.Color[] PlayableColors =
    {
        UnoCard.Color.Red,
        UnoCard.Color.Blue,
        UnoCard.Color.Green,
        UnoCard.Color.Yellow
    };

    public UnoGame(UnoGameSettings settings = null, int playerCount = 2, int maxRounds = 300, ICardGameIO io = null)
        : base(playerCount, maxRounds, io)
    {
        _settings = settings ?? new UnoGameSettings();
        VariantName = _settings.Mode.ToString();
    }

    protected override Deck<UnoCard> CreateDeck() => UnoDeck.CreateDeck();

    public override void RunGame()
    {
        Setup();
        EmitGameStarted();
        _currentPlayerIndex = 0;
        _direction = 1;
        var turnIndex = 0;

        while (!_gameOver && turnIndex < MaxRounds)
        {
            TurnIndex = turnIndex;
            EmitTurnStarted(turnIndex, _currentPlayerIndex);
            PlayTurn(_currentPlayerIndex);
            EmitTurnEnded(turnIndex, _currentPlayerIndex);

            if (_gameOver) break;

            AdvancePlayer();
            ++turnIndex;
        }

        if (!_gameOver && MaxRounds > 0)
            DLog.LogW($"Max turns reached ({MaxRounds}).");

        ShowScores();
        EmitGameEnded();
    }

    protected override void Setup()
    {
        Deck = CreateDeck();
        Deck.Shuffle();
        PlayerHands = new List<Hand<UnoCard>>(PlayerCount);
        for (var i = 0; i < PlayerCount; ++i) PlayerHands.Add(new Hand<UnoCard>());

        if (_settings.PromptForMode) PromptForMode();
        VariantName = _settings.Mode.ToString();

        DealStartingHands();
        PlaceOpeningCard();
        DLog.Log("Uno setup complete.");
    }

    protected override void DistributeDeck(Deck<UnoCard> deck)
    {
        // Uno uses a fixed starting hand size, handled in Setup.
    }

    public override void PlayTurn(int playerIdx)
    {
        if (_gameOver) return;

        if (_skipNext)
        {
            WriteLine($"{GetPlayerName(playerIdx)} is skipped.");
            _skipNext = false;
            return;
        }

        if (_pendingDraw > 0)
        {
            if (_settings.AllowStackingDraws && TryStackDraw(playerIdx)) return;

            DrawCards(playerIdx, _pendingDraw);
            WriteLine($"{GetPlayerName(playerIdx)} draws {_pendingDraw} and skips.");
            _pendingDraw = 0;
            return;
        }

        WriteLine($"Top card: {DiscardPile.PeekTop()} (Color: {_currentColor}).");
        ShowHand(playerIdx);

        List<int> playableIndices = GetPlayableIndices(PlayerHands[playerIdx]);
        if (playableIndices.Count == 0)
        {
            DrawAndMaybePlay(playerIdx);
            return;
        }

        if (_settings.AllowDrawWhenPlayable)
        {
            List<string> options = BuildPlayableOptions(playerIdx, playableIndices, true);
            int choice = ReadChoice($"{GetPlayerName(playerIdx)} - choose a card:", options);
            if (choice == options.Count - 1)
            {
                DrawAndMaybePlay(playerIdx);
                return;
            }

            PlayCard(playerIdx, playableIndices[choice]);
            return;
        }

        List<string> playOptions = BuildPlayableOptions(playerIdx, playableIndices, false);
        int selected = ReadChoice($"{GetPlayerName(playerIdx)} - choose a card:", playOptions);
        PlayCard(playerIdx, playableIndices[selected]);
    }

    public override bool IsGameOver() => _gameOver;

    public override void ShowScores()
    {
        for (var i = 0; i < PlayerHands.Count; ++i)
            WriteLine($"{GetPlayerName(i)} has {PlayerHands[i].Count} cards.");

        string result = GetResultSummary();
        if (!string.IsNullOrEmpty(result)) WriteLine(result);
    }

    protected override string GetResultSummary() =>
        _winnerIndex >= 0 ? $"{GetPlayerName(_winnerIndex)} wins!" : null;

    private void PromptForMode()
    {
        var options = new List<string> { "Standard", "Stacking Draws" };
        int choice = ReadChoice("Choose Uno mode:", options, (int)_settings.Mode);
        _settings.Mode = choice == 1 ? UnoGameSettings.UnoMode.StackingDraws : UnoGameSettings.UnoMode.Standard;
    }

    private void DealStartingHands()
    {
        for (var p = 0; p < PlayerCount; ++p)
        for (var i = 0; i < _settings.StartingHandSize; ++i)
            DealCardToPlayer(p, true);
    }

    private void PlaceOpeningCard()
    {
        UnoCard openingCard = DrawOpeningCard();
        DiscardCard(openingCard, -1);
        _currentColor = openingCard.IsWild ? ChooseColor(-1) : openingCard.CardColor;
        ApplyCardEffects(openingCard, true);
        WriteLine($"Opening card: {openingCard}. Current color: {_currentColor}.");
    }

    private UnoCard DrawOpeningCard()
    {
        while (true)
        {
            UnoCard card = Deck.Draw();
            if (card.CardRank == UnoCard.Rank.DrawFour)
            {
                Deck.DrawPile.Add(card);
                Deck.Shuffle();
                continue;
            }

            return card;
        }
    }

    private void AdvancePlayer() => _currentPlayerIndex = WrapIndex(_currentPlayerIndex + _direction);

    private int WrapIndex(int value)
    {
        int mod = value % PlayerCount;
        return mod < 0 ? mod + PlayerCount : mod;
    }

    private void ShowHand(int playerIdx)
    {
        Hand<UnoCard> hand = PlayerHands[playerIdx];
        var lines = new List<string>(hand.Count + 1) { $"{GetPlayerName(playerIdx)} hand:" };
        for (var i = 0; i < hand.Count; ++i) lines.Add($"{i + 1}. {hand[i]}");
        WriteLines(lines);
    }

    private List<int> GetPlayableIndices(CardCollection<UnoCard> hand)
    {
        var playable = new List<int>();
        for (var i = 0; i < hand.Count; ++i)
            if (IsPlayable(hand[i]))
                playable.Add(i);

        return playable;
    }

    private bool IsPlayable(UnoCard card)
    {
        if (card.IsWild) return true;
        if (card.CardColor == _currentColor) return true;
        return card.CardRank == DiscardPile.PeekTop().CardRank;
    }

    private List<string> BuildPlayableOptions(int playerIdx, List<int> playableIndices, bool includeDrawOption)
    {
        var options = new List<string>(playableIndices.Count + (includeDrawOption ? 1 : 0));
        foreach (int index in playableIndices) options.Add(PlayerHands[playerIdx][index].ToString());
        if (includeDrawOption) options.Add("Draw a card");
        return options;
    }

    private void DrawAndMaybePlay(int playerIdx)
    {
        UnoCard drawn = DrawCardToPlayer(playerIdx, false);
        WriteLine($"{GetPlayerName(playerIdx)} draws {drawn}.");

        if (!IsPlayable(drawn)) return;

        int choice = ReadChoice("Play the drawn card?", new List<string> { "Play", "Keep" });
        if (choice == 0) PlayCard(playerIdx, PlayerHands[playerIdx].Count - 1);
    }

    private bool TryStackDraw(int playerIdx)
    {
        Hand<UnoCard> hand = PlayerHands[playerIdx];
        var drawIndices = new List<int>();
        for (var i = 0; i < hand.Count; ++i)
            if (IsDrawCard(hand[i]))
                drawIndices.Add(i);

        if (drawIndices.Count == 0) return false;

        var options = new List<string>(drawIndices.Count + 1);
        foreach (int index in drawIndices) options.Add(hand[index].ToString());
        options.Add($"Take {_pendingDraw} cards");

        int choice = ReadChoice($"{GetPlayerName(playerIdx)} has {_pendingDraw} to draw. Stack a draw card?", options,
            options.Count - 1);
        if (choice == options.Count - 1) return false;

        PlayCard(playerIdx, drawIndices[choice]);
        return true;
    }

    private bool IsDrawCard(UnoCard card) =>
        card.CardRank == UnoCard.Rank.DrawTwo || card.CardRank == UnoCard.Rank.DrawFour;

    private void DrawCards(int playerIdx, int count)
    {
        for (var i = 0; i < count; ++i)
        {
            UnoCard card = DrawCardToPlayer(playerIdx, false);
            WriteLine($"{GetPlayerName(playerIdx)} draws {card}.");
        }
    }

    private void PlayCard(int playerIdx, int cardIndex)
    {
        UnoCard card = RemoveCardFromHand(playerIdx, cardIndex);
        EmitCardPlayed(playerIdx, card);
        DiscardCard(card, playerIdx);

        if (card.IsWild) _currentColor = ChooseColor(playerIdx);
        else _currentColor = card.CardColor;

        ApplyCardEffects(card, false);
        HandleUnoState(playerIdx);
    }

    private void ApplyCardEffects(UnoCard card, bool isOpeningCard)
    {
        switch (card.CardRank)
        {
            case UnoCard.Rank.Skip:
                _skipNext = true;
                break;
            case UnoCard.Rank.Reverse:
                if (PlayerCount == 2) _skipNext = true;
                else _direction *= -1;
                break;
            case UnoCard.Rank.DrawTwo:
                _pendingDraw += 2;
                break;
            case UnoCard.Rank.DrawFour:
                _pendingDraw += 4;
                break;
        }

        if (!isOpeningCard) return;

        if (card.CardRank == UnoCard.Rank.Skip)
            WriteLine("Opening card is Skip. Next player is skipped.");
        else if (card.CardRank == UnoCard.Rank.Reverse)
            WriteLine("Opening card is Reverse. Direction changed.");
        else if (card.CardRank == UnoCard.Rank.DrawTwo)
            WriteLine("Opening card is Draw Two. Next player draws two.");
    }

    private UnoCard.Color ChooseColor(int playerIdx)
    {
        var options = new List<string> { "Red", "Blue", "Green", "Yellow" };
        string prompt = playerIdx >= 0
            ? $"{GetPlayerName(playerIdx)} - choose a color:"
            : "Choose opening color:";
        int choice = ReadChoice(prompt, options);
        return PlayableColors[choice];
    }

    private void HandleUnoState(int playerIdx)
    {
        int count = PlayerHands[playerIdx].Count;
        if (count == 1) WriteLine($"{GetPlayerName(playerIdx)} says UNO!");
        if (count > 0) return;

        _gameOver = true;
        _winnerIndex = playerIdx;
    }
}