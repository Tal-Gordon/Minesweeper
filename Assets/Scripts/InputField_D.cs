using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputField_D : MonoBehaviour
{
    public TMP_InputField width;
    public TMP_InputField height;
    public GameObject Ggrid;
    public Canvas canvas;
    string inputted_w;
    string inputted_h;

    public void EndEdit_W()
    {
        inputted_w = width.text;
        try
        {
            Grid.width = int.Parse(inputted_w);
        }
        catch { }
    }
    public void EndEdit_H()
    {
        inputted_h = height.text;
        try
        {
            Grid.height = int.Parse(inputted_h);
            canvas.enabled = false;
            Ggrid.GetComponent<Grid>().GenerateGrid();
        }
        catch { }
    }
}
