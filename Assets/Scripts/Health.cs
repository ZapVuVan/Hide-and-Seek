// Health.cs
using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public event Action OnDie;
    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} took {amount} damage, current health: {currentHealth}/{maxHealth}");
        // Báo cho BotController biết bị bắn
        GetComponent<BotController>()?.OnHit();

        if (currentHealth <= 0)
            OnDie?.Invoke();
    }




    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    public float GetHealthPercent() => currentHealth / maxHealth;
}