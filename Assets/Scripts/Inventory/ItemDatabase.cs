using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems = new();

    public ItemData GetById(string id)
        => allItems.Find(i => i.itemId == id);

    public List<ItemData> GetByType(ItemType type)
        => allItems.FindAll(i => i.itemType == type);

    public List<ItemData> GetAll()
        => allItems;
}
