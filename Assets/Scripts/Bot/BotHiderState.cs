using UnityEngine;
using UnityEngine.AI;

public class BotHiderState : IBotState
{
    private InvisibleController invisibleController;

    private enum HiderPersonality
    {
        Camper,     // Thích trốn nhưng giờ cũng chỉ đứng yên một lát rồi chạy tiếp
        Explorer    // Siêu tăng động, chạy đường dài liên tục, vừa đứng im là chạy ngay
    }

    private enum HiderSubState
    {
        Idle,
        Moving,
        Hiding,
        Hyperactive // Trạng thái chạy loạn xé gió khi được add tốc độ
    }

    private HiderSubState subState;
    private HiderPersonality personality;

    // 🌟 1. TĂNG BÁN KÍNH DI CHUYỂN: Ép Bot phải chọn những điểm rất xa để chạy đường dài
    private float hideRadius = 55f;
    private GameState lastGameState;
    private float changeHideSpotTimer = 0f;

    public void EnterState(BotController bot)
    {
        if (bot.Agent == null) return;

        invisibleController = bot.GetComponent<InvisibleController>();
        subState = HiderSubState.Idle;

        // Tỉ lệ ngẫu nhiên chia tính cách khi vào trận
        personality = (Random.value < 0.6f) ? HiderPersonality.Explorer : HiderPersonality.Camper;
        ResetPersonalityTimer();

        lastGameState = GameManager.Instance.CurrentState;
    }

    public void UpdateState(BotController bot)
    {
        if (bot.Agent == null) return;

        GameState currentState = GameManager.Instance.CurrentState;

        // ==========================================
        // KIỂM TRA TRẠNG THÁI SPEED BOOST TỪ ITEM
        // ==========================================
        if (bot.IsSpeedBoosted && subState != HiderSubState.Hyperactive)
        {
            subState = HiderSubState.Hyperactive;
            bot.Agent.stoppingDistance = 0.8f;
            FindRandomWanderPoint(bot);
        }
        else if (!bot.IsSpeedBoosted && subState == HiderSubState.Hyperactive)
        {
            ResetPersonalityTimer();
            StartHiding(bot);
        }

        // =========================
        // 1. AssigningRoles → đứng yên
        // =========================
        if (currentState == GameState.AssigningRoles)
        {
            bot.Agent.isStopped = true;
            return;
        }

        // =========================
        // 2. Vừa chuyển sang HidingPhase → bắt đầu chạy
        // =========================
        if (lastGameState == GameState.AssigningRoles &&
            currentState == GameState.HidingPhase)
        {
            StartHiding(bot);
        }

        lastGameState = currentState;

        // =========================
        // 3. Update invisible theo tốc độ
        // =========================
        float speed = bot.Agent.velocity.magnitude;
        invisibleController?.UpdateInvisible(speed);

        // =========================
        // 4. State logic
        // =========================
        switch (subState)
        {
            case HiderSubState.Moving:
                bot.Agent.isStopped = false;

                // Khi đang di chuyển đường dài, nếu đến đích thì chuyển sang đứng yên tạm thời
                if (!bot.Agent.pathPending &&
                    bot.Agent.remainingDistance <= bot.Agent.stoppingDistance)
                {
                    bot.Agent.isStopped = true;
                    subState = HiderSubState.Hiding;
                }
                break;

            case HiderSubState.Hiding:
                bot.Agent.isStopped = true;

                // 🌟 2. ĐẾM NGƯỢC THỜI GIAN ĐỨNG YÊN (GIỜ ĐÃ NGẮN HƠN RẤT NHIỀU)
                changeHideSpotTimer -= Time.deltaTime;
                if (changeHideSpotTimer <= 0f)
                {
                    ResetPersonalityTimer();
                    StartHiding(bot); // Hết vài giây đứng im ngắn ngủi -> lại lập tức ôm chân chạy tiếp
                }
                break;

            case HiderSubState.Hyperactive:
                bot.Agent.isStopped = false;

                if (!bot.Agent.pathPending &&
                    (bot.Agent.remainingDistance <= bot.Agent.stoppingDistance || !bot.Agent.hasPath))
                {
                    FindRandomWanderPoint(bot);
                }
                break;
        }
    }

    public void ExitState(BotController bot)
    {
        if (bot.Agent == null) return;

        if (bot.Agent.enabled && bot.Agent.isOnNavMesh)
            bot.Agent.isStopped = false;

        invisibleController?.ResetInvisible();
    }

    public void OnHit(BotController bot)
    {
        invisibleController?.ResetInvisible();
        ResetPersonalityTimer();
        StartHiding(bot);
    }

    private void StartHiding(BotController bot)
    {
        subState = HiderSubState.Moving;

        bot.Agent.isStopped = false;
        bot.Agent.stoppingDistance = 0.5f;

        // 🌟 3. TÌM ĐIỂM ĐƯỜNG DÀI: Tăng phạm vi tìm kiếm ngẫu nhiên rộng ra toàn map
        Vector3 randomPos = bot.transform.position + Random.insideUnitSphere * hideRadius;
        randomPos.y = bot.transform.position.y;

        // Tăng khoảng cách kiểm tra SamplePosition lên 20f để đảm bảo luôn bám được vào góc xa trên NavMesh
        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 20f, NavMesh.AllAreas))
        {
            bot.Agent.SetDestination(hit.position);
        }
        else
        {
            // Nếu xui quá không tìm được điểm ở xa, bắt nó chạy đại về phía trước để duy trì chuyển động liên tục
            Vector3 forwardPos = bot.transform.position + bot.transform.forward * 30f;
            if (NavMesh.SamplePosition(forwardPos, out NavMeshHit fallbackHit, 30f, NavMesh.AllAreas))
            {
                bot.Agent.SetDestination(fallbackHit.position);
            }
        }
    }

    // 🌟 4. THAY ĐỔI THỜI GIAN CHỜ: Thi thoảng mới đứng yên thôi
    private void ResetPersonalityTimer()
    {
        if (personality == HiderPersonality.Camper)
        {
            // Loại "thích đứng yên" giờ cũng chỉ cho đứng thở từ 6 đến 12 giây là phải đi chỗ khác
            changeHideSpotTimer = Random.Range(6f, 12f);
        }
        else if (personality == HiderPersonality.Explorer)
        {
            // Loại "thích chạy" thì siêu gắt: chỉ khựng lại 1 đến 3 giây để đánh lạc hướng rồi lại phi tiếp đường dài
            changeHideSpotTimer = Random.Range(1f, 3f);
        }
    }

    // Tìm điểm chạy loạn cực xa khi có Speed Boost từ Item
    private void FindRandomWanderPoint(BotController bot)
    {
        Vector3 randomDirection = Random.insideUnitSphere * 40f; // Bán kính chạy khi có speed cũng tăng lên 40m
        randomDirection += bot.transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 25f, NavMesh.AllAreas))
        {
            bot.Agent.SetDestination(hit.position);
        }
    }
}