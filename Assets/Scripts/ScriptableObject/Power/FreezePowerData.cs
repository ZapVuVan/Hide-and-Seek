using UnityEngine;

[CreateAssetMenu(fileName = "NewFreezePower", menuName = "Inventory/Power/Freeze")]
public class FreezePowerData : PowerData
{
    [Header("Freeze Settings")]
    public GameObject icePrefab;
    public float freezeDuration = 5f;

    public override void Apply(GameObject user)
    {
        if (icePrefab == null) return;
        var seekers = RoleManager.Instance.GetAllByRole(GameRole.Seeker);
        foreach (var seeker in seekers)
        {
            var freezable = seeker.GetComponent<IFreezable>();
            if (freezable != null)
            {
                freezable.ApplyFreeze(freezeDuration);
                var ice = Object.Instantiate(icePrefab, seeker.transform.position,
                          Quaternion.identity, seeker.transform);
                Object.Destroy(ice, freezeDuration);
                Debug.Log($"[Freeze] {seeker.name} trong {freezeDuration}s");
            }
        }
    }
}