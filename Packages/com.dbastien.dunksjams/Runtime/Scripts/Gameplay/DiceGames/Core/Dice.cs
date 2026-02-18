using System;

public static class Dice
{
    public static readonly DieType[] StandardDice =
    {
        DieType.D4,
        DieType.D6,
        DieType.D8,
        DieType.D12,
        DieType.D16,
        DieType.D20,
        DieType.D100
    };

    public static int Roll(DieType die) => Roll((int)die);

    public static int Roll(int sides)
    {
        ValidateSides(sides);
        return Rand.IntRanged(1, sides + 1);
    }

    public static DiceRoll RollDetailed(DieType die, int count = 1, int modifier = 0, bool log = false) =>
        RollDetailed((int)die, count, modifier, log);

    public static DiceRoll RollDetailed(int sides, int count = 1, int modifier = 0, bool log = false)
    {
        ValidateSides(sides);
        ValidateCount(count);

        var rolls = new int[count];
        var sum = 0;

        for (var i = 0; i < count; i++)
        {
            int roll = Rand.IntRanged(1, sides + 1);
            rolls[i] = roll;
            sum += roll;
        }

        var result = new DiceRoll(sides, count, modifier, rolls, sum);
        if (log) result.Log();
        return result;
    }

    public static int RollTotal(DieType die, int count = 1, int modifier = 0, bool log = false) =>
        RollTotal((int)die, count, modifier, log);

    public static int RollTotal(int sides, int count = 1, int modifier = 0, bool log = false)
    {
        if (log) return RollDetailed(sides, count, modifier, true).Total;

        ValidateSides(sides);
        ValidateCount(count);

        var sum = 0;
        for (var i = 0; i < count; i++)
            sum += Rand.IntRanged(1, sides + 1);

        return sum + modifier;
    }

    public static bool IsStandardSides(int sides)
    {
        for (var i = 0; i < StandardDice.Length; i++)
            if ((int)StandardDice[i] == sides)
                return true;

        return false;
    }

    private static void ValidateSides(int sides)
    {
        if (sides >= 2) return;
        DLog.LogE($"Dice requires at least 2 sides (got {sides}).");
        throw new ArgumentOutOfRangeException(nameof(sides), sides, "Sides must be >= 2.");
    }

    private static void ValidateCount(int count)
    {
        if (count >= 1) return;
        DLog.LogE($"Dice roll count must be >= 1 (got {count}).");
        throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be >= 1.");
    }
}