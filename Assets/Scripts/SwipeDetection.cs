using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeDetection : MonoBehaviour
{
    public static SwipeDetection instance;

    public delegate void Swipe(Vector2 direction);
    public event Swipe SwipePerformed;
    [SerializeField] private InputAction position, press;

    private Vector2 currentPosition => position.ReadValue<Vector2>();
    private Vector2 initialPosition;

    [SerializeField] private float swipeResistance = 100;

    void Awake()
    {
        instance = this;

        position.Enable();
        press.Enable();

        press.performed += _ => { initialPosition = currentPosition; };
        press.canceled += _ => DetectSwipe();
    }

    void DetectSwipe()
    {
        Vector2 delta = currentPosition - initialPosition;

        Vector2 direction = Vector2.zero;
        if (Math.Abs(delta.x) > swipeResistance)
        {
            direction.x = Mathf.Clamp(delta.x, -1, 1);
        }
        if (Math.Abs(delta.y) > swipeResistance)
        {
            direction.y = Mathf.Clamp(delta.y, -1, 1);
        }

        if (direction != Vector2.zero && SwipePerformed != null)
        {
            SwipePerformed(direction);
        }
    }
}
