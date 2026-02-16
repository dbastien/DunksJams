using System;
using System.Collections.Generic;
using UnityEngine;

public static class PaletteExtraction
{
    const int DefaultIterations = 5;
    const int MaxSampleCount = 8192;

    public static Color[] ExtractColors(Texture2D tex, int count)
    {
        if (!tex) return Array.Empty<Color>();
        var pixels = tex.GetPixels();
        if (pixels == null || pixels.Length == 0) return Array.Empty<Color>();

        if (pixels.Length > MaxSampleCount)
        {
            var sample = new Color[MaxSampleCount];
            for (var i = 0; i < MaxSampleCount; i++)
                sample[i] = pixels[UnityEngine.Random.Range(0, pixels.Length)];
            pixels = sample;
        }

        count = Mathf.Clamp(count, 2, 128);
        var centroids = new Color[count];
        for (var i = 0; i < count; i++)
            centroids[i] = pixels[UnityEngine.Random.Range(0, pixels.Length)];

        for (var iter = 0; iter < DefaultIterations; iter++)
        {
            var clusters = new List<Color>[count];
            for (var i = 0; i < count; i++)
                clusters[i] = new List<Color>();

            foreach (var p in pixels)
            {
                var minDist = float.MaxValue;
                var closest = 0;
                for (var i = 0; i < count; i++)
                {
                    var d = ColorTheory.DeltaE(p, centroids[i]);
                    if (d < minDist)
                    {
                        minDist = d;
                        closest = i;
                    }
                }
                clusters[closest].Add(p);
            }

            for (var i = 0; i < count; i++)
            {
                if (clusters[i].Count == 0) continue;
                var sum = Color.black;
                foreach (var c in clusters[i]) sum += c;
                centroids[i] = sum / clusters[i].Count;
            }
        }

        return centroids;
    }
}
