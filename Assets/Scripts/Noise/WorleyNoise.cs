using UnityEngine;

public static class WorleyNoise
{
    static readonly Vector2[,] precomputedOffsets;

    static WorleyNoise()
    {
        int seed = 42; // Example seed
        Random.InitState(seed);
        precomputedOffsets = new Vector2[3, 3];
        for (int i = 0; i < 3; ++i)
        for (int j = 0; j < 3; ++j)
            precomputedOffsets[i, j] = new(Random.value, Random.value);
    }

    public static float Worley2D(float x, float y, int cells = 5)
    {
        float minDistSq = float.MaxValue;
        float cellSize = 1f / cells;
        int px = Mathf.FloorToInt(x * cells), py = Mathf.FloorToInt(y * cells);

        for (int i = -1; i <= 1; ++i)
        {
            for (int j = -1; j <= 1; ++j)
            {
                int xi = (px + i + cells) % cells;
                int yi = (py + j + cells) % cells;

                float cx = (px + i + precomputedOffsets[xi % 3, yi % 3].x) * cellSize;
                float cy = (py + j + precomputedOffsets[xi % 3, yi % 3].y) * cellSize;

                float dx = cx - x, dy = cy - y;
                float distSq = dx * dx + dy * dy;
                minDistSq = Mathf.Min(minDistSq, distSq);
            }
        }
        return Mathf.Sqrt(minDistSq);
    }
}