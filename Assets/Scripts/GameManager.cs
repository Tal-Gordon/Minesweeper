using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Utils;

public class GameManager : MonoBehaviour
{
    public GameObject squarePrefab;

    public int width;
    public int height;
    [SerializeField] private int numOfMines = 80;
    public event Action<int> OnMineCountChanged;

    private Camera mainCamera;
    private Square_new[,] gridGO;

    private int[,] truthGrid;
    private List<int2> emptyBeginningSquares;
    private bool isFirstSquareOpened = false;
    private readonly float squareSize = 1f;
    private readonly float margin = 1f;

    public int[,] TruthGrid { get => truthGrid; private set => truthGrid = value; }
    public int NumOfMines 
    { 
        get => numOfMines;
        private set
        {
            if (numOfMines != value) // only trigger if the value actually changes
            {
                numOfMines = value;
                OnMineCountChanged?.Invoke(numOfMines); // notify observers
                // ?. (null-conditional operator) ensures that the event is only invoked if there are subscribers, preventing a NullReferenceException
            }
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        GenerateGameGrid();
        CenterCamera();
        OnMineCountChanged?.Invoke(NumOfMines);
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

        List<int2> mines = new(NumOfMines);
        var rand = new System.Random();

        while (mines.Count < NumOfMines)
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

        emptyBeginningSquares = new() { new int2(x, y) };

        ForEachNeighbor(x, y, width, height, (nx, ny) =>
        {
            emptyBeginningSquares.Add(new int2(nx, ny));
        });

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
        ForEachNeighbor(x, y, width, height, (nx, ny) =>
        {
            if (TruthGrid[nx, ny] != MINE)
            {
                TruthGrid[nx, ny]++;
            }
        });
    }

    public void FloodFill(int x, int y)
    {
        bool[,] visited = new bool[width, height];
        FloodFillRecursive(x, y, visited);
    }

    private void FloodFillRecursive(int x, int y, bool[,] visited)
    {
        if (!IsWithinGrid(x, y, width, height) || visited[x, y])
            return;

        visited[x, y] = true;
        gridGO[x, y].RevealSquare();

        // Recurse only if the current tile is EMPTY
        if (truthGrid[x, y] == EMPTY)
        {
            ForEachNeighbor(x, y, width, height, (nx, ny) =>
            {
                if (!visited[nx, ny])
                {
                    FloodFillRecursive(nx, ny, visited);
                }
            });
        }
    }

    public void RemoveMineOnFirstSquare(int x, int y)
    {
        if (!isFirstSquareOpened && TryIsMineAt(x, y, out bool isMine) && isMine)
        {
            truthGrid[x, y] = EMPTY;
            gridGO[x, y].isMine = false;
            NumOfMines--;

            // update surrounding numbers
            ForEachNeighbor(x, y, width, height, (nx, ny) =>
            {
                if (TruthGrid[nx, ny] != MINE)
                {
                    TruthGrid[nx, ny]--;
                }
            });
        }
        isFirstSquareOpened = true;
    }

    private bool TryIsMineAt(int x, int y, out bool isMine)
    {
        if (IsWithinGrid(x, y, width, height))
        {
            isMine = (TruthGrid[x, y] == MINE);
            return true;
        }

        isMine = false;
        return false;
    }

    public void Chording(int x, int y) // open squares around numbers where correct number of flags has been set
    {
        Square_new square = gridGO[x, y];
        if (!square.isFlagged && square.isOpen)
        {
            int squareNumber = truthGrid[x, y];
            int flagsAroundSquare = 0;

            ForEachNeighbor(x, y, width, height, (nx, ny) =>
            {
                if (gridGO[nx, ny].isFlagged)
                {
                    flagsAroundSquare++;
                }
            });

            if (flagsAroundSquare == squareNumber) // only if EXACT number
            {
                ForEachNeighbor(x, y, width, height, (nx, ny) =>
                {
                    gridGO[nx, ny].OpenSquare();
                });
            }
        }
    }

    public void IncrementMineCounter()
    {
        NumOfMines++;
    }

    public void DecrementMineCounter()
    {
        NumOfMines--;
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
