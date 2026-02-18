using System.Collections.Generic;

public class PokerGame : CardGameBase<StandardCard>
{
    private readonly PokerGameSettings _settings;
    private string _resultSummary;

    public PokerGame(PokerGameSettings settings = null, int playerCount = 2, ICardGameIO io = null)
        : base(playerCount, 1, io)
    {
        _settings = settings ?? new PokerGameSettings();
        VariantName = _settings.Variant.ToString();
    }

    protected override Deck<StandardCard> CreateDeck() => StandardDeck.CreateDeck();

    protected override void DistributeDeck(Deck<StandardCard> deck)
    {
        for (var p = 0; p < PlayerHands.Count; ++p)
        for (var i = 0; i < 5; ++i)
            DealCardToPlayer(p, true);
    }

    public override void PlayTurn(int playerIdx)
    {
        EmitPhaseChanged("Draw");
        WriteLine($"{GetPlayerName(playerIdx)} - choose cards to discard (max {_settings.MaxDiscardCount}).");
        ShowHand(playerIdx);

        List<int> discardIndices = ReadDiscardIndices(playerIdx);
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

        for (var i = 0; i < PlayerHands.Count; ++i)
        {
            PokerHand hand = PokerHandEvaluator.Evaluate(PlayerHands[i].Cards);
            hands.Add(hand);
            WriteLine($"{GetPlayerName(i)}: {PlayerHands[i]} -> {hand}");
        }

        if (hands.Count == 0)
        {
            _resultSummary = "No hands to score.";
            return;
        }

        var bestIndex = 0;
        for (var i = 1; i < hands.Count; ++i)
            if (hands[i].CompareTo(hands[bestIndex]) > 0)
                bestIndex = i;

        var winners = new List<int>();
        for (var i = 0; i < hands.Count; ++i)
            if (hands[i].CompareTo(hands[bestIndex]) == 0)
                winners.Add(i);

        if (winners.Count == 1)
            _resultSummary = $"{GetPlayerName(winners[0])} wins with {hands[winners[0]]}.";
        else
            _resultSummary = $"Tie between {FormatWinners(winners)} with {hands[bestIndex]}.";

        WriteLine(_resultSummary);
    }

    protected override string GetResultSummary() => _resultSummary;

    private void ShowHand(int playerIdx)
    {
        Hand<StandardCard> hand = PlayerHands[playerIdx];
        var lines = new List<string>(hand.Count + 1) { $"{GetPlayerName(playerIdx)} hand:" };
        for (var i = 0; i < hand.Count; ++i) lines.Add($"{i + 1}. {hand[i]}");
        WriteLines(lines);
    }

    private List<int> ReadDiscardIndices(int playerIdx)
    {
        string input = ReadText("Enter card numbers to discard (comma separated), or press Enter to keep all:");
        List<int> indices = ParseIndices(input, PlayerHands[playerIdx].Count);

        if (_settings.MaxDiscardCount > 0 && indices.Count > _settings.MaxDiscardCount)
        {
            indices.RemoveRange(_settings.MaxDiscardCount, indices.Count - _settings.MaxDiscardCount);
            WriteLine($"Only the first {_settings.MaxDiscardCount} discards are used.");
        }

        indices.Sort((a, b) => b.CompareTo(a));
        return indices;
    }

    private static List<int> ParseIndices(string input, int maxCount)
    {
        var indices = new List<int>();
        if (string.IsNullOrWhiteSpace(input)) return indices;

        string[] parts = input.Split(',', ' ', ';');
        foreach (string part in parts)
        {
            if (!int.TryParse(part, out int value)) continue;
            int index = value - 1;
            if (index < 0 || index >= maxCount) continue;
            if (!indices.Contains(index)) indices.Add(index);
        }

        return indices;
    }

    private void DiscardSelected(int playerIdx, List<int> discardIndices)
    {
        foreach (int index in discardIndices)
        {
            StandardCard card = RemoveCardFromHand(playerIdx, index);
            DiscardCard(card, playerIdx);
        }

        WriteLine($"{GetPlayerName(playerIdx)} discards {discardIndices.Count} card(s).");
    }

    private void DrawReplacements(int playerIdx, int count)
    {
        for (var i = 0; i < count; ++i)
        {
            StandardCard card = DrawCardToPlayer(playerIdx, false);
            WriteLine($"{GetPlayerName(playerIdx)} draws {card}.");
        }
    }

    private string FormatWinners(List<int> winners)
    {
        var names = new List<string>(winners.Count);
        foreach (int index in winners) names.Add(GetPlayerName(index));
        return string.Join(", ", names);
    }
}