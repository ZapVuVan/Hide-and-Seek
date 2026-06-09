using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float maxLifetime = 3f; // Sẽ dùng làm bảo hiểm nếu đạn bay lên trời

    private string poolTag;
    private Vector3 moveDirection; // ĐỔI THÀNH: Hướng di chuyển thay vì điểm đích cố định
    private bool isMoving;
    private GameRole ownerRole;
    private float spawnTime;
    private GameObject owner;
    private Vector3 startPosition; // Lưu vị trí lúc bắn để tính khoảng cách 500m
    private bool hasHitSomething;  // Đánh dấu khi đã va chạm để chạy delay 0.5s

    public void Init(string tag, Vector3 target, GameRole role, GameObject ownerObject = null)
    {
        poolTag = tag;
        isMoving = true;
        hasHitSomething = false; // Reset trạng thái khi tái sử dụng từ Pool
        ownerRole = role;
        spawnTime = Time.time;
        owner = ownerObject;
        startPosition = transform.position;

        // Tính toán hướng bay thẳng từ súng đến điểm target được chỉ định
        transform.LookAt(target);
        moveDirection = (target - transform.position).normalized;
    }

    private void Update()
    {
        if (!isMoving || hasHitSomething) return;

        // ĐỔI THÀNH: Bay liên tục theo hướng moveDirection (không bị khựng lại tại targetPoint nữa)
        transform.position += moveDirection * speed * Time.deltaTime;

        // BẢO HIỂM 1: Nếu đi quá 500m từ điểm xuất phát -> Biến mất luôn
        if (Vector3.Distance(startPosition, transform.position) >= 500f)
        {
            ReturnToPoolDirectly();
            return;
        }

        // BẢO HIỂM 2: Nếu hết thời gian tồn tại (ví dụ bắn lên trời) -> Biến mất luôn
        if (Time.time - spawnTime >= maxLifetime)
        {
            ReturnToPoolDirectly();
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Nếu đã va chạm rồi thì không xử lý lại nữa (tránh đạn xuyên thấu nhiều mục tiêu một lúc)
        if (hasHitSomething) return;

        // Bỏ qua va chạm với chính người bắn
        if (ownerObjectMatches(other.gameObject)) return;

        GameRole hitRole = RoleManager.Instance.GetRole(other.gameObject);

        // Xử lý gây sát thương
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

        // Bắt đầu chu trình delay 0.5s trước khi thu hồi về Pool
        StartCoroutine(DelayReturnRoutine());
    }

    private IEnumerator DelayReturnRoutine()
    {
        hasHitSomething = true;

        // Đóng băng viên đạn lại ngay tại điểm va chạm
        isMoving = false;

        // Nếu bạn có MeshRenderer hay Particle đuôi đạn, bạn có thể tắt Mesh đi tại đây 
        // để người chơi thấy đạn nổ bụp cái rồi biến mất, giữ lại khói đuôi. Vd: GetComponent<MeshRenderer>().enabled = false;

        // Chờ đúng 0.5 giây như bạn yêu cầu
        yield return new WaitForSeconds(0.5f);

        ReturnToPoolDirectly();
    }

    // Hàm trả về Pool ngay lập tức
    private void ReturnToPoolDirectly()
    {
        isMoving = false;
        hasHitSomething = false;
        GameObjectPool.Instance.Return(poolTag, gameObject);
    }

    private bool ownerObjectMatches(GameObject go)
    {
        if (owner == null) return false;
        return go == owner || go.transform.IsChildOf(owner.transform);
    }
}