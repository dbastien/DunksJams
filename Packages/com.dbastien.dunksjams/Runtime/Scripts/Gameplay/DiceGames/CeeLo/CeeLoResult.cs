using System;
using System.Text;

public readonly struct CeeLoResult : IComparable<CeeLoResult>
{
    public int[] Dice { get; }
    public CeeLoOutcome Outcome { get; }
    public int Point { get; }
    public bool IsScoring => Outcome != CeeLoOutcome.NoScore;

    public CeeLoResult(int[] dice, CeeLoOutcome outcome, int point)
    {
        Dice = dice ?? Array.Empty<int>();
        Outcome = outcome;
        Point = point;
    }

    public int Rank => Outcome switch
    {
        CeeLoOutcome.AutoWin => 800,
        CeeLoOutcome.Triple => 700 + Point,
        CeeLoOutcome.Point => 100 + Point,
        CeeLoOutcome.AutoLose => 0,
        _ => -1
    };

    public int CompareTo(CeeLoResult other) => Rank.CompareTo(other.Rank);

    public override string ToString()
    {
        var sb = new StringBuilder(24);
        AppendDice(sb);
        sb.Append(" (").Append(OutcomeLabel()).Append(')');
        return sb.ToString();
    }

    void AppendDice(StringBuilder sb)
    {
        if (Dice.Length == 0)
        {
            sb.Append("no dice");
            return;
        }

        sb.Append(Dice[0]);
        for (var i = 1; i < Dice.Length; i++) sb.Append('-').Append(Dice[i]);
    }

    string OutcomeLabel() => Outcome switch
    {
        CeeLoOutcome.AutoWin => "4-5-6",
        CeeLoOutcome.AutoLose => "1-2-3",
        CeeLoOutcome.Triple => $"triple {Point}",
        CeeLoOutcome.Point => $"point {Point}",
        _ => "no score"
    };
}