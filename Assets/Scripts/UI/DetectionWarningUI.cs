using UnityEngine;

public class DetectionWarningUI : MonoBehaviour
{
    public static DetectionWarningUI Instance { get; private set; }
    [SerializeField] private GameObject warningIcon;

    private GameObject playerObj;
    private int seekersTargetingMe = 0; // Bộ đếm số lượng Bot đang nhắm vào Player

    private void Awake()
    {
        Instance = this;
        warningIcon.SetActive(false);
    }

    private void Start()
    {
        playerObj = GameObject.FindWithTag("Player");
    }

    /// <summary>
    /// Được gọi bởi Bot khi bắt đầu đổi mục tiêu hoặc mất dấu mục tiêu
    /// </summary>
    /// <param name="isTargeting">true nếu Bot bắt đầu nhắm vào Player, false nếu Bot bỏ mục tiêu</param>
    public void UpdateTargetState(bool isTargeting)
    {
        if (isTargeting)
        {
            seekersTargetingMe++;
        }
        else
        {
            seekersTargetingMe = Mathf.Max(0, seekersTargetingMe - 1); // Không để tụt xuống âm
        }

        EvaluateWarningVisibility();
    }

    private void Update()
    {
        // Kiểm tra an toàn: Nếu Player chết hoặc bị ẩn đi (SetActive(false)), ép buộc reset bộ đếm và tắt UI ngay lập tức
        if (playerObj != null && !playerObj.activeInHierarchy)
        {
            if (seekersTargetingMe > 0 || warningIcon.activeSelf)
            {
                seekersTargetingMe = 0;
                warningIcon.SetActive(false);
            }
            return;
        }

        // Liên tục đánh giá lại trạng thái UI dựa trên số lượng Bot đang target
        EvaluateWarningVisibility();
    }

    private void EvaluateWarningVisibility()
    {
        if (playerObj != null && playerObj.activeInHierarchy && seekersTargetingMe > 0)
        {
            if (!warningIcon.activeSelf) warningIcon.SetActive(true);
        }
        else
        {
            if (warningIcon.activeSelf) warningIcon.SetActive(false);
        }
    }
}