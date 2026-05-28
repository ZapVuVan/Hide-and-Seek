// SpawnOnSeekerEffect.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnOnSeekerEffect", menuName = "Game/Effects/SpawnOnSeeker")]
public class SpawnOnSeekerEffect : ItemEffect
{
    public GameObject prefab;
    public float duration = 5f;

    public override void Apply(GameObject user)
    {
        var seekers = RoleManager.Instance.GetAllByRole(GameRole.Seeker);
        foreach (var seeker in seekers)
        {
            GameObject obj = Instantiate(prefab, seeker.transform.position, Quaternion.identity);
            Object.Destroy(obj, duration);
        }
    }
}