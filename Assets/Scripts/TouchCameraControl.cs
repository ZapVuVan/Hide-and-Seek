using UnityEngine;
using UnityEngine.EventSystems;

public class TouchCameraController : MonoBehaviour, IDragHandler
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private GameObject headPlayer;

    [Header("Sensitivity")]
    [SerializeField] private float sensitivityX = 0.2f;
    [SerializeField] private float sensitivityY = 2f;

    [Header("Vertical Clamp")]
    [SerializeField] private float minY = -70f;
    [SerializeField] private float maxY = 70f;

    private float rotX;
    private float rotY;

    private bool isFirstCam = false;

    public void OnDrag(PointerEventData eventData)
    {
        float mouseX =
            (eventData.delta.x / Screen.width)
            * sensitivityX * 100f;

        float mouseY =
            (eventData.delta.y / Screen.height)
            * sensitivityY * 100f;

        rotY += mouseX;
        rotX -= mouseY;

        rotX = Mathf.Clamp(rotX, minY, maxY);

        orientation.rotation =
            Quaternion.Euler(rotX, rotY, 0f);
    }

    public void TransitionToFirstPerson()
    {
        isFirstCam = true;

        if (headPlayer != null)
            headPlayer.transform.localScale = Vector3.zero;

        rotX = orientation.eulerAngles.x;
        rotY = orientation.eulerAngles.y;
    }

    public void TransitionToThirdPerson()
    {
        isFirstCam = false;

        if (headPlayer != null)
            headPlayer.transform.localScale = Vector3.one;

        rotX = orientation.eulerAngles.x;
        rotY = orientation.eulerAngles.y;
    }
}