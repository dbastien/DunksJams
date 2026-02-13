using System.Collections.Generic;

public class WarGame : CardGameBase<StandardCard>
{
    public WarGame(string variantName = "Standard", ICardGameIO io = null)
        : base(2, io: io) => VariantName = variantName;

    protected override Deck<StandardCard> CreateDeck() => StandardDeck.CreateDeck();

    public override void PlayTurn(int playerIdx)
    {
        if (IsGameOver()) return;

        var p1Card = RemoveCardFromHand(0, PlayerHands[0].Count - 1);
        var p2Card = RemoveCardFromHand(1, PlayerHands[1].Count - 1);
        EmitCardPlayed(0, p1Card);
        EmitCardPlayed(1, p2Card);

        WriteLine($"{GetPlayerName(0)} plays {p1Card}; {GetPlayerName(1)} plays {p2Card}");

        var result = p1Card.CardRank.CompareTo(p2Card.CardRank);
        if (result > 0)
            PlayerHands[0].AddRange(new[] { p1Card, p2Card });
        else if (result < 0)
            PlayerHands[1].AddRange(new[] { p1Card, p2Card });
        else
            HandleWar(p1Card, p2Card);
    }

    void HandleWar(StandardCard p1Card, StandardCard p2Card)
    {
        List<StandardCard> warCards = new() { p1Card, p2Card };
        for (var i = 0; i < 3; ++i)
        {
            if (PlayerHands[0].Count > 0) warCards.Add(PlayerHands[0].DrawFromTop());
            if (PlayerHands[1].Count > 0) warCards.Add(PlayerHands[1].DrawFromTop());
        }

        var result = warCards[^2].CompareTo(warCards[^1]);
        if (result > 0) PlayerHands[0].AddRange(warCards.ToArray());
        else if (result < 0) PlayerHands[1].AddRange(warCards.ToArray());
    }

    public override bool IsGameOver() => PlayerHands[0].Count == 0 || PlayerHands[1].Count == 0;

    public override void ShowScores()
    {
        WriteLine(
            $"{GetPlayerName(0)} has {PlayerHands[0].Count} cards; {GetPlayerName(1)} has {PlayerHands[1].Count} cards.");
        var result = GetResultSummary();
        if (!string.IsNullOrEmpty(result)) WriteLine(result);
    }

    protected override string GetResultSummary()
    {
        if (!IsGameOver()) return null;
        return PlayerHands[0].Count > PlayerHands[1].Count ? $"{GetPlayerName(0)} wins!" :
            PlayerHands[1].Count > PlayerHands[0].Count ? $"{GetPlayerName(1)} wins!" : "It's a draw.";
    }
}