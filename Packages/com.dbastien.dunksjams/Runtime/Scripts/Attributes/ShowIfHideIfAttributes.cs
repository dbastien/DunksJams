using UnityEngine;

public abstract class ConditionalAttribute : PropertyAttribute
{
    public string ConditionField { get; }
    protected ConditionalAttribute(string conditionField) => ConditionField = conditionField;
}

public class HideIfAttribute : ConditionalAttribute
{
    public HideIfAttribute(string conditionField) : base(conditionField)
    {
    }
}

public class ShowIfAttribute : ConditionalAttribute
{
    public ShowIfAttribute(string conditionField) : base(conditionField)
    {
    }
}