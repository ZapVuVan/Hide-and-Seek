using UnityEngine;

public class SyncCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private void LateUpdate()
    {
        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
        GetComponent<Camera>().fieldOfView = mainCamera.fieldOfView;
    }
}