using UnityEngine;
using UnityEngine.UI;

namespace YourGame.Debugging
{
    /// <summary>
    /// Gan script nay vao 1 nut Button bat ky (HUD, menu, lobby...) de cong tien cheat.
    /// Hoan toan doc lap voi RoleCheatPanel - dat o dau, scene nao cung duoc.
    /// </summary>
    public class AddMoneyCheatButton : MonoBehaviour
    {
        [Tooltip("De trong se tu lay Button tren chinh GameObject nay")]
        public Button button;

        [Tooltip("So tien cong moi lan bam")]
        public int amount = 9999;

        private void Start()
        {
            if (DebugCheatManager.Instance == null || !DebugCheatManager.Instance.CheatsEnabled)
            {
                gameObject.SetActive(false);
                return;
            }

            if (button == null) button = GetComponent<Button>();
            button.onClick.AddListener(() => DebugCheatManager.Instance.RequestAddMoney(amount));
        }
    }
}   