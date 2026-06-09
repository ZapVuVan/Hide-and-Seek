using UnityEngine;

public abstract class PowerData : ItemData
{
    [Header("Power Stats")]
    public int maxCharges;
    public float cooldown;

    void OnValidate() => itemType = ItemType.Power;

    public abstract void Apply(GameObject user);
}