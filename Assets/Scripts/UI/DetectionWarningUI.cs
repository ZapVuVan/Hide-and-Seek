using UnityEngine;

public class DetectionWarningUI : MonoBehaviour
{
    public static DetectionWarningUI Instance { get; private set; }
    [SerializeField] private GameObject warningIcon;

    private void Awake()
    {
        Instance = this;
        warningIcon.SetActive(false);
    }

    public void SetDetected(bool detected)
    {
        warningIcon.SetActive(detected);
    }
}