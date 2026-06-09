using UnityEngine;

public class LadderTrigger : MonoBehaviour
{
    private Collider ladderCollider;

    private void Awake()
    {
        ladderCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                // Truyền kèm Collider của thang vào để tính toán đỉnh thang
                movement.SetClimbing(true, ladderCollider);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.SetClimbing(false, null);
            }
        }
    }
}