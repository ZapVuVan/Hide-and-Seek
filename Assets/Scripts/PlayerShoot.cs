// PlayerShoot.cs
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private string bulletTag = "Bullet";
    [SerializeField] private Camera cam;

    private RoleComponent roleComponent;

    private void Awake()
    {
        roleComponent = GetComponent<RoleComponent>();
    }

    public void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit)
            ? hit.point
            : ray.GetPoint(100f);

        GameObject obj = GameObjectPool.Instance.Get(
            bulletTag,
            firePoint.position,
            Quaternion.identity
        );

        if (obj != null && obj.TryGetComponent<Bullet>(out var bullet))
            bullet.Init(bulletTag, targetPoint, roleComponent.Role);
    }
}