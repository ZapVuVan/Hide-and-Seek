// Health.cs
using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public event EventHandler<float> OnHealthChanged;
    public event Action OnDie;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(this, GetHealthPercent());

        GetComponent<BotController>()?.OnHit();

        if (currentHealth <= 0)
            OnDie?.Invoke();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(this, GetHealthPercent());
    }

    public float GetHealthPercent() => currentHealth / maxHealth;
}