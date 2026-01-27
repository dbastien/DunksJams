using System.Collections.Generic;

public class PokerGame : CardGameBase<StandardCard>
{
    readonly PokerGameSettings _settings;
    string _resultSummary;

    public PokerGame(PokerGameSettings settings = null, int playerCount = 2, ICardGameIO io = null)
        : base(playerCount, maxRounds: 1, io: io)
    {
        _settings = settings ?? new PokerGameSettings();
        VariantName = _settings.Variant.ToString();
    }

    protected override Deck<StandardCard> CreateDeck() => StandardDeck.CreateDeck();

    protected override void DistributeDeck(Deck<StandardCard> deck)
    {
        for (int p = 0; p < PlayerHands.Count; ++p)
        {
            for (int i = 0; i < 5; ++i)
                DealCardToPlayer(p, isFaceDown: true);
        }
    }

    public override void PlayTurn(int playerIdx)
    {
        EmitPhaseChanged("Draw");
        WriteLine($"{GetPlayerName(playerIdx)} - choose cards to discard (max {_settings.MaxDiscardCount}).");
        ShowHand(playerIdx);

        var discardIndices = ReadDiscardIndices(playerIdx);
        if (discardIndices.Count == 0)
        {
            WriteLine($"{GetPlayerName(playerIdx)} stands pat.");
            return;
        }

        DiscardSelected(playerIdx, discardIndices);
        DrawReplacements(playerIdx, discardIndices.Count);
    }

    public override void ShowScores()
    {
        EmitPhaseChanged("Showdown");
        var hands = new List<PokerHand>(PlayerHands.Count);

        for (int i = 0; i < PlayerHands.Count; ++i)
        {
            var hand = PokerHandEvaluator.Evaluate(PlayerHands[i].Cards);
            hands.Add(hand);
            WriteLine($"{GetPlayerName(i)}: {PlayerHands[i]} -> {hand}");
        }

        if (hands.Count == 0)
        {
            _resultSummary = "No hands to score.";
            return;
        }

        int bestIndex = 0;
        for (int i = 1; i < hands.Count; ++i)
        {
            if (hands[i].CompareTo(hands[bestIndex]) > 0) bestIndex = i;
        }

        var winners = new List<int>();
        for (int i = 0; i < hands.Count; ++i)
        {
            if (hands[i].CompareTo(hands[bestIndex]) == 0) winners.Add(i);
        }

        if (winners.Count == 1)
            _resultSummary = $"{GetPlayerName(winners[0])} wins with {hands[winners[0]]}.";
        else
            _resultSummary = $"Tie between {FormatWinners(winners)} with {hands[bestIndex]}.";

        WriteLine(_resultSummary);
    }

    protected override string GetResultSummary() => _resultSummary;

    void ShowHand(int playerIdx)
    {
        var hand = PlayerHands[playerIdx];
        var lines = new List<string>(hand.Count + 1) { $"{GetPlayerName(playerIdx)} hand:" };
        for (int i = 0; i < hand.Count; ++i) lines.Add($"{i + 1}. {hand[i]}");
        WriteLines(lines);
    }

    List<int> ReadDiscardIndices(int playerIdx)
    {
        string input = ReadText("Enter card numbers to discard (comma separated), or press Enter to keep all:", "");
        var indices = ParseIndices(input, PlayerHands[playerIdx].Count);

        if (_settings.MaxDiscardCount > 0 && indices.Count > _settings.MaxDiscardCount)
        {
            indices.RemoveRange(_settings.MaxDiscardCount, indices.Count - _settings.MaxDiscardCount);
            WriteLine($"Only the first {_settings.MaxDiscardCount} discards are used.");
        }

        indices.Sort((a, b) => b.CompareTo(a));
        return indices;
    }

    static List<int> ParseIndices(string input, int maxCount)
    {
        var indices = new List<int>();
        if (string.IsNullOrWhiteSpace(input)) return indices;

        string[] parts = input.Split(',', ' ', ';');
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out int value)) continue;
            int index = value - 1;
            if (index < 0 || index >= maxCount) continue;
            if (!indices.Contains(index)) indices.Add(index);
        }

        return indices;
    }

    void DiscardSelected(int playerIdx, List<int> discardIndices)
    {
        foreach (int index in discardIndices)
        {
            var card = RemoveCardFromHand(playerIdx, index);
            DiscardCard(card, playerIdx);
        }

        WriteLine($"{GetPlayerName(playerIdx)} discards {discardIndices.Count} card(s).");
    }

    void DrawReplacements(int playerIdx, int count)
    {
        for (int i = 0; i < count; ++i)
        {
            var card = DrawCardToPlayer(playerIdx, isFaceDown: false);
            WriteLine($"{GetPlayerName(playerIdx)} draws {card}.");
        }
    }

    string FormatWinners(List<int> winners)
    {
        var names = new List<string>(winners.Count);
        foreach (int index in winners) names.Add(GetPlayerName(index));
        return string.Join(", ", names);
    }
}
