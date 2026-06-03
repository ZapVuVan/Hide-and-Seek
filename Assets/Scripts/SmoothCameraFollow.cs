using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float positionSmooth = 12f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position;

        // Smooth follow (không giật)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            1f / positionSmooth
        );
    }
}