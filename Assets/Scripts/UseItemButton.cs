// UseItemButton.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UseItemButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private ItemEffect effect;
    [SerializeField] private PlayerMovement player;

    public void OnPointerDown(PointerEventData eventData)
    {
        effect?.Apply(player.gameObject);
    }
}