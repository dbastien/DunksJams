using System;
using System.Collections.Generic;
using System.Text;

public sealed class CeeLoGame : IDisposable
{
    private readonly int _playerCount;
    private readonly int _rounds;
    private readonly int _maxRollsPerTurn;
    private readonly ICardGameIO _io;

    public CeeLoGame
    (
        int playerCount = 2, int rounds = 1, int maxRollsPerTurn = CeeLoRules.DefaultMaxRolls,
        ICardGameIO io = null
    )
    {
        if (playerCount < 2)
        {
            DLog.LogE($"Cee-Lo requires at least 2 players (got {playerCount}).");
            throw new ArgumentOutOfRangeException(nameof(playerCount), playerCount, "Player count must be >= 2.");
        }

        if (rounds < 1)
        {
            DLog.LogE($"Cee-Lo requires at least 1 round (got {rounds}).");
            throw new ArgumentOutOfRangeException(nameof(rounds), rounds, "Rounds must be >= 1.");
        }

        _playerCount = playerCount;
        _rounds = rounds;
        _maxRollsPerTurn = Math.Max(1, maxRollsPerTurn);
        _io = io ?? CardGameIO.Default;
    }

    public void RunGame()
    {
        WriteLine($"Starting Cee-Lo with {_playerCount} players.");
        var wins = new int[_playerCount];

        for (var round = 1; round <= _rounds; ++round)
        {
            int winner = PlayRound(round);
            wins[winner]++;
            WriteLine($"Round {round} winner: {GetPlayerName(winner)}.");
        }

        WriteLine("Final results:");
        for (var i = 0; i < wins.Length; ++i)
            WriteLine($"{GetPlayerName(i)} won {wins[i]} round(s).");
    }

    public int PlayRound(int roundIndex)
    {
        var contenders = new List<int>(_playerCount);
        for (var i = 0; i < _playerCount; ++i) contenders.Add(i);

        var results = new CeeLoResult[_playerCount];
        var rollOff = 1;

        while (true)
        {
            WriteLine($"Round {roundIndex} roll-off {rollOff}:");
            RollForPlayers(contenders, results);

            var bestRank = int.MinValue;
            var tied = new List<int>();

            for (var i = 0; i < contenders.Count; ++i)
            {
                int playerIndex = contenders[i];
                int rank = results[playerIndex].Rank;
                if (rank > bestRank)
                {
                    bestRank = rank;
                    tied.Clear();
                    tied.Add(playerIndex);
                }
                else if (rank == bestRank) { tied.Add(playerIndex); }
            }

            if (tied.Count == 1)
                return tied[0];

            WriteLine($"Tie between {FormatPlayers(tied)}. Rerolling...");
            contenders = tied;
            rollOff++;
        }
    }

    private void RollForPlayers(List<int> players, CeeLoResult[] results)
    {
        for (var i = 0; i < players.Count; ++i)
        {
            int playerIndex = players[i];
            CeeLoResult result = CeeLoRules.RollScoring(_maxRollsPerTurn);
            results[playerIndex] = result;
            WriteLine($"{GetPlayerName(playerIndex)} rolled {result}.");
        }
    }

    private string GetPlayerName(int index) => $"Player {index + 1}";

    private string FormatPlayers(IReadOnlyList<int> players)
    {
        var sb = new StringBuilder(32);
        for (var i = 0; i < players.Count; ++i)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(GetPlayerName(players[i]));
        }

        return sb.ToString();
    }

    private void WriteLine(string message) => _io?.WriteLine(message);

    public void Dispose() => DLog.Log("Cee-Lo game disposed.");
}