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

    public GameRole GenerateRoles(PlayerController player, List<BotController> bots, GameRole? forcedPlayerRole = null)
    {
        pendingRoles.Clear();
        List<RoleComponent> all = new();
        var playerRoleComp = player.GetComponent<RoleComponent>();
        all.Add(playerRoleComp);
        foreach (var bot in bots)
            all.Add(bot.GetComponent<RoleComponent>());

        GameRole playerFinalRole;
        bool hasForcedRole = forcedPlayerRole.HasValue && forcedPlayerRole.Value != GameRole.None;

        if (hasForcedRole)
        {
            playerFinalRole = forcedPlayerRole.Value;
            pendingRoles.Add(playerRoleComp, playerFinalRole);

            var others = all.Where(r => r != playerRoleComp).ToList();

            if (playerFinalRole == GameRole.Seeker)
            {
                // Player la Seeker, tat ca con lai la Hider
                foreach (var r in others)
                    pendingRoles.Add(r, GameRole.Hider);
            }
            else
            {
                // Player la Hider, van random 1 bot trong so con lai lam Seeker
                int seekerIndex = others.Count > 0 ? Random.Range(0, others.Count) : -1;
                for (int i = 0; i < others.Count; i++)
                    pendingRoles.Add(others[i], i == seekerIndex ? GameRole.Seeker : GameRole.Hider);
            }
        }
        else
        {
            // Logic random binh thuong nhu cu, khong doi gi
            int seekerIndex = Random.Range(0, all.Count);
            playerFinalRole = GameRole.None;
            for (int i = 0; i < all.Count; i++)
            {
                GameRole role = (i == seekerIndex) ? GameRole.Seeker : GameRole.Hider;
                pendingRoles.Add(all[i], role);
                if (all[i] == playerRoleComp)
                    playerFinalRole = role;
            }
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