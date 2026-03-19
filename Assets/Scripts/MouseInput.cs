using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInput : MonoBehaviour
{
    public static MouseInput instance;

    public delegate void LeftClick();
    public event LeftClick LeftClickPerformed;
    public event LeftClick LeftClickCanceled;

    [SerializeField] private InputAction position, leftClick;

    public Vector2 Position => position.ReadValue<Vector2>();
    public bool IsLeftPressed { get; private set; }

    void Awake()
    {
        instance = this;

        position.Enable();
        leftClick.Enable();

        leftClick.performed += _ =>
        {
            IsLeftPressed = true;
            LeftClickPerformed?.Invoke();
        };
        leftClick.canceled += _ =>
        {
            IsLeftPressed = false;
            LeftClickCanceled?.Invoke();
        };
    }
}

