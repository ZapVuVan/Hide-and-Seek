// KeepLocalPosition.cs
using UnityEngine;

public class KeepLocalPosition : MonoBehaviour
{
    private Vector3 initialLocalPosition;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        initialLocalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        transform.localPosition = initialLocalPosition;
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position
        );
    }
}