using UnityEngine;
using UnityEngine.Events;

public class CountdownTimer : MonoBehaviour
{
    public float timeLimit = 60f;
    private float currentTime;
    private bool isRunning = false;

    public UnityEvent onTimeOut; // ส่งสัญญาณเมื่อหมดเวลา

    private void Start()
    {
        currentTime = timeLimit;
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false; // สั่งหยุดเวลา
    }

    public void ResetTimer()
    {
        currentTime = timeLimit;
    }

    private void Update()
    {
        if (!isRunning) return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                onTimeOut?.Invoke(); // หมดเวลา! โยน Event แจ้งเตือน
            }
        }
    }
}