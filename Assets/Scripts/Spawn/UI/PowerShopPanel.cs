using UnityEngine;

public class PowerShopPanel : MonoBehaviour
{
    [SerializeField] private PowerCardUI[] cardSlots;
    [SerializeField] private PowerData[] availablePowers;

    private void OnEnable()
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i < availablePowers.Length && availablePowers[i] != null)
                cardSlots[i].Setup(availablePowers[i]);
        }
    }
}