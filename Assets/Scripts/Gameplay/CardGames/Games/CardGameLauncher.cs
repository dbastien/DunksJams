public static class CardGameLauncher
{
    public static void Main()
    {
        var game = new WarGame();
        using (game) game.RunGame();
    }
}
