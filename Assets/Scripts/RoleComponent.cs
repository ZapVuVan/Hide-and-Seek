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
        var inv = GetComponent<InvisibleController>();
        if (inv != null)
        {
            if (role == GameRole.Hider)
                inv.SetTransparentMode();
            else if (role == GameRole.Seeker)
            {
                inv.SetOpaqueMode();
                inv.ResetInvisible();
            }
        }
        bool isBotHider = role == GameRole.Hider && GetComponent<BotController>() != null;
        uint layerMask = isBotHider ? 1u << 0 : role switch
        {
            GameRole.Hider => 1u << 1,
            GameRole.Seeker => 1u << 2,
            _ => 1u << 0
        };
        StartCoroutine(ApplyRenderingLayer(layerMask));
    }
    private System.Collections.IEnumerator ApplyRenderingLayer(uint layerMask)
    {
        yield return null;
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.renderingLayerMask = layerMask;
            r.enabled = false;
            r.enabled = true;
        }
    }
    private void OnDestroy()
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.Unregister(this);
    }
}