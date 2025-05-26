using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Utils;

public class GameManager : MonoBehaviour
{
    public GameObject squarePrefab;

    public int width;
    public int height;
    public int numOfMines = 80;

    private Camera mainCamera;
    private Square_new[,] gridGO;

    private int[,] truthGrid;
    private List<int2> emptyBeginningSquares;
    private bool isFirstSquareOpened = false;
    private readonly float squareSize = 1f;
    private readonly float margin = 1f;

    public int[,] TruthGrid { get => truthGrid; private set => truthGrid = value; }

    void Start()
    {
        mainCamera = Camera.main;
        GenerateGameGrid();
        CenterCamera();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) 
        //{ 
        //    OpenGrid();
        //}
    }

    public void OpenGrid()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                gridGO[i, j].RevealSquare();
            }
        }
    }

    private void GenerateTruthGrid()
    {
        TruthGrid = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TruthGrid[x, y] = EMPTY;
            }
        }

        List<int2> mines = new(numOfMines);
        var rand = new System.Random();

        while (mines.Count < numOfMines)
        {
            int x = rand.Next(0, width);
            int y = rand.Next(0, height);
            var p = new int2(x, y);

            // skip duplicates or first-click safe area
            if (TruthGrid[x, y] == MINE) continue;
            if (emptyBeginningSquares.Contains(p)) continue;

            TruthGrid[x, y] = MINE;
            mines.Add(p);
        }

        foreach (int2 mine in mines)
        {
            IncrementNumbersAroundMine(mine.x, mine.y);
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                gridGO[i, j].InitMines();
            }
        }
    }

    public void SetEmptyBeginningSquaresAndTruth(int x, int y)
    {
        if (isFirstSquareOpened) { return; }

        emptyBeginningSquares = new()
        {
            // immediately add x, y
            new int2(x, y)
        };

        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        for (int i = 0; i < dx.Length; i++)
        {
            int neighborX = x + dx[i];
            int neighborY = y + dy[i];

            if (IsWithinGrid(neighborX, neighborY))
            {
                emptyBeginningSquares.Add(new int2(neighborX, neighborY));
            }
        }

        GenerateTruthGrid();
        isFirstSquareOpened = true;
    }

    private void GenerateGameGrid()
    {
        gridGO = new Square_new[width, height];
        Vector2 gridOrigin = new(-(width - 1) * squareSize / 2f, -(height - 1) * squareSize / 2f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 spawnPos = gridOrigin + new Vector2(x * squareSize, y * squareSize);
                Square_new square = Instantiate(squarePrefab, spawnPos, Quaternion.identity, transform).GetComponent<Square_new>();
                square.Init(this, new int2(x, y));
                gridGO[x, y] = square;
            }
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

            if (IsWithinGrid(neighborX, neighborY) && TruthGrid[neighborX, neighborY] != MINE)
            {
                TruthGrid[neighborX, neighborY]++;
            }
        }
    }

    public void FloodFill(int startX, int startY)
    {
        if (!IsWithinGrid(startX, startY)) return;

        Queue<int2> queue = new();
        bool[,] visited = new bool[width, height];
        queue.Enqueue(new int2(startX, startY));

        while (queue.Count > 0)
        {
            int2 current = queue.Dequeue();
            int x = current.x;
            int y = current.y;

            if (!IsWithinGrid(x, y) || visited[x, y])
                continue;

            visited[x, y] = true;
            gridGO[x, y].RevealSquare();

            // Only enqueue neighbors if this tile is EMPTY
            if (truthGrid[x, y] == EMPTY)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx;
                        int ny = y + dy;
                        if (IsWithinGrid(nx, ny) && !visited[nx, ny])
                        {
                            queue.Enqueue(new int2(nx, ny));
                        }
                    }
                }
            }
        }
    }

    public void RemoveMineOnFirstSquare(int x, int y)
    {
        if (!isFirstSquareOpened && IsMineAt(x, y))
        {
            truthGrid[x, y] = EMPTY;
            gridGO[x, y].isMine = false;
            numOfMines--;

            // update surrounding numbers
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (IsWithinGrid(nx, ny) && truthGrid[nx, ny] != MINE)
                    {
                        truthGrid[nx, ny]--;
                    }
                }
            }
        }
        isFirstSquareOpened = true;
    }

    private bool IsMineAt(int x, int y)
    {
        if (IsWithinGrid(x, y))
        {
            return (TruthGrid[x, y] == MINE);
        }
        return false;
    }

    private bool IsWithinGrid(int x, int y)
    {
        return (x >= 0 && y >= 0 && x < width && y < height);
    }

    private void CenterCamera()
    {
        Vector3 gridCenter = new(0, 0, -10f);
        mainCamera.transform.position = gridCenter;

        // Calculate required size (half height) to fit grid with margin
        float totalWidth = width * squareSize + margin * 2f;
        float totalHeight = height * squareSize + margin * 2f;

        float screenRatio = (float)Screen.width / Screen.height;
        float targetSize = Mathf.Max(totalHeight / 2f, totalWidth / (2f * screenRatio));

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = targetSize;
    }
}
