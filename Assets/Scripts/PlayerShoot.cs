using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask hitMask;

    [Header("Hitscan VFX")]
    [SerializeField] private GameObject impactParticlePrefab;

    [Header("Muzzle Flash")]
    [SerializeField] private ParticleSystem muzzleFlash;

    private RoleComponent roleComponent;

    private void Awake()
    {
        roleComponent = GetComponent<RoleComponent>();
    }

    public void Shoot()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.transform.position = firePoint.position;
            muzzleFlash.transform.rotation = firePoint.rotation;
            muzzleFlash.Play();
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, 300f, hitMask))
        {
            if (impactParticlePrefab != null)
            {
                GameObject impact = Instantiate(
                    impactParticlePrefab,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );
                Destroy(impact, 2f);
            }

            // Seeker không damage Seeker
            if (hit.collider.TryGetComponent<RoleComponent>(out var targetRole))
            {
                if (roleComponent.Role == GameRole.Seeker &&
                    targetRole.Role == GameRole.Seeker)
                    return;
            }

            if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                damageable.TakeDamage(10, gameObject);
        }
    }
}