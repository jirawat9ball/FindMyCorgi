using UnityEngine;
using UnityEngine.Events;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    private bool isDead = false;

    public UnityEvent onDamaged;  // เผื่อเอาไว้เล่นเสียงตอนโดนตี หรือทำบอสกระพริบแดง
    public UnityEvent onDefeated; // ส่งสัญญาณเมื่อบอสตาย (ชนะ)

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        onDamaged?.Invoke();

        Debug.Log($"💥 บอสโดนดาเมจ! เลือดเหลือ: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            onDefeated?.Invoke(); // บอสตาย! โยน Event แจ้งเตือน
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
    }
}