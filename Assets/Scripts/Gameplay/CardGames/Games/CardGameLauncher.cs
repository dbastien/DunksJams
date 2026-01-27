public static class CardGameLauncher
{
    public static void Main()
    {
        var io = new ConsoleCardGameIO();
        var options = new[]
        {
            "War",
            "Uno",
            "Poker (5-Card Draw)",
            "Tarot Reading"
        };

        int choice = io.ReadChoice("Select a card game:", options, 0);
        switch (choice)
        {
            case 1:
            {
                int players = io.ReadInt("Player count (2-6):", 2, 6, 2);
                using var game = new UnoGame(playerCount: players, io: io);
                game.RunGame();
                break;
            }
            case 2:
            {
                int players = io.ReadInt("Player count (2-6):", 2, 6, 2);
                using var game = new PokerGame(playerCount: players, io: io);
                game.RunGame();
                break;
            }
            case 3:
            {
                using var game = new TarotGame(io: io);
                game.RunGame();
                break;
            }
            default:
            {
                using var game = new WarGame(io: io);
                game.RunGame();
                break;
            }
        }
    }
}
