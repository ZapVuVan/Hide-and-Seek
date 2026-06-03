using UnityEngine;

public abstract class ItemEffect : ScriptableObject
{

    public float cooldown;
    public abstract void Apply(GameObject user);
}   