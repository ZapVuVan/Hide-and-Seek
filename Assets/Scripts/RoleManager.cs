using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoleManager : MonoBehaviour
{
    public static RoleManager Instance { get; private set; }
    private List<RoleComponent> allRoles = new List<RoleComponent>();
    public event System.Action OnRolesChanged;

    private void Awake() => Instance = this;

    public void Register(RoleComponent role)
    {
        if (!allRoles.Contains(role))
        {
            allRoles.Add(role);
            OnRolesChanged?.Invoke();
        }
    }

    public void Unregister(RoleComponent role)
    {

        bool removed = allRoles.Remove(role);
        if (removed)
            OnRolesChanged?.Invoke();
    }

    public List<RoleComponent> GetAllByRole(GameRole role)
        => allRoles.Where(r => r != null && r.Role == role).ToList(); 

    public int CountByRole(GameRole role)
        => allRoles.Count(r => r != null && r.Role == role); 

    public GameRole GetRole(GameObject obj)
    {
        var role = obj.GetComponentInParent<RoleComponent>();
        return role != null ? role.Role : GameRole.None;
    }

    public void NotifyRolesChanged()
    {
        OnRolesChanged?.Invoke();
    }
}