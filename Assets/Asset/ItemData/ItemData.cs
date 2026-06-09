using UnityEngine;

public enum ItemType { Gun, Knife, Power, Emote }
public enum ItemRarity { Common, Rare, Epic, Legendary, Mythic }

public abstract class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public ItemRarity rarity;

    [Header("Shop")]
    public int price;
    public bool isDefault;
    public string description;

    public Color GetRarityColor()
    {
        return rarity switch
        {
            ItemRarity.Rare => new Color(0.3f, 0.6f, 1f),
            ItemRarity.Epic => new Color(0.7f, 0.3f, 1f),
            ItemRarity.Legendary => new Color(1f, 0.7f, 0f),
            ItemRarity.Mythic => new Color(1f, 0.2f, 0.2f),
            _ => Color.white
        };
    }
}