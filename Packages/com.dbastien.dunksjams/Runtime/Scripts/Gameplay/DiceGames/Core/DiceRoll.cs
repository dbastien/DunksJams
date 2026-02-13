using System;
using System.Text;
using Object = UnityEngine.Object;

public readonly struct DiceRoll
{
    public int Sides { get; }
    public int Count { get; }
    public int Modifier { get; }
    public int[] Rolls { get; }
    public int Sum { get; }
    public int Total => Sum + Modifier;

    public DiceRoll(int sides, int count, int modifier, int[] rolls, int sum)
    {
        Sides = sides;
        Count = count;
        Modifier = modifier;
        Rolls = rolls ?? Array.Empty<int>();
        Sum = sum;
    }

    public string Notation
    {
        get
        {
            var sb = new StringBuilder(16);
            sb.Append(Count).Append('d').Append(Sides);
            if (Modifier > 0) sb.Append('+').Append(Modifier);
            else if (Modifier < 0) sb.Append(Modifier);
            return sb.ToString();
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder(32);
        sb.Append(Notation).Append(" = ").Append(Total);
        if (Rolls.Length > 0)
        {
            sb.Append(" [");
            for (var i = 0; i < Rolls.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(Rolls[i]);
            }

            sb.Append(']');
        }

        return sb.ToString();
    }

    public void Log(Object ctx = null) => DLog.Log(ToString(), ctx);
}