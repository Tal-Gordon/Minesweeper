using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Scene select;

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color =  Color.black;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = Color.white;
    }
    public void Play(string scenename)
    {
        SceneManager.LoadScene(scenename);
    }
    public void Exit()
    {
        Application.Quit();
    }
}