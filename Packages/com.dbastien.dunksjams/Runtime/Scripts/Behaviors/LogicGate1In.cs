using UnityEngine;

public class LogicGate1In : MonoBehaviour
{
    public Gate1InType gateType;

    public bool inA;

    public bool Evaluate() => LogicGates.Evaluate(gateType, inA);
}