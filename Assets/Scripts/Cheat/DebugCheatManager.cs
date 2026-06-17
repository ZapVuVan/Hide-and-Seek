using System;
using UnityEngine;

namespace YourGame.Debugging
{
    public enum DebugRole
    {
        None,   // Choi binh thuong, khong ep role
        Hider,
        Seeker
    }

    /// <summary>
    /// Singleton quan ly cac cheat dung cho tester.
    /// Gan script nay vao 1 GameObject duy nhat o scene dau tien (Bootstrap/Lobby).
    /// Se tu DontDestroyOnLoad nen dung xuyen suot cac scene.
    /// </summary>
    public class DebugCheatManager : MonoBehaviour
    {
        public static DebugCheatManager Instance { get; private set; }

        [Header("Cau hinh cheat")]
        [Tooltip("Bo ON de tat ca cheat hoat dong. Bo OFF (vd: trong build ban that) de tat het, khong can sua code o noi khac.")]
        public bool CheatsEnabled = true;

        public DebugRole ForcedRole { get; private set; } = DebugRole.None;

        [Tooltip("True khi tester da bam 1 trong 3 nut tren RoleCheatPanel (ke ca 'Choi binh thuong'). GameManager se cho cho den khi cai nay = true moi chia role.")]
        public bool HasChosenRole { get; private set; } = false;

        // Cac he thong khac (RoleManager, CurrencyManager...) lang nghe 2 event nay
        public static event Action<int> OnAddMoneyRequested;
        public static event Action<DebugRole> OnForcedRoleChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Neu tat cheat hoan toan thi se khong co panel nao hien len de bam,
            // nen coi nhu da "chon" luon de GameManager khong bi cho mai.
            if (!CheatsEnabled) HasChosenRole = true;
        }

        public void SetForcedRole(DebugRole role)
        {
            if (!CheatsEnabled) return;
            ForcedRole = role;
            HasChosenRole = true;
            OnForcedRoleChanged?.Invoke(role);
            Debug.Log($"[CHEAT] Forced role set to: {role}");
        }

        public void RequestAddMoney(int amount)
        {
            if (!CheatsEnabled) return;
            OnAddMoneyRequested?.Invoke(amount);
            Debug.Log($"[CHEAT] Requested +{amount} money");
        }
    }
}