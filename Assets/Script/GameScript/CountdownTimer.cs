using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public float timeLimit = 60f;
    private float currentTime;
    private bool isRunning = false;

    public TextMeshProUGUI timerText;
    public UnityEvent onTimeOut;

    private void Start()
    {
        // 🌟 งมหา UI ที่ออโต้เจนมาจาก Core Manager
        if (timerText == null)
        {
            if (Gamemanager.Instance != null && Gamemanager.Instance.uiIngame != null)
            {
                // เข้าถึงก้อน UI หลัก
                Transform mainGame = Gamemanager.Instance.uiIngame.panalgame.transform;

                // หา Boss Panal ที่ซ่อนอยู่
                Transform bossPanel = mainGame.Find("Boss Panal");
                if (bossPanel != null)
                {
                    bossPanel.gameObject.SetActive(true); // บังคับเปิดทันที!

                    // เจาะเข้าไปหา Time txt
                    Transform timeTxtTrans = bossPanel.Find("Time/Time txt");
                    if (timeTxtTrans != null)
                    {
                        timerText = timeTxtTrans.GetComponent<TextMeshProUGUI>();
                        Debug.Log("✅ CountdownTimer ดึง Time txt จากระบบออโต้เจนสำเร็จ!");
                    }
                }
            }
        }

        currentTime = timeLimit;
        UpdateTimerUI();
    }

    public void StartTimer() => isRunning = true;
    public void StopTimer() => isRunning = false;

    public void ReduceTime(float penaltyTime)
    {
        if (!isRunning) return;
        currentTime -= penaltyTime;
        if (currentTime < 0) currentTime = 0;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!isRunning || currentTime <= 0) return;

        currentTime -= Time.deltaTime;
        UpdateTimerUI();

        if (currentTime <= 0)
        {
            isRunning = false;
            onTimeOut?.Invoke();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
    }
}