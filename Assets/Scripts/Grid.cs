using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

public class Grid: MonoBehaviour
{
    public static int width;
    public static int height;
    public static Square[,] squares;
    public GameObject squarePrefab;

    [SerializeField] private Camera cam;
    [SerializeField] private float zoomSpeed = 20f;
    [SerializeField] private float minCamSize = 5f;
    [SerializeField] private float maxCamSize = 20f;

    Vector2 mouseClickPos;
    Vector2 mouseCurrentPos;

    [SerializeField] static GameObject fireworks;

    public void GenerateGrid()
    {
        squares = new Square[width, height];
        for (int k = 0; k < squares.GetLength(0); k++)
        {
            for (int l = 0; l < squares.GetLength(1); l++)
            {
                Instantiate(squarePrefab, new Vector3(k, l, 0), Quaternion.identity);
            }
        }
    }
    public static void ShowMines()
    {
            foreach (Square square in squares)
            {
                square.ShowMine();
                square.opened = true;
            }
    }
    public static bool IsMineAt(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
            return squares[x, y].isMine;
        return false;
    }
    public static bool IsFlagAt(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
            return squares[x, y].flagged;
        return false;
    }
    public static int GetMinesAround(int x, int y)
    {
        int count = 0;

        if (IsMineAt(x, y + 1)) ++count; // top
        if (IsMineAt(x + 1, y + 1)) ++count; // top-right
        if (IsMineAt(x + 1, y)) ++count; // right
        if (IsMineAt(x + 1, y - 1)) ++count; // bottom-right
        if (IsMineAt(x, y - 1)) ++count; // bottom
        if (IsMineAt(x - 1, y - 1)) ++count; // bottom-left
        if (IsMineAt(x - 1, y)) ++count; // left
        if (IsMineAt(x - 1, y + 1)) ++count; // top-left

        return count;
    }
    public static void RemoveMinesAround(int x, int y)
    {
        if (IsMineAt(x, y + 1)) squares[x, y + 1].isMine = false; // top
        if (IsMineAt(x + 1, y + 1)) squares[x + 1, y + 1].isMine = false; // top-right
        if (IsMineAt(x + 1, y)) squares[x + 1, y].isMine = false; // right
        if (IsMineAt(x + 1, y - 1)) squares[x + 1, y - 1].isMine = false; // bottom-right
        if (IsMineAt(x, y - 1)) squares[x, y - 1].isMine = false; // bottom
        if (IsMineAt(x - 1, y - 1)) squares[x - 1, y - 1].isMine = false; // bottom-left
        if (IsMineAt(x - 1, y)) squares[x - 1, y].isMine = false; // left
        if (IsMineAt(x - 1, y + 1)) squares[x - 1, y + 1].isMine = false; // top-left
    }
    public static int GetFlagsAround(int x, int y)
    {
        int count = 0;

        if (IsFlagAt(x, y + 1)) ++count; // top
        if (IsFlagAt(x + 1, y + 1)) ++count; // top-right
        if (IsFlagAt(x + 1, y)) ++count; // right
        if (IsFlagAt(x + 1, y - 1)) ++count; // bottom-right
        if (IsFlagAt(x, y - 1)) ++count; // bottom
        if (IsFlagAt(x - 1, y - 1)) ++count; // bottom-left
        if (IsFlagAt(x - 1, y)) ++count; // left
        if (IsFlagAt(x - 1, y + 1)) ++count; // top-left

        return count;
    }
    public static void FFuncover(int x, int y, bool[,] visited)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            if (visited[x, y])
                return;

            squares[x, y].ClickedSquare();

            if (GetMinesAround(x, y) > 0)
                return;

            visited[x, y] = true;

            FFuncover(x - 1, y, visited);
            FFuncover(x + 1, y, visited);
            FFuncover(x, y - 1, visited);
            FFuncover(x, y + 1, visited);

            FFuncover(x + 1, y + 1, visited);
            FFuncover(x - 1, y + 1, visited);
            FFuncover(x + 1, y - 1, visited);
            FFuncover(x - 1, y - 1, visited);

        }
    }
    public static void OpenAround(int x, int y)
    {
        try { squares[x + 1, y + 1].ClickedSquare(); }//top-right
        catch { }
        try { squares[x, y + 1].ClickedSquare(); } //top
        catch { }
        try { squares[x - 1, y + 1].ClickedSquare(); } //top-left
        catch { }
        try { squares[x + 1, y].ClickedSquare(); }//right
        catch { }
        try { squares[x - 1, y].ClickedSquare(); } //left
        catch { }
        try { squares[x - 1, y - 1].ClickedSquare(); } //bottom-left
        catch { }
        try { squares[x, y - 1].ClickedSquare(); } //bottom
        catch { }
        try { squares[x + 1, y - 1].ClickedSquare(); } //bottom-right
        catch { }
    }
    public static bool isFinished()
    {
        foreach (Square square in squares)
        {
            if (!square.opened && !square.isMine)
                return false;
        }
        return true;
    }
    public static void DisableActions()
    {
        foreach (Square square in squares)
        {
            if (!square.opened)
                square.Flag();
            square.opened = true;
        }
        fireworks.GetComponent<VisualEffect>().enabled = true;
    }
    public static void RemoveFirsts()
    {
        foreach (Square square in squares)
            square.isFirst = false;
    }
    void Zoom()
    {
        float mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        float newZoomLevel = cam.orthographicSize - mouseScrollWheel;

        Vector3 mouseOnWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        cam.orthographicSize = Mathf.Clamp(newZoomLevel, minCamSize, maxCamSize);
        Vector3 mouseOnWorld1 = cam.ScreenToWorldPoint(Input.mousePosition);

        Vector3 posDiff = mouseOnWorld - mouseOnWorld1;

        Vector3 camPos = cam.transform.position;
        Vector3 targetPos = new Vector3(
            camPos.x + posDiff.x,
            camPos.y + posDiff.y,
            camPos.z);

        cam.transform.position = targetPos;
    }
    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Easy")
        {
            width = 7;
            height = 10;
        }
        if (SceneManager.GetActiveScene().name == "Medium")
        {
            width = 12;
            height = 22;
        }
        if (SceneManager.GetActiveScene().name == "Hard")
        {
            width = 18;
            height = 32;
        }
        if (SceneManager.GetActiveScene().name == "Custom")
        {
            width = 0;
            height = 0;
        }
    }
    void Start()
    {
        GenerateGrid();
        fireworks = GameObject.Find("Fireworks");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MainMenu");

        Zoom();

        if (Input.GetMouseButton(0))
        {
            if (mouseClickPos == default)
            {
                mouseClickPos = cam.ScreenToWorldPoint(Input.mousePosition);
            }

            mouseCurrentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            var distance = mouseCurrentPos - mouseClickPos;
            cam.transform.position += new Vector3(-distance.x, -distance.y, 0);
        }

        if (Input.GetMouseButtonUp(0))
            mouseClickPos = default;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (Square square in squares)
            {
                if (!square.isMine)
                    square.ClickedSquare();
            }
        }
    }
}
