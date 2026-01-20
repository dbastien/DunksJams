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
        if (numBins <= 0 || max <= min)
        {
            throw new ArgumentOutOfRangeException();
        }

        this.BinMin = min;
        this.BinMax = max;

        this.Bins = new int[numBins];
        this.BinTotals = new double[numBins];

        this.BinWidth = (this.BinMax - this.BinMin) / numBins;
    }

    public void Clear()
    {
        Array.Clear(this.Bins, 0, this.Bins.Length);
        Array.Clear(this.BinTotals, 0, this.BinTotals.Length);
    }

    public bool AddSample(float value)
    {
        int binTarget = (int)((value - this.BinMin) / this.BinWidth);

        if (binTarget < 0 || binTarget >= this.Bins.Length)
        {
            return false;
        }

        this.Bins[binTarget]++;
        this.BinTotals[binTarget] += value;

        return true;
    }

    public float GetBinAverage(int bin)
    {
        if ((bin < 0) || (bin > this.Bins.Length - 1))
        {
            throw new ArgumentOutOfRangeException();
        }

        if (this.Bins[bin] == 0)
        {
            return 0.0f;
        }

        return (float)(this.BinTotals[bin] / this.Bins[bin]);
    }

    public float GetBinCenter(int bin)
    {
        if ((bin < 0) || (bin > this.Bins.Length - 1))
        {
            throw new ArgumentOutOfRangeException();
        }

        return this.BinMin + (bin + 0.5f) * this.BinWidth;
    }

    public float GetBinTop(int bin)
    {
        if ((bin < 0) || (bin > this.Bins.Length - 1))
        {
            throw new ArgumentOutOfRangeException();
        }

        return this.BinMin + (bin + 1.0f) * this.BinWidth;
    }

    public float GetBinBottom(int bin)
    {
        if ((bin < 0) || (bin > this.Bins.Length - 1))
        {
            throw new ArgumentOutOfRangeException();
        }

        return this.BinMin + bin * this.BinWidth;
    }

    public int GetFirstBinWithMinCount(int minCount)
    {
        for (var i = 0; i < this.Bins.Length; ++i)
        {
            if (this.Bins[i] > minCount)
            {
                return i;
            }
        }

        return -1;
    }

    public int GetLastBinWithMinCount(int minCount)
    {
        for (var i = this.Bins.Length - 1; i >= 0; --i)
        {
            if (this.Bins[i] > minCount)
            {
                return i;
            }
        }

        return -1;
    }

    public int GetHighestBin()
    {
        int maxValue = 0;
        int maxBin = 0;

        for (int i = 0; i < this.Bins.Length; ++i)
        {
            if (this.Bins[i] > maxValue)
            {
                maxValue = this.Bins[i];
                maxBin = i;
            }
        }

        return maxBin;
    }
}
