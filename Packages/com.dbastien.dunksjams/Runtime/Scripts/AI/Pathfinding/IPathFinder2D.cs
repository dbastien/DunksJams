using System.Collections.Generic;
using UnityEngine;

public interface IPathFinder2D
{
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, int[,] grid, bool allowDiag = false);
    public void UpdateObstacle(Vector2Int pos, bool isObstacle);
}