using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth;
    private GameObject lastAttacker;

    public event EventHandler<float> OnHealthChanged;
    public event Action OnDie;
    public event Action OnRespawn; // <--- THÊM EVENT NÀY
    public event Action<GameObject, GameObject> OnKilled;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (currentHealth <= 0) return; // Nếu đã chết rồi thì không nhận thêm damage

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

    public void RespawnHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(this, GetHealthPercent());
        OnRespawn?.Invoke(); // Kích hoạt event hồi sinh
    }

    public float GetHealthPercent() => currentHealth / maxHealth;
}