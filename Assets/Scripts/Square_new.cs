using TMPro;
using Unity.Mathematics;
using UnityEngine;
using static Utils;

public class Square_new : MonoBehaviour
{
    public bool isOpen = false;
    public bool isFlagged = false;
    public bool isMine;
    public int2 coordinates;

    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;
    [SerializeField] Sprite squareUnopened;
    [SerializeField] Sprite squareUnopenedFlag;
    [SerializeField] Sprite squareOpenedEmpty;
    [SerializeField] Sprite squareOpenedMine;

    [SerializeField] TextMeshProUGUI number;

    private int truthGridNumber;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        coordinates = new();
    }

    public void Init(GameManager gameManager, int2 coordinates)
    {
        this.gameManager = gameManager;
        this.coordinates = coordinates;
    }

    public void InitMines()
    {
        truthGridNumber = gameManager.TruthGrid[coordinates.x, coordinates.y];
        isMine = (truthGridNumber == MINE);
    }

    public void OpenSquare()
    {
        gameManager.SetEmptyBeginningSquaresAndTruth(coordinates.x, coordinates.y);

        if (isFlagged || isOpen)
        {
            return;
        }

        if (isMine)
        {
            gameManager.OpenGrid();
            isOpen = true;
        }
        else
        {
            spriteRenderer.sprite = squareOpenedEmpty;
            isOpen = true;
            if (truthGridNumber != EMPTY)
            {
                number.text = truthGridNumber.ToString();
            }
            gameManager.FloodFill(coordinates.x, coordinates.y);

            //if (Grid.isFinished())
            //    Grid.DisableActions();
        }
    }

    public void RevealSquare()
    {
        spriteRenderer.sprite = isMine ? squareOpenedMine : squareOpenedEmpty;
        if (truthGridNumber > 0)
        {
            number.text = truthGridNumber.ToString();
        }
        isOpen = true;
        isFlagged = false;
    }

    private void ToggleFlag()
    {
        if (!isOpen) 
        { 
            isFlagged = !isFlagged;
            spriteRenderer.sprite = isFlagged ? squareUnopenedFlag: squareUnopened;
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OpenSquare();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ToggleFlag();
        }
        else if (Input.GetMouseButtonDown(2))
        {
            gameManager.Chording(coordinates.x, coordinates.y);
        }
    }
}
