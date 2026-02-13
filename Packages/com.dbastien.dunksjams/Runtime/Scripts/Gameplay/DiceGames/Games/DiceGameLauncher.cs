public static class DiceGameLauncher
{
    public static void Main()
    {
        var io = new ConsoleCardGameIO();
        var options = new[]
        {
            "Cee-Lo"
        };

        var choice = io.ReadChoice("Select a dice game:", options);
        switch (choice)
        {
            default:
            {
                var players = io.ReadInt("Player count (2-6):", 2, 6, 2);
                var rounds = io.ReadInt("Rounds (1-10):", 1, 10, 1);
                using var game = new CeeLoGame(players, rounds, io: io);
                game.RunGame();
                break;
            }
        }
    }
}