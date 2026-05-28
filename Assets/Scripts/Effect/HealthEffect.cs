// HealEffect.cs
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "Game/Effects/Heal")]
public class HealEffect : ItemEffect
{
    public float healAmount;

    public override void Apply(GameObject user)
    {
        if (user.TryGetComponent<Health>(out var health))
        {
            Debug.Log($"Applying HealEffect to {user.name}, healing for {healAmount}.");
            health.Heal(healAmount);
        }
            
    }
}