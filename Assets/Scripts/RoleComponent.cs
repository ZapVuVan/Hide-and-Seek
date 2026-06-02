using UnityEngine;

public class RoleComponent : MonoBehaviour
{
    public GameRole Role { get; private set; } = GameRole.None;

    public void SetRole(GameRole role)
    {
        Role = role;
        RoleManager.Instance.Register(this);
        RoleManager.Instance.NotifyRolesChanged();
        GetComponent<IRole>()?.OnRoleChanged(role);
    }

    private void OnDestroy()
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.Unregister(this);
    }
}