using UnityEngine;
using UnityEngine.AI;

public class BotAnim : MonoBehaviour
{
    private Animator anim;
    private NavMeshAgent agent;
    private Health health;
    private BotController botController;

    private bool isBotDead = false;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        botController = GetComponent<BotController>();
    }

    private void Start()
    {
        if (health != null)
        {
            health.OnDie += Bot_OnDie;
            health.OnRespawn += Bot_OnRespawn;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDie -= Bot_OnDie;
            health.OnRespawn -= Bot_OnRespawn;
        }
    }

    private void Bot_OnDie()
    {
        isBotDead = true;

        // Kích hoạt animation chết
        if (anim != null) anim.SetBool("isDead", true);

        // Tắt script AI để bot ngừng tìm đường/đi săn ngay lập tức
        if (botController != null)
        {
            botController.enabled = false;
        }

        // Không cần tắt agent tại đây để tránh lỗi NavMesh bực mình, 
        // GameManager sẽ xử lý việc disable/enable khi Warp sau 2 giây.
    }

    private void Bot_OnRespawn()
    {
        isBotDead = false;

        // Đứng dậy, quay lại trạng thái di chuyển bình thường
        if (anim != null) anim.SetBool("isDead", false);

        // Bật lại bộ não AI cho Bot
        if (botController != null)
        {
            botController.enabled = true;
        }
    }

    private void Update()
    {
        // Nếu Bot đã chết thì dừng mọi cập nhật chuyển động chạy
        if (isBotDead) return;

        if (agent != null && agent.enabled)
        {
            anim.SetBool("isRunning", agent.velocity.magnitude > 0.1f);
        }
    }
}