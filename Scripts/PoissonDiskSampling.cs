using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiskSampling
{
    public static List<Vector2> Generate(float xMin, float zMin, float width, float height, float minDistance, int numPoints, int maxSamples)
    {
        float cellSize = minDistance / Mathf.Sqrt(2);
        int gridWidth = Mathf.CeilToInt(width / cellSize);
        int gridHeight = Mathf.CeilToInt(height / cellSize);
        
        Vector2[,] grid = new Vector2[gridWidth, gridHeight];
        List<Vector2> points = new();
        List<Vector2> activeList = new();

        Vector2 firstPoint = new Vector2(
            Random.Range(xMin, xMin + width),
            Random.Range(zMin, zMin + height)
        );

        points.Add(firstPoint);
        activeList.Add(firstPoint);
        grid[(int)((firstPoint.x - xMin) / cellSize), (int)((firstPoint.y - zMin) / cellSize)] = firstPoint;

        while (activeList.Count > 0 && points.Count < numPoints)
        {
            int randomIndex = Random.Range(0, activeList.Count);
            Vector2 currentPoint = activeList[randomIndex];
            bool pointAdded = false;

            for (int i = 0; i < maxSamples; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float radius = Random.Range(minDistance, 2 * minDistance);
                Vector2 newPoint = currentPoint + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                if (newPoint.x >= xMin && newPoint.x < xMin + width &&
                    newPoint.y >= zMin && newPoint.y < zMin + height &&
                    IsValid(newPoint, grid, xMin, zMin, cellSize, minDistance))
                {
                    points.Add(newPoint);
                    activeList.Add(newPoint);
                    grid[(int)((newPoint.x - xMin) / cellSize), (int)((newPoint.y - zMin) / cellSize)] = newPoint;
                    pointAdded = true;
                    break;
                }
            }

            if (!pointAdded)
            {
                activeList.RemoveAt(randomIndex);
            }
        }

        return points;
    }

    private static bool IsValid(Vector2 point, Vector2[,] grid, float xMin, float zMin, float cellSize, float minDistance)
    {
        int gridX = (int)((point.x - xMin) / cellSize);
        int gridY = (int)((point.y - zMin) / cellSize);
        int searchRadius = 2; // Check within a 2-cell radius

        for (int x = Mathf.Max(0, gridX - searchRadius); x < Mathf.Min(grid.GetLength(0), gridX + searchRadius); x++)
        {
            for (int y = Mathf.Max(0, gridY - searchRadius); y < Mathf.Min(grid.GetLength(1), gridY + searchRadius); y++)
            {
                if (grid[x, y] != Vector2.zero && (grid[x, y] - point).sqrMagnitude < minDistance * minDistance)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
