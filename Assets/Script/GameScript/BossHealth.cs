using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;
    public TextMeshProUGUI healthText;

    public UnityEvent onDamaged;
    public UnityEvent onDefeated;

    public void SetupHealthUI(TextMeshProUGUI externalText)
    {
        healthText = externalText;
        UpdateHealthUI();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateHealthUI();
        onDamaged?.Invoke();
        if (currentHealth <= 0) onDefeated?.Invoke();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = $"HP:{currentHealth}";
    }
}