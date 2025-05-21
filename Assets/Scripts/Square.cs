using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Square : MonoBehaviour
{
    SpriteRenderer Renderer;
    [SerializeField] Sprite Square_unopened;
    [SerializeField] Sprite Square_opened_empty;
    [SerializeField] Sprite Square_opened_mine;
    [SerializeField] Sprite Square_unopened_flag;

    [SerializeField] TextMeshProUGUI mines_around;

    public bool opened = false;
    public bool flagged = false;
    public bool isMine;
    public bool isFirst;

    private bool RandBool()
    {
        return (Random.Range(0, 5) == 1);
    }
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClickedSquare();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (!opened && !flagged)
            {
                Flag();
            }
            else if (!opened && flagged)
            {
                Renderer.sprite = Square_unopened;
                flagged = false;
            }
        }
        else if (Input.GetMouseButtonDown(2) && opened)
        {
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            if (Grid.GetFlagsAround(x, y).ToString() == mines_around.text)
                Grid.OpenAround(x, y);
        }
    }
    public void ClickedSquare()
    {
        AtFirst();
        if (isMine && !flagged)
        {
            Grid.ShowMines();
            opened = true;
        }
        else if (flagged) { }
        else if (opened) { }
        else
        {
            Renderer.sprite = Square_opened_empty;
            opened = true;
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            if (Grid.GetMinesAround(x, y) != 0)
                mines_around.text = Grid.GetMinesAround(x, y).ToString();
            Grid.FFuncover(x, y, new bool[Grid.width, Grid.height]);

            if (Grid.isFinished())
                Grid.DisableActions();
        }
    }
    public void ShowMine()
    {
        if (isMine)
            Renderer.sprite = Square_opened_mine;
    }
    public void AtFirst()
    {
        if (isFirst)
        {
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            if (isMine)
                isMine = false;
            Grid.RemoveMinesAround(x, y);
            Grid.RemoveFirsts();
        }
    }
    public void Flag()
    {
        Renderer.sprite = Square_unopened_flag;
        flagged = true;
    }
    void Start()
    {
        Renderer = GetComponent<SpriteRenderer>();

        isMine = RandBool();

        int x = (int)transform.position.x;
        int y = (int)transform.position.y;
        Grid.squares[x, y] = this;

        isFirst = true;
    }

    void Update()
    {
    }
}
