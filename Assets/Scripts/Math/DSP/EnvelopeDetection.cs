using System;

public class EnvelopeDetector
{
    private float attackCoeff;
    private float releaseCoeff;
    private float envelope;

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
        float absInput = MathF.Abs(input);

        if (absInput > envelope)
        {
            envelope += attackCoeff * (absInput - envelope);
        }
        else
        {
            envelope += releaseCoeff * (absInput - envelope);
        }

        return envelope;
    }

    public void Reset() => envelope = 0f;

    public float CurrentEnvelope => envelope;
}

public static class EnvelopeDetection
{
    public static float[] PeakEnvelope(float[] samples, int windowSize = 1)
    {
        float[] envelope = new float[samples.Length];

        for (int i = 0; i < samples.Length; i++)
        {
            float peak = 0f;
            int start = Math.Max(0, i - windowSize);
            int end = Math.Min(samples.Length, i + windowSize + 1);

            for (int j = start; j < end; j++)
            {
                peak = MathF.Max(peak, MathF.Abs(samples[j]));
            }

            envelope[i] = peak;
        }

        return envelope;
    }

    public static float[] RMSEnvelope(float[] samples, int windowSize = 32)
    {
        float[] envelope = new float[samples.Length];

        for (int i = 0; i < samples.Length; i++)
        {
            float sum = 0f;
            int count = 0;
            int start = Math.Max(0, i - windowSize / 2);
            int end = Math.Min(samples.Length, i + windowSize / 2);

            for (int j = start; j < end; j++)
            {
                sum += samples[j] * samples[j];
                count++;
            }

            envelope[i] = count > 0 ? MathF.Sqrt(sum / count) : 0f;
        }

        return envelope;
    }
}