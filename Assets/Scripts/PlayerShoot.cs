using UnityEngine;
using System.Collections;

public class PlayerShoot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask hitMask;

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject impactParticlePrefab;
    [SerializeField] private GameObject decalPrefab;
    [SerializeField] private GameObject bulletTrailPrefab;

    [Header("Settings")]
    [SerializeField] private float maxDistance = 300f;
    [SerializeField] private float trailSpeed = 200f;
    [SerializeField] private float decalLifeTime = 5f;
    [SerializeField] private float minTrailTime = 0.05f;

    private RoleComponent roleComponent;

    private void Awake()
    {
        roleComponent = GetComponent<RoleComponent>();
    }

    public void Shoot()
    {
        PlayMuzzleFlash();

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 endPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask))
        {
            endPoint = hit.point;

            SpawnImpactVFX(hit);
            SpawnDecal(hit);
            HandleDamage(hit);
        }
        else
        {
            endPoint = ray.origin + ray.direction * maxDistance;
        }

        SpawnBulletTrail(endPoint);
    }

    // ================= VFX =================

    private void PlayMuzzleFlash()
    {
        if (muzzleFlash == null) return;

        muzzleFlash.transform.position = firePoint.position;
        muzzleFlash.transform.rotation = firePoint.rotation;
        muzzleFlash.Play();
    }

    private void SpawnImpactVFX(RaycastHit hit)
    {
        if (impactParticlePrefab == null) return;

        GameObject impact = Instantiate(
            impactParticlePrefab,
            hit.point,
            Quaternion.LookRotation(hit.normal)
        );

        Destroy(impact, 2f);
    }

    private void SpawnDecal(RaycastHit hit)
    {
        if (decalPrefab == null) return;

        Quaternion rot = Quaternion.LookRotation(-hit.normal);
        GameObject decal = Instantiate(decalPrefab, hit.point + hit.normal * 0.01f, rot);

        decal.transform.Rotate(0, 0, Random.Range(0f, 360f));

        // Tính góc giữa ray và surface normal
        float angle = Vector3.Angle(hit.normal, -cam.transform.forward);
        // angle = 0 → bắn thẳng vuông góc → scale bình thường
        // angle = 80 → bắn rất chéo → kéo dãn theo trục X để bù lại
        float stretch = 1f / Mathf.Max(Mathf.Cos(angle * Mathf.Deg2Rad), 0.3f);
        stretch = Mathf.Clamp(stretch, 1f, 3f); // giới hạn không kéo quá 3x

        float baseScale = Random.Range(2f, 3f);
        // Kéo dãn theo local X (hướng song song với mặt bắn)
        decal.transform.localScale = new Vector3(baseScale * stretch, baseScale, baseScale);

        decal.transform.SetParent(hit.transform);
        Destroy(decal, decalLifeTime);
    }

    private void SpawnBulletTrail(Vector3 targetPoint)
    {
        if (bulletTrailPrefab == null) return;

        GameObject trail = Instantiate(bulletTrailPrefab, firePoint.position, Quaternion.identity);
        StartCoroutine(MoveTrail(trail, targetPoint));
    }

    private IEnumerator MoveTrail(GameObject trail, Vector3 target)
    {
        if (trail == null) yield break;

        Vector3 start = trail.transform.position;
        float distance = Vector3.Distance(start, target);
        float time = Mathf.Max(distance / trailSpeed, minTrailTime);

        float t = 0;

        while (t < 1f)
        {
            if (trail == null) yield break;

            t += Time.deltaTime / time;
            trail.transform.position = Vector3.Lerp(start, target, t);

            yield return null;
        }

        if (trail != null)
        {
            trail.transform.position = target;

            // destroy sau khi bay xong
            Destroy(trail, 0.1f);
        }
    }

    // ================= GAMEPLAY =================

    private void HandleDamage(RaycastHit hit)
    {
        if (!hit.collider.TryGetComponent<RoleComponent>(out var targetRole))
            return;

        if (roleComponent.Role == GameRole.Seeker &&
            targetRole.Role == GameRole.Seeker)
            return;

        if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(10, gameObject);
        }
    }
}