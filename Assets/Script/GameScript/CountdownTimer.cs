using UnityEngine;
using UnityEngine.Events;
using TMPro; // 🌟 เรียกใช้ TextMeshPro

public class CountdownTimer : MonoBehaviour
{
    public float timeLimit = 60f;
    private float currentTime;
    private bool isRunning = false;

    [Header("UI Settings")]
    public TextMeshProUGUI timerText; // 🌟 ช่องสำหรับลาก TextMeshPro มาใส่

    public UnityEvent onTimeOut;

    private void Start()
    {
        // 🌟 วิ่งหา UI เวลาอัตโนมัติ
        if (timerText == null)
        {
            
            GameObject timerObj = GameObject.Find("Time txt");
            if (timerObj != null)
            {
                timerText = timerObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogWarning("⚠️ หา UI เวลาไม่เจอ! ตรวจสอบชื่อ GameObject อีกทีครับ");
            }
        }

        currentTime = timeLimit;
        UpdateTimerUI();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    // 🌟 ฟังก์ชันใหม่: เอาไว้ให้บอสสั่งหักเวลาตอนทายผิด
    public void ReduceTime(float penaltyTime)
    {
        if (!isRunning) return;

        currentTime -= penaltyTime;
        if (currentTime < 0) currentTime = 0;

        UpdateTimerUI();
        Debug.Log($"⏳ โดนหักเวลา! เหลือเวลา: {currentTime} วินาที");
    }

    private void Update()
    {
        if (!isRunning) return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI(); // 🌟 อัปเดต UI ทุกเฟรม

            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                UpdateTimerUI();
                onTimeOut?.Invoke();
            }
        }
    }

    // 🌟 ฟังก์ชันจัดการ UI โดยเฉพาะ
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // แสดงผลเป็นตัวเลขจำนวนเต็ม (วินาที)
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
    }
}