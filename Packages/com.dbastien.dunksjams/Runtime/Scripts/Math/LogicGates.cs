public enum Gate1InType
{
    BUFFER,
    NOT
}

public enum Gate2InType
{
    AND,
    OR,
    XOR,
    NAND,
    NOR,
    XNOR
}

public enum Gate3InType
{
    TMAJ,
    TMUX,
    TAND3,
    TOR3,
    TXOR3,
    TXNOR3,
    TNAND3,
    TNOR3,
    TIMP,
    TCOF
}

public static class LogicGates
{
    public static bool Evaluate(Gate1InType gateType, bool inA) =>
        gateType switch
        {
            Gate1InType.BUFFER => inA,
            Gate1InType.NOT => !inA,
            _ => false
        };

    public static bool Evaluate(Gate2InType gateType, bool inA, bool inB) =>
        gateType switch
        {
            Gate2InType.AND => inA && inB,
            Gate2InType.OR => inA || inB,
            Gate2InType.XOR => inA ^ inB,
            Gate2InType.NAND => !(inA && inB),
            Gate2InType.NOR => !(inA || inB),
            Gate2InType.XNOR => inA == inB,
            _ => false
        };

    public static bool Evaluate(Gate3InType gateType, bool inA, bool inB, bool inC) =>
        gateType switch
        {
            Gate3InType.TAND3 => inA && inB && inC,
            Gate3InType.TOR3 => inA || inB || inC,
            Gate3InType.TNAND3 => !(inA && inB && inC),
            Gate3InType.TNOR3 => !(inA || inB || inC),

            // TXOR3: true if an odd number of inputs are true
            Gate3InType.TXOR3 => inA ^ inB ^ inC,

            // TXNOR3: true if an even number of inputs are true
            Gate3InType.TXNOR3 => !(inA ^ inB ^ inC),

            // TMAJ: True if at least 2 inputs are true
            Gate3InType.TMAJ => (inA && inB) || (inA && inC) || (inB && inC),

            // TMUX: If inputA is true, select inputC; otherwise, select inputB.
            Gate3InType.TMUX => inA ? inC : inB,

            // TIMP: Implication - If inA implies inB, or inC is true
            Gate3InType.TIMP => !(inA && !inB) || inC,

            // TCOF: Co-factor - True if (inA && !inB) or (!inA && inC)
            Gate3InType.TCOF => (inA && !inB) || (!inA && inC),

            _ => false
        };
}