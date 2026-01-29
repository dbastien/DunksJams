using UnityEngine;

public class LogicGate3In : MonoBehaviour
{
    public Gate3InType gateType;

    public bool inA;
    public bool inB;
    public bool inC;
    
    public bool Evaluate() => LogicGates.Evaluate(gateType, inA, inB, inC);
}