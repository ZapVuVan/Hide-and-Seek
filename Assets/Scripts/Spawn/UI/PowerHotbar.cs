using System.Collections.Generic;
using UnityEngine;

public class PowerHotbar : MonoBehaviour
{
    public static PowerHotbar Instance { get; private set; }

    [Header("UI")]
    public Transform hotbarContainer;
    public GameObject useItemButtonPrefab;

    private List<UseItemButton> activeButtons = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        InventoryManager.OnInventoryChanged += Refresh;
        Refresh();
    }

    public void Refresh()
    {
        foreach (var btn in activeButtons)
            if (btn != null) Destroy(btn.gameObject);
        activeButtons.Clear();

        if (InventoryManager.Instance == null) return;

        var powerItems = InventoryManager.Instance.GetItemsByType(ItemType.Power);
        foreach (var item in powerItems)
        {
            if (item == null) continue;

            var pd = item as PowerData;
            if (pd == null) continue;

            int charges = InventoryManager.Instance.GetCharges(pd.itemId);
            if (charges <= 0) continue; // Chỉ hiện khi có charge

            var go = Instantiate(useItemButtonPrefab, hotbarContainer);
            var btn = go.GetComponent<UseItemButton>();
            if (btn == null) continue;

            btn.Setup(pd);
            activeButtons.Add(btn);
        }
    }
}