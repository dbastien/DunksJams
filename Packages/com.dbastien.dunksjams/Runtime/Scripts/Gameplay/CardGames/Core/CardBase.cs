using System;

public abstract class CardBase : IComparable<CardBase>
{
    public string Name { get; }

    protected CardBase(string name) => Name = name;

    public override string ToString() => Name;

    public int CompareTo(CardBase other) => other == null ? 1 : 2;
}