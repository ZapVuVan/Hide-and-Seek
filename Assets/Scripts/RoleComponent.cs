using UnityEngine;

public class RoleComponent : MonoBehaviour
{
    public GameRole Role { get; private set; } = GameRole.None;

    private void Start()
    {
        RoleManager.Instance.Register(this);
    }
    public void SetRole(GameRole role)
    {
        Role = role;
        RoleManager.Instance.NotifyRolesChanged();
        GetComponent<IRole>()?.OnRoleChanged(role);

        // Tắt outline nếu là bot hider
        bool isBotHider = role == GameRole.Hider && GetComponent<BotController>() != null;

        uint layerMask = isBotHider ? 1u << 0 : role switch // Default = không outline
        {
            GameRole.Hider => 1u << 1,
            GameRole.Seeker => 1u << 2,
            _ => 1u << 0
        };

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.renderingLayerMask = layerMask;
    }

    private void OnDestroy()
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.Unregister(this);
    }
}