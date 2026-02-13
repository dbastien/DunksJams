using System;

public class EnvelopeDetector
{
    float attackCoeff;
    float releaseCoeff;
    float envelope;

    public EnvelopeDetector(float attackTimeMs, float releaseTimeMs, float sampleRate)
    {
        // Convert time constants to coefficients
        // attackCoeff = 1 - exp(-1/(attackTimeMs * sampleRate / 1000))
        attackCoeff = 1f - MathF.Exp(-1f / (attackTimeMs * sampleRate / 1000f));
        releaseCoeff = 1f - MathF.Exp(-1f / (releaseTimeMs * sampleRate / 1000f));
        envelope = 0f;
    }

    public float Process(float input)
    {
        var absInput = MathF.Abs(input);

        if (absInput > envelope)
            envelope += attackCoeff * (absInput - envelope);
        else
            envelope += releaseCoeff * (absInput - envelope);

        return envelope;
    }

    public void Reset() => envelope = 0f;

    public float CurrentEnvelope => envelope;
}

public static class EnvelopeDetection
{
    public static float[] PeakEnvelope(float[] samples, int windowSize = 1)
    {
        var envelope = new float[samples.Length];

        for (var i = 0; i < samples.Length; i++)
        {
            var peak = 0f;
            var start = Math.Max(0, i - windowSize);
            var end = Math.Min(samples.Length, i + windowSize + 1);

            for (var j = start; j < end; j++) peak = MathF.Max(peak, MathF.Abs(samples[j]));

            envelope[i] = peak;
        }

        return envelope;
    }

    public static float[] RMSEnvelope(float[] samples, int windowSize = 32)
    {
        var envelope = new float[samples.Length];

        for (var i = 0; i < samples.Length; i++)
        {
            var sum = 0f;
            var count = 0;
            var start = Math.Max(0, i - windowSize / 2);
            var end = Math.Min(samples.Length, i + windowSize / 2);

            for (var j = start; j < end; j++)
            {
                sum += samples[j] * samples[j];
                count++;
            }

            envelope[i] = count > 0 ? MathF.Sqrt(sum / count) : 0f;
        }

        return envelope;
    }
}