using UnityEngine;

public class HiderPingIcon : MonoBehaviour
{
    [SerializeField] private GameObject iconVisual; // kéo Quad vào đây

    private void Awake()
    {
        iconVisual.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!iconVisual.activeSelf) return;

        // Billboard về camera hiện tại
        transform.LookAt(
            transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up
        );
    }

    public void Show() => iconVisual.SetActive(true);
    public void Hide() => iconVisual.SetActive(false);
}