using System.Collections.Generic;

public class TarotGame : CardGameBase<TarotCard>
{
    private readonly TarotReadingSettings _settings;
    private bool _hasRead;

    public TarotGame(TarotReadingSettings settings = null, ICardGameIO io = null)
        : base(1, 1, io)
    {
        _settings = settings ?? new TarotReadingSettings();
        VariantName = _settings.Spread.ToString();
    }

    protected override Deck<TarotCard> CreateDeck() => TarotDeck.CreateDeck();

    protected override void Setup()
    {
        Deck = CreateDeck();
        Deck.Shuffle();
        PlayerHands = new List<Hand<TarotCard>>(1) { new() };

        if (_settings.PromptForSpread) PromptForSpread();
        VariantName = _settings.Spread.ToString();
        DLog.Log("Tarot reading setup complete.");
    }

    protected override void DistributeDeck(Deck<TarotCard> deck)
    {
        // Tarot reading draws directly from the deck.
    }

    public override void PlayTurn(int playerIdx)
    {
        if (_hasRead) return;

        EmitPhaseChanged($"Reading:{_settings.Spread}");
        if (_settings.Spread == TarotReadingSettings.TarotSpread.SingleCard) ReadSingleCard();
        else ReadThreeCard();

        _hasRead = true;
    }

    public override bool IsGameOver() => _hasRead;

    public override void ShowScores()
    {
        if (_hasRead) WriteLine("Reading complete.");
    }

    protected override string GetResultSummary() => _hasRead ? "Reading complete." : null;

    private void PromptForSpread()
    {
        var options = new List<string> { "Single Card", "Three Card (Past/Present/Future)" };
        int choice = ReadChoice("Choose a tarot spread:", options, (int)_settings.Spread);
        _settings.Spread = choice == 0
            ? TarotReadingSettings.TarotSpread.SingleCard
            : TarotReadingSettings.TarotSpread.ThreeCard;
    }

    private void ReadSingleCard()
    {
        TarotCard card = DrawReadingCard();
        WriteLine($"Single card draw: {card}");
    }

    private void ReadThreeCard()
    {
        TarotCard past = DrawReadingCard();
        TarotCard present = DrawReadingCard();
        TarotCard future = DrawReadingCard();
        WriteLine("Three card spread:");
        WriteLine($"Past: {past}");
        WriteLine($"Present: {present}");
        WriteLine($"Future: {future}");
    }

    private TarotCard DrawReadingCard()
    {
        TarotCard card = Deck.Draw();
        EmitCardDrawn(-1, card, false);
        DiscardCard(card, -1);
        return card;
    }
}