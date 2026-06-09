using UnityEngine;

[CreateAssetMenu(fileName = "NewHealPower", menuName = "Inventory/Power/Heal")]
public class HealPowerData : PowerData
{
    [Header("Heal Settings")]
    public float healAmount;

    public override void Apply(GameObject user)
    {
        if (user.TryGetComponent<Health>(out var health))
        {
            health.Heal(healAmount);
            Debug.Log($"[Heal] +{healAmount} cho {user.name}");
        }
    }
}