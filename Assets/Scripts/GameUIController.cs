using UnityEngine;
using TMPro;
using JetBrains.Annotations;

public class GameUIController : MonoBehaviour
{
    public GameManager gameManager;
    public TextMeshProUGUI mineCounter;
    public TextMeshProUGUI time;

    private float timerAccumulator;
    private int currentTime;
    private bool timerActive;

    void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnMineCountChanged += UpdateMineDisplay; // subscribe to the event
        }
    }

    void OnDisable() // unsubscribe to prevent memory leaks
    {
        if (gameManager != null)
        {
            gameManager.OnMineCountChanged -= UpdateMineDisplay;
        }
    }

    void Start()
    {
        timerAccumulator = 0f;
        currentTime = 0;
        timerActive = true;
        UpdateTimeDisplay();
    }

    void Update()
    {
        if (timerActive)
        {
            UpdateTime();
        }
    }

    public void ToggleTimer()
    {
        timerActive = !timerActive;
    }

    private void UpdateTime()
    {
        timerAccumulator += Time.deltaTime;

        // check if a full second has passed
        if (timerAccumulator >= 1f)
        {
            currentTime += Mathf.FloorToInt(timerAccumulator); // add whole second
            timerAccumulator -= Mathf.FloorToInt(timerAccumulator); // subtract whole second from accumulator
            UpdateTimeDisplay();
        }
    }

    private void UpdateTimeDisplay()
    {
        time.text = currentTime.ToString() + "s";
    }

    private void UpdateMineDisplay(int numOfMines)
    {
        mineCounter.text = numOfMines.ToString();
    }
}
