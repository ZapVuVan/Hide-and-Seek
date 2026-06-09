using UnityEngine;

public class MapTriggerZone : MonoBehaviour
{
    [Header("Map Info")]
    [SerializeField] private string sceneName;
    [SerializeField] private bool isLocked;
    [SerializeField] private int unlockCost;

    public string SceneName => sceneName;
    public bool IsLocked
    {
        get => isLocked;
        set => isLocked = value;
    }

    public int UnlockCost => unlockCost;

    private void Start()
    {
        // Kiểm tra map đã unlock chưa
        if (PlayerPrefs.GetInt($"Map_Unlocked_{sceneName}", 0) == 1)
        {
            isLocked = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log($"[MAP] Enter Trigger: {sceneName}");

        if (MapSelectUI.Instance != null)
        {
            MapSelectUI.Instance.ShowJoinButton(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log($"[MAP] Exit Trigger: {sceneName}");

        if (MapSelectUI.Instance != null)
        {
            MapSelectUI.Instance.HideJoinButton();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();

        if (col == null)
            return;

        Gizmos.color = isLocked ? Color.red : Color.green;

        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
        {
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
#endif
}