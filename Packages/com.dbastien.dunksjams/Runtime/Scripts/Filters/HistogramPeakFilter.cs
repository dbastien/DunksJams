using UnityEngine;

public class HistogramPeakFilter : IFilter1D
{
    // 5 minutes worth of samples at 30 fps
    const int MaxSamples = 9000;

    int[] bins;
    int numSamples;

    int peakBin;

    float lowerBound;
    float upperBound;
    float binSize;

    // Get the value associated with the bin associated with the most samples
    public float CurrentValue =>
        // + 0.5f is to put us at the center of the range that the bin represents.
        lowerBound + (peakBin + 0.5f) * binSize;

    // Constructor
    public HistogramPeakFilter(float lowerBound, float upperBound, int binCount)
    {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;

        binSize = (upperBound - lowerBound) / binCount;

        bins = new int[binCount];

        Reset();
    }

    // Add a sample to the filter 
    public void Update(float s)
    {
        if (numSamples < MaxSamples && s >= lowerBound && s <= upperBound)
        {
            var binAccumulate = (int)((s - lowerBound) / binSize);

            //should handle: binAccumulate could == m_cBins 
            if (binAccumulate < bins.Length && binAccumulate >= 0)
            {
                ++bins[binAccumulate];
                ++numSamples;
            }
        }

        var peakBin = bins.Length / 2;
        var peakValue = 0;

        for (var i = 0; i < bins.Length; ++i)
        {
            if (peakValue < bins[i])
            {
                peakBin = i;
                peakValue = bins[i];
            }
        }

        this.peakBin = peakBin;
    }

    // Clear all samples from the histogram 
    public void Reset()
    {
        for (var i = 0; i < bins.Length; ++i) bins[i] = 0;

        numSamples = 0;
        peakBin = bins.Length / 2;
    }
}