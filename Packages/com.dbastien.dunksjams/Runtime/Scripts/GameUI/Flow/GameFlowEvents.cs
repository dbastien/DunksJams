public class GameStartedEvent : GameEvent { public string Title; }
public class GameEndedEvent : GameEvent { public string Title; }
public class GamePausedEvent : GameEvent { }
public class GameResumedEvent : GameEvent { }
public class ScoreChangedEvent : GameEvent { public string Id; public int Value; public int Delta; }
public class GameConfigChangedEvent : GameEvent { public string Key; public string Value; }
