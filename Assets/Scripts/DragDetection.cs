using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragDetection : MonoBehaviour
{
    public enum Button { LMB, RMB, MMB }

    [SerializeField] private Button button;

    private static Dictionary<Button, DragDetection> instances = new();

    public static DragDetection Get(Button b) => instances[b];

    public delegate void Drag(Vector2 delta);
    public delegate void Pressed();
    public delegate void Canceled();
    public event Drag DragPerformed;
    public event Pressed DragPressed;
    public event Canceled DragCancelled;
    public event Pressed PressPerformed;
    public bool IsDragging;

    [SerializeField] private InputAction position, press;

    public Vector2 CurrentPosition => position.ReadValue<Vector2>();
    private Vector2 previousPosition;

    void Awake()
    {
        instances[button] = this;

        position.Enable();
        press.Enable();

        press.performed += _ =>
        {
            IsDragging = true;
            previousPosition = CurrentPosition;
            PressPerformed?.Invoke();
        };
        press.canceled += _ =>
        {
            IsDragging = false;
            DragCancelled?.Invoke();
        };
    }

    void Update()
    {
        if (!IsDragging) return;

        Vector2 delta = CurrentPosition - previousPosition;
        DragPerformed?.Invoke(delta);
        DragPressed?.Invoke();
        previousPosition = CurrentPosition;
    }

    void OnDestroy()
    {
        if (instances.ContainsKey(button) && instances[button] == this)
            instances.Remove(button);
    }
}