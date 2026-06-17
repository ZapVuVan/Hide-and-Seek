using UnityEngine;
using UnityEngine.UI;

namespace YourGame.Debugging
{
    /// <summary>
    /// Panel chon role hien thi NGAY TU DAU (man hinh dau tien khi mo app/vao lobby),
    /// truoc khi vao tran: Choi binh thuong / Hider / Seeker.
    /// Khong dung phim tat vi test tren dien thoai khong co ban phim.
    /// Sau khi nguoi dung cham chon 1 trong 3 nut, panel tu an di va game
    /// tiep tuc chay luong binh thuong (vao lobby/matchmaking nhu cu),
    /// chi khac la role se duoc ap theo cai da chon.
    /// </summary>
    public class RoleCheatPanel : MonoBehaviour
    {
        [Header("3 nut chon role - keo Button tuong ung vao day")]
        public Button btnPlayNormal;
        public Button btnForceHider;
        public Button btnForceSeeker;

        private void Start()
        {
            if (DebugCheatManager.Instance == null || !DebugCheatManager.Instance.CheatsEnabled)
            {
                gameObject.SetActive(false);
                return;
            }

            // Panel se hien san (active) tu luc scene load, khong can bam phim gi de mo
            btnPlayNormal.onClick.AddListener(() => ChooseRole(DebugRole.None));
            btnForceHider.onClick.AddListener(() => ChooseRole(DebugRole.Hider));
            btnForceSeeker.onClick.AddListener(() => ChooseRole(DebugRole.Seeker));
        }

        private void ChooseRole(DebugRole role)
        {
            DebugCheatManager.Instance.SetForcedRole(role);
            gameObject.SetActive(false); // an panel, tu day game chay tiep nhu binh thuong
        }
    }
}