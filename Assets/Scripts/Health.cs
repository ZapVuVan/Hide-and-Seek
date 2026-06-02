using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private GameObject lastAttacker;

    public event EventHandler<float> OnHealthChanged;
    public event Action OnDie;
    public event Action<GameObject, GameObject> OnKilled;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (attacker != null)
            lastAttacker = attacker;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(this, GetHealthPercent());
        GetComponent<BotController>()?.OnHit();

        if (currentHealth <= 0)
        {
            OnDie?.Invoke();
            OnKilled?.Invoke(lastAttacker, gameObject);
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(this, GetHealthPercent());
    }

    public float GetHealthPercent() => currentHealth / maxHealth;
}