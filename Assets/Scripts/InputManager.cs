using System.Collections;
using System.Collections.Generic;
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

    public void Update()
    {
        
    }
    public Vector2 GetMoveInput()
    {
        return new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical);
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

}
