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
        // 1. Ray từ tâm màn hình
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        // 2. Hướng bay từ đầu súng → target
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        // 3. Spawn bullet
        GameObject obj = GameObjectPool.Instance.Get(
            bulletTag,
            firePoint.position,
            Quaternion.LookRotation(direction) // 👈 QUAN TRỌNG
        );

        if (obj != null && obj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.Init(bulletTag, targetPoint, roleComponent.Role, gameObject);
        }
    }
}