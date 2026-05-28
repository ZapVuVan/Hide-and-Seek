// ShootButton.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class ShootButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private PlayerShoot playerShoot;

    public void OnPointerDown(PointerEventData eventData)
    {
        playerShoot.Shoot();
    }
}