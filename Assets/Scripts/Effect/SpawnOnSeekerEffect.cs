// SpawnOnSeekerEffect.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnOnSeekerEffect", menuName = "Game/Effects/SpawnOnSeeker")]
public class SpawnOnSeekerEffect : ItemEffect
{
    public GameObject icePrefab;
    public float duration = 5f;

    public override void Apply(GameObject user)
    {
        var seekers = RoleManager.Instance.GetAllByRole(GameRole.Seeker);

        foreach (var seeker in seekers)
        {
            var bot = seeker.GetComponent<BotController>();

            if (bot != null)
                bot.ApplyFreeze(duration);

            GameObject ice = Object.Instantiate(
                icePrefab,
                seeker.transform.position,
                Quaternion.identity,
                seeker.transform
            );

            Object.Destroy(ice, duration);
        }
    }
}