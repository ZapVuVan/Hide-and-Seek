    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class RoleManager : MonoBehaviour
    {
        public static RoleManager Instance { get; private set; }

        private List<RoleComponent> allRoles = new List<RoleComponent>();
        private Dictionary<RoleComponent, GameRole> pendingRoles = new Dictionary<RoleComponent, GameRole>();
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

    public GameRole GenerateRoles(PlayerController player, List<BotController> bots)
    {
        Debug.Log("===== GENERATE ROLES CALLED =====");
        Debug.Log($"ALL ROLES COUNT = {allRoles.Count}");

        foreach (var r in allRoles)
        {
            if (r == null)
            {
                Debug.Log("NULL ROLE");
                continue;
            }

            Debug.Log(
                $"RoleObject={r.name} | " +
                $"Active={r.gameObject.activeInHierarchy} | " +
                $"Role={r.Role}"
            );
        }
        pendingRoles.Clear();

        List<RoleComponent> all = new();

        var playerRole = player.GetComponent<RoleComponent>();
        all.Add(playerRole);

        foreach (var bot in bots)
            all.Add(bot.GetComponent<RoleComponent>());

        int seekerIndex = Random.Range(0, all.Count);

        GameRole playerFinalRole = GameRole.None;

        for (int i = 0; i < all.Count; i++)
        {
            GameRole role =
                (i == seekerIndex)
                ? GameRole.Seeker
                : GameRole.Hider;

            pendingRoles.Add(all[i], role);

            if (all[i] == playerRole)
                playerFinalRole = role;
        }

        return playerFinalRole;
    }
    public void ApplyRoles()
    {
        foreach (var pair in pendingRoles)
        {
            pair.Key.SetRole(pair.Value);
        }

        NotifyRolesChanged();
    }
}