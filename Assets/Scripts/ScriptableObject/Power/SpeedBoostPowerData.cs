using UnityEngine;

[CreateAssetMenu(fileName = "NewSpeedPower", menuName = "Inventory/Power/SpeedBoost")]
public class SpeedBoostPowerData : PowerData
{
    [Header("Speed Boost Settings")]
    public float speedAmount;
    public float speedDuration;

    public override void Apply(GameObject user)
    {
        if (user.TryGetComponent<PlayerMovement>(out var movement))
        {
            movement.ApplySpeedBoost(speedAmount, speedDuration);
            Debug.Log($"[Speed] +{speedAmount} trong {speedDuration}s cho {user.name}");
        }
        else if (user.TryGetComponent<BotController>(out var bot))
        {
            bot.ApplySpeedBoost(speedAmount, speedDuration);
            Debug.Log($"[Speed] Bot {user.name}");
        }
    }
}   