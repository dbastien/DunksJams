using System;

public class Histogram
{
    public int[] Bins;
    public double[] BinTotals;

    public float BinMin;
    public float BinMax;
    public float BinWidth;

    public Histogram(float min, float max, int numBins)
    {
        if (numBins <= 0 || max <= min) throw new ArgumentOutOfRangeException();

        BinMin = min;
        BinMax = max;

        Bins = new int[numBins];
        BinTotals = new double[numBins];

        BinWidth = (BinMax - BinMin) / numBins;
    }

    public void Clear()
    {
        Array.Clear(Bins, 0, Bins.Length);
        Array.Clear(BinTotals, 0, BinTotals.Length);
    }

    public bool AddSample(float value)
    {
        var binTarget = (int)((value - BinMin) / BinWidth);

        if (binTarget < 0 || binTarget >= Bins.Length) return false;

        Bins[binTarget]++;
        BinTotals[binTarget] += value;

        return true;
    }

    public float GetBinAverage(int bin)
    {
        if (bin < 0 || bin > Bins.Length - 1) throw new ArgumentOutOfRangeException();

        if (Bins[bin] == 0) return 0.0f;

        return (float)(BinTotals[bin] / Bins[bin]);
    }

    public float GetBinCenter(int bin)
    {
        if (bin < 0 || bin > Bins.Length - 1) throw new ArgumentOutOfRangeException();

        return BinMin + (bin + 0.5f) * BinWidth;
    }

    public float GetBinTop(int bin)
    {
        if (bin < 0 || bin > Bins.Length - 1) throw new ArgumentOutOfRangeException();

        return BinMin + (bin + 1.0f) * BinWidth;
    }

    public float GetBinBottom(int bin)
    {
        if (bin < 0 || bin > Bins.Length - 1) throw new ArgumentOutOfRangeException();

        return BinMin + bin * BinWidth;
    }

    public int GetFirstBinWithMinCount(int minCount)
    {
        for (var i = 0; i < Bins.Length; ++i)
        {
            if (Bins[i] > minCount)
                return i;
        }

        return -1;
    }

    public int GetLastBinWithMinCount(int minCount)
    {
        for (var i = Bins.Length - 1; i >= 0; --i)
        {
            if (Bins[i] > minCount)
                return i;
        }

        return -1;
    }

    public int GetHighestBin()
    {
        var maxValue = 0;
        var maxBin = 0;

        for (var i = 0; i < Bins.Length; ++i)
        {
            if (Bins[i] > maxValue)
            {
                maxValue = Bins[i];
                maxBin = i;
            }
        }

        return maxBin;
    }
}