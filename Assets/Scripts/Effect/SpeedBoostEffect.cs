// SpeedBoostEffect.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedBoostEffect", menuName = "Game/Effects/SpeedBoost")]
public class SpeedBoostEffect : ItemEffect
{
    public float amount;
    public float duration;

    public override void Apply(GameObject user)
    {
        if (user.TryGetComponent<PlayerMovement>(out var movement))
        {
            Debug.Log($"Applying SpeedBoostEffect to {user.name}, boosting speed by {amount} for {duration} seconds.");
            movement.ApplySpeedBoost(amount, duration);
        }
            
    }
}