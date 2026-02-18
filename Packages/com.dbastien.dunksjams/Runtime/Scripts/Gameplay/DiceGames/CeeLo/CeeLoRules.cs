using System;

public static class CeeLoRules
{
    public const int DicePerRoll = 3;
    public const int DefaultMaxRolls = 100;

    public static CeeLoResult RollOnce()
    {
        int[] dice = new[]
        {
            Dice.Roll(DieType.D6),
            Dice.Roll(DieType.D6),
            Dice.Roll(DieType.D6)
        };

        return Evaluate(dice);
    }

    public static CeeLoResult RollScoring(int maxRolls = DefaultMaxRolls)
    {
        maxRolls = Math.Max(1, maxRolls);

        CeeLoResult last = default;
        for (var i = 0; i < maxRolls; i++)
        {
            last = RollOnce();
            if (last.IsScoring) return last;
        }

        DLog.LogW($"Cee-Lo roll exceeded {maxRolls} attempts without a score.");
        return last;
    }

    public static CeeLoResult Evaluate(int d1, int d2, int d3) =>
        Evaluate(new[] { d1, d2, d3 });

    public static CeeLoResult Evaluate(int[] dice)
    {
        if (dice == null || dice.Length != DicePerRoll)
        {
            DLog.LogE("Cee-Lo requires exactly three dice.");
            throw new ArgumentException("Cee-Lo requires exactly three dice.", nameof(dice));
        }

        int[] sorted = new[] { dice[0], dice[1], dice[2] };
        Array.Sort(sorted);

        if (sorted[0] == 1 && sorted[1] == 2 && sorted[2] == 3)
            return new CeeLoResult(sorted, CeeLoOutcome.AutoLose, 0);

        if (sorted[0] == 4 && sorted[1] == 5 && sorted[2] == 6)
            return new CeeLoResult(sorted, CeeLoOutcome.AutoWin, 0);

        if (sorted[0] == sorted[2])
            return new CeeLoResult(sorted, CeeLoOutcome.Triple, sorted[0]);

        if (sorted[0] == sorted[1])
            return new CeeLoResult(sorted, CeeLoOutcome.Point, sorted[2]);

        if (sorted[1] == sorted[2])
            return new CeeLoResult(sorted, CeeLoOutcome.Point, sorted[0]);

        return new CeeLoResult(sorted, CeeLoOutcome.NoScore, 0);
    }
}