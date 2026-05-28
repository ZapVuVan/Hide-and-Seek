// BotShoot.cs
using UnityEngine;

public class BotShoot : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private string bulletTag = "Bullet";

    public void Shoot(Vector3 targetPos)
    {
        GameObject obj = GameObjectPool.Instance.Get(
            bulletTag,
            firePoint.position,
            Quaternion.identity
        );

        if (obj != null && obj.TryGetComponent<Bullet>(out var bullet))
            bullet.Init(bulletTag, targetPos, GameRole.Seeker);
    }
}