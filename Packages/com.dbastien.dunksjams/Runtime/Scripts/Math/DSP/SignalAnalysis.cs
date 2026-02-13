using System;

public static class SignalAnalysis
{
    /// <summary>Calculate RMS (Root Mean Square) of a signal</summary>
    public static float RMS(float[] samples)
    {
        if (samples.Length == 0) return 0f;

        var sum = 0f;
        for (var i = 0; i < samples.Length; i++) sum += samples[i] * samples[i];

        return MathF.Sqrt(sum / samples.Length);
    }

    /// <summary>Find the peak value in a signal</summary>
    public static float Peak(float[] samples)
    {
        if (samples.Length == 0) return 0f;

        var peak = 0f;
        for (var i = 0; i < samples.Length; i++)
        {
            var abs = MathF.Abs(samples[i]);
            if (abs > peak) peak = abs;
        }

        return peak;
    }

    /// <summary>Count zero crossings in a signal</summary>
    public static int ZeroCrossings(float[] samples)
    {
        if (samples.Length < 2) return 0;

        var crossings = 0;
        var wasPositive = samples[0] >= 0;

        for (var i = 1; i < samples.Length; i++)
        {
            var isPositive = samples[i] >= 0;
            if (wasPositive != isPositive)
            {
                crossings++;
                wasPositive = isPositive;
            }
        }

        return crossings;
    }

    /// <summary>Calculate the centroid (center of mass) of a signal</summary>
    public static float Centroid(float[] samples)
    {
        if (samples.Length == 0) return 0f;

        var numerator = 0f;
        var denominator = 0f;

        for (var i = 0; i < samples.Length; i++)
        {
            var magnitude = MathF.Abs(samples[i]);
            numerator += i * magnitude;
            denominator += magnitude;
        }

        return denominator > 0 ? numerator / denominator : 0f;
    }

    /// <summary>Calculate spectral centroid (requires FFT data)</summary>
    public static float SpectralCentroid(float[] magnitudes, float sampleRate)
    {
        if (magnitudes.Length == 0) return 0f;

        var numerator = 0f;
        var denominator = 0f;

        for (var i = 0; i < magnitudes.Length; i++)
        {
            var frequency = i * sampleRate / (2f * magnitudes.Length);
            numerator += frequency * magnitudes[i];
            denominator += magnitudes[i];
        }

        return denominator > 0 ? numerator / denominator : 0f;
    }
}