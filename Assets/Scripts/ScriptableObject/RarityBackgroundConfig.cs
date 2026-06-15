using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Rarity Background Config")]
public class RarityBackgroundConfig : ScriptableObject
{
    [System.Serializable]
    public struct RarityEntry
    {
        public ItemRarity rarity;
        public Sprite backgroundSprite;
    }

    public RarityEntry[] entries;

    public Sprite GetSprite(ItemRarity rarity)
    {
        foreach (var e in entries)
            if (e.rarity == rarity) return e.backgroundSprite;
        return null;
    }
}