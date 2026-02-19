using UnityEngine;

public abstract class InspectorConditionaPropertyAttribute : PropertyAttribute
{
    public string ConditionField { get; }
    protected InspectorConditionaPropertyAttribute(string conditionField) => ConditionField = conditionField;
}

public class HideIfAttribute : InspectorConditionaPropertyAttribute
{
    public HideIfAttribute(string conditionField) : base(conditionField) { }
}

public class ShowIfAttribute : InspectorConditionaPropertyAttribute
{
    public ShowIfAttribute(string conditionField) : base(conditionField) { }
}