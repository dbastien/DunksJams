public static class FIRFilters
{
    //https://www.wikiwand.com/en/Low-pass_filter
    //y[i] := y[i-1] + α * (x[i] - y[i-1])
    public static float LowPass(float sample, float previousOutput, float smoothing)
    {
        return previousOutput + smoothing * (sample - previousOutput);
    }

    //https://www.wikiwand.com/en/High-pass_filter
    //y[i] := α * (y[i-1] + x[i] - x[i-1])
    public static float HighPass(float sample, float previousOutput, float previousInput, float smoothing)
    {
        return smoothing * (previousOutput + sample - previousInput);
    }

    //band pass
    //band stop
}
