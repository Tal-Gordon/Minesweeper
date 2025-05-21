using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class Grid_new : MonoBehaviour
{
    public int width;
    public int height;
    public static int[,] squares;
    public GameObject squarePrefab;

    const int FLAG = -2;
    const int MINE = -1;
    const int NOMINE = 0;

    void Start()
    {
        GenerateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateGrid()
    {
        squares = new int[width, height];
        List<(int, int)> mines = new();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                squares[i, j] = NOMINE;
                if (RandBool(0.2f)) // TODO: dont hardcode, should change according to difficulty
                {
                    squares[i, j] = MINE;
                    mines.Add((i, j));
                }
            }
        }

        foreach ((int, int) mine  in mines)
        {
            IncrementNumbersAroundMine(mine.Item1, mine.Item2);
        }
    }

    private void IncrementNumbersAroundMine(int x, int y)
    {
        // this feels stupid but i cant find anything better
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        for (int i = 0; i < dx.Length; i++)
        {
            int neighborX = x + dx[i];
            int neighborY = y + dy[i];

            if (IsMineAt(neighborX, neighborY))
            {
                squares[neighborX, neighborY]++;
            }
        }
    }

    private bool IsMineAt(int x, int y)
    {
        if (IsWithinGrid(x, y))
        {
            return (squares[x, y] == MINE);
        }
        return false;
    }

    private bool RandBool(float chance)
    {
        if (chance < 0 || chance > 1)
        {
            Debug.LogError($"Error: chance value is invalid: {chance}");
            return false;
        }

        return Random.Range(0f, 1f) < chance;
    }

    private bool IsWithinGrid(int x, int y)
    {
        return (x >= 0 && y >= 0 && x < width && y < height);
    }
}
