// BillboardUI.cs
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform target;
    private Camera cam;
    private Vector3 offset;

    private void Awake()
    {
        cam = Camera.main;
        target = transform.parent;
        offset = transform.position - target.position; // lưu offset hiện tại
        transform.SetParent(null); // tách ra khỏi parent
    }

    private void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position
        );
    }
}