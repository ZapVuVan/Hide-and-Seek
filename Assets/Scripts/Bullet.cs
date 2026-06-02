using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float maxLifetime = 3f;
    private string poolTag;
    private Vector3 targetPoint;
    private bool isMoving;
    private GameRole ownerRole;
    private float spawnTime;
    private GameObject owner;

    public void Init(string tag, Vector3 target, GameRole role, GameObject ownerObject = null)
    {
        poolTag = tag;
        targetPoint = target;
        isMoving = true;
        ownerRole = role;
        spawnTime = Time.time;
        owner = ownerObject;
        transform.LookAt(targetPoint);
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint,
            speed * Time.deltaTime
        );

        if (Time.time - spawnTime >= maxLifetime)
        {
            ReturnToPool();
            return;
        }

        if (transform.position == targetPoint)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameRole hitRole = RoleManager.Instance.GetRole(other.gameObject);
        if (hitRole != ownerRole && hitRole != GameRole.None)
        {
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                if (damageable is Health health)
                    health.TakeDamage(damage, owner);
                else
                    damageable.TakeDamage(damage);
            }
        }
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        isMoving = false;
        GameObjectPool.Instance.Return(poolTag, gameObject);
    }
}