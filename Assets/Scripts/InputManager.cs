using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance { get; private set; }

    private bool _jumpPressed;
    private bool _switchPressed;

    [SerializeField] private Joystick moveJoystick;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public Vector2 GetMoveInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE

        Vector2 keyboardInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        Vector2 joystickInput = new Vector2(
            moveJoystick.Horizontal,
            moveJoystick.Vertical
        );

        return keyboardInput.sqrMagnitude > 0.01f
            ? keyboardInput
            : joystickInput;

#else

        return new Vector2(
            moveJoystick.Horizontal,
            moveJoystick.Vertical
        );

#endif
    }

    public void OnJumpPressed()
    {
        _jumpPressed = true;
    }

    public void OnSwitchPressed()
    {
        _switchPressed = true;
    }

    public bool GetJumpInput()
    {
        bool val = _jumpPressed;
        _jumpPressed = false;
        return val;
    }

    public bool GetSwitchInput()
    {
        bool val = _switchPressed;
        _switchPressed = false;
        return val;
    }
}