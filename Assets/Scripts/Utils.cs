using System;
using UnityEngine;

public static class Utils
{
    public const int MINE = -1;
    public const int EMPTY = 0;

    public static bool RandBool(float chance)
    {
        if (chance < 0 || chance > 1)
        {
            Debug.LogError($"Error: chance value is invalid: {chance}");
            return false;
        }

        return UnityEngine.Random.Range(0f, 1f) < chance;
    }

    public static void ForEachNeighbor(int x, int y, int width, int height, Action<int, int> action)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (IsWithinGrid(nx, ny, width, height))
                {
                    action(nx, ny);
                }
            }
        }
    }

    public static bool IsWithinGrid(int x, int y, int width, int height)
    {
        return (x >= 0 && y >= 0 && x < width && y < height);
    }
}
