public static class MathConsts
{
    public const float PosInf = float.PositiveInfinity;
    public const float NegInf = float.NegativeInfinity;

    // Precision/Epsilon constants
    public const float Epsilon_Rounding = 1e-2f;
    public const float Epsilon_Normal = 1e-3f;
    public const float Epsilon_Physics = 1e-3f;
    public const float Epsilon_Angle = 1e-3f;
    public const float Epsilon_Color = 1e-3f;
    public const float Epsilon_Scale = 1e-4f;
    public const float Epsilon_Graphics = 1e-4f;
    public const float Epsilon_Raycast = 1e-4f;
    public const float Epsilon_LowPrecision = 1e-5f;
    public const float Epsilon_Vector = 1e-6f;
    public const float Epsilon_Inverse = 1e-6f;
    public const float Epsilon_Float = 1e-6f;
    public const float Epsilon_HighPrecision = 1e-7f;
    public const float Epsilon_UltraPrecision = 1e-8f;
    public const float Epsilon_Nano = 1e-9f;
    public const float Epsilon_SmallestFloatDifference = 1.1920928955078125e-7f; // Updated to match float precision

    // Thresholds
    public const float Threshold_VectorAlignment = 0.999999f; // Improved to reflect near-parallel precision

    // Tau/Pi-related constants
    public const float Tau = 6.283185307179586f;
    public const float Tau_Sqrt = 2.5066282746310007f; // √Tau
    public const float Tau_Inverse = 0.15915494309189535f; // 1/Tau
    public const float Pi = 3.141592653589793f;

    // Fractional Tau
    public const float Tau_Div2 = 3.141592653589793f;
    public const float Tau_Div3 = 2.0943951023931953f;
    public const float Tau_Div4 = 1.5707963267948966f;
    public const float Tau_Div5 = 1.2566370614359173f;
    public const float Tau_Div6 = 1.0471975511965976f;
    public const float Tau_Div7 = 0.8975979010256552f;
    public const float Tau_Div8 = 0.7853981633974483f;
    public const float Tau_Div12 = 0.5235987755982988f;

    // Conversion factors
    public const float Conversion_Deg2Rad = 0.017453292519943295f; // π/180
    public const float Conversion_Rad2Deg = 57.29577951308232f; // 180/π

    // Math constants
    public const float E = 2.718281828459045f;
    public const float Log2E = 1.4426950408889634f;
    public const float Log10E = 0.4342944819032518f;
    public const float Sqrt2 = 1.4142135623730951f;
    public const float Sqrt3 = 1.7320508075688772f;

    // Inverse values
    public const float Inverse_255 = 0.00392156862745098f;
    public const float Inverse_64 = 0.015625f;
    public const float Inverse_32 = 0.03125f;
    public const float Inverse_16 = 0.0625f;
    public const float Inverse_8 = 0.125f;

    // Famous mathematical constants
    public const float EulerGamma = 0.5772156649015329f;
    public const float Catalan = 0.915965594177219f;
    public const float Feigenbaum_Delta = 4.66920160910299f;
    public const float Feigenbaum_Alpha = 2.502907875095892f;
    public const float Glaisher = 1.2824271291006226f;
    public const float Khinchin = 2.685452001065306f;
    public const float Lemniscate = 2.6220575542921198f;
    public const float Omega = 0.567143290409784f;
    public const float RamanujanSoldner = 1.451369234883381f;
    public const float Plastic = 1.324717957244746f;
    public const float Mertens = 0.2614972128476428f;
    public const float Niven = 1.7052111401053678f;
    public const float PorterKnobloch = 1.186569110415625f;
    public const float LiebSquareIce = 1.539600717839002f;
    public const float ErdosBorwein = 1.6066951524152918f;
    public const float FransenRobinson = 2.807770242028519f;
    public const float GoldenRatio = 1.618033988749895f;
    public const float SilverRatio = 2.414213562373095f;

    // Special aliases
    public const float ε = Epsilon_Float;
    public const float τ = Tau;
    public const float π = Pi;
    public const float φ = GoldenRatio;
    public const float δS = SilverRatio;
    public const float e = E;
    public const float γ = EulerGamma;
    public const float δ = Feigenbaum_Delta;
    public const float α = Feigenbaum_Alpha;
    public const float A = Glaisher;
    public const float K = Khinchin;
    public const float Ω = Omega;
    public const float R = RamanujanSoldner;
    public const float P = Plastic;
}