// ShootButton.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class ShootButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private TouchCameraController touchCamera;
    [SerializeField] private float fireRate = 0.1f;

    private bool isShooting = false;
    private float nextFireTime = 0f;

    private void Update()
    {
        if (isShooting && Time.time >= nextFireTime)
        {
            playerShoot.Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isShooting = true;
        nextFireTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isShooting = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        touchCamera.OnDrag(eventData);
    }
}