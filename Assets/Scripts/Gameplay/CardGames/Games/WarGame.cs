using System.Collections.Generic;

public class WarGame : CardGameBase<StandardCard>
{
    public WarGame() : base(playerCount: 2) { }

    protected override Deck<StandardCard> CreateDeck() => StandardDeck.CreateDeck();

    public override void PlayTurn(int playerIdx)
    {
        if (IsGameOver()) return;

        var p1Card = PlayerHands[0].DrawFromTop();
        var p2Card = PlayerHands[1].DrawFromTop();

        DLog.Log($"Player 1 plays {p1Card}; Player 2 plays {p2Card}");

        int result = p1Card.CardRank.CompareTo(p2Card.CardRank);
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
        for (int i = 0; i < 3; ++i)
        {
            if (PlayerHands[0].Count > 0) warCards.Add(PlayerHands[0].DrawFromTop());
            if (PlayerHands[1].Count > 0) warCards.Add(PlayerHands[1].DrawFromTop());
        }

        int result = warCards[^2].CompareTo(warCards[^1]);
        if (result > 0) PlayerHands[0].AddRange(warCards.ToArray());
        else if (result < 0) PlayerHands[1].AddRange(warCards.ToArray());
    }

    public override bool IsGameOver() => PlayerHands[0].Count == 0 || PlayerHands[1].Count == 0;

    public override void ShowScores()
    {
        DLog.Log($"Player 1 has {PlayerHands[0].Count} cards; Player 2 has {PlayerHands[1].Count} cards.");
        if (IsGameOver())
            DLog.Log(PlayerHands[0].Count > PlayerHands[1].Count ? "Player 1 wins!" :
                     PlayerHands[1].Count > PlayerHands[0].Count ? "Player 2 wins!" : "It's a draw.");
    }
}
