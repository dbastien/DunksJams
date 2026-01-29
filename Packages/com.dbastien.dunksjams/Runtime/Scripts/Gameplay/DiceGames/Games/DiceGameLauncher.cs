public static class DiceGameLauncher
{
    public static void Main()
    {
        var io = new ConsoleCardGameIO();
        var options = new[]
        {
            "Cee-Lo"
        };

        int choice = io.ReadChoice("Select a dice game:", options);
        switch (choice)
        {
            default:
            {
                int players = io.ReadInt("Player count (2-6):", 2, 6, 2);
                int rounds = io.ReadInt("Rounds (1-10):", 1, 10, 1);
                using var game = new CeeLoGame(playerCount: players, rounds: rounds, io: io);
                game.RunGame();
                break;
            }
        }
    }
}
