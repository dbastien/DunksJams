public abstract class CardGameEvent : GameEvent
{
    public string GameId { get; set; }
}

public sealed class CardGameStartedEvent : CardGameEvent
{
    public int PlayerCount { get; set; }
    public string Variant { get; set; }
}

public sealed class CardGameEndedEvent : CardGameEvent
{
    public string ResultSummary { get; set; }
}

public abstract class CardGameTurnEvent : CardGameEvent
{
    public int TurnIndex { get; set; }
    public int PlayerIndex { get; set; }
}

public sealed class CardGameTurnStartedEvent : CardGameTurnEvent { }
public sealed class CardGameTurnEndedEvent : CardGameTurnEvent { }

public abstract class CardGameCardEvent : CardGameEvent
{
    public int PlayerIndex { get; set; }
    public CardBase Card { get; set; }
    public bool IsFaceDown { get; set; }
}

public sealed class CardGameCardDealtEvent : CardGameCardEvent { }
public sealed class CardGameCardDrawnEvent : CardGameCardEvent { }
public sealed class CardGameCardPlayedEvent : CardGameCardEvent { }
public sealed class CardGameCardDiscardedEvent : CardGameCardEvent { }

public sealed class CardGamePhaseChangedEvent : CardGameEvent
{
    public string Phase { get; set; }
}
