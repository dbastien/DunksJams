using UnityEngine;

public class LogicGate2In : MonoBehaviour
{
    public Gate2InType gateType;

    public bool inA;
    public bool inB;

    public bool Evaluate() => LogicGates.Evaluate(gateType, inA, inB);
}