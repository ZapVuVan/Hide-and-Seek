using UnityEngine;

public class SpeedBoostItem : IBotItem
{
    public string ItemName => "Speed";
    public void Use(BotController bot)
    {
        bot.StartCoroutine(SpeedRoutine(bot.Agent));
    }

    private System.Collections.IEnumerator SpeedRoutine(UnityEngine.AI.NavMeshAgent agent)
    {
        if (agent == null) yield break;
        float originalSpeed = agent.speed;
        agent.speed *= 1.5f; // Tăng 50% tốc độ
        yield return new WaitForSeconds(5f);
        if (agent != null) agent.speed = originalSpeed;
    }
}