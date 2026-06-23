using UnityEngine;
using UnityEngine.Events;
using TMPro; // 🌟 เรียกใช้ TextMeshPro

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;
    private bool isDead = false;

    [Header("UI Settings")]
    public TextMeshProUGUI healthText; // 🌟 ช่องสำหรับลาก TextMeshPro มาใส่

    public UnityEvent onDamaged;
    public UnityEvent onDefeated;

    private void Start()
    {
        
        if (healthText == null)
        {
            
            GameObject hpObj = GameObject.Find("HP txt");
            if (hpObj != null)
            {
                healthText = hpObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogWarning("⚠️ หา UI เลือดบอสไม่เจอ! ตรวจสอบชื่อ GameObject อีกทีครับ");
            }
        }

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthUI(); // 🌟 อัปเดต UI ทันทีที่โดนตี
        onDamaged?.Invoke();

        Debug.Log($"💥 บอสโดนดาเมจ! เลือดเหลือ: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            UpdateHealthUI();
            onDefeated?.Invoke();
        }
    }

    // 🌟 ฟังก์ชันจัดการ UI
    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"HP:{currentHealth}";
        }
    }
}