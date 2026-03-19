using UnityEngine;
using System.Collections;
using Vector2Extensions;

public class RotateBigCube : MonoBehaviour
{
    // TODO get current object instead of Passing Down Dependency
    public GameObject target;

    public float swipeSpeed = 400f;
    public float dragSpeed = 0.1f;

    // Start is called after all Awake methods, so instances are guaranteed to be set
    void Start()
    {
        SwipeDetection.instance.SwipePerformed += context => { Swipe(context); };

        DragDetection.Get(DragDetection.Button.MMB).DragPerformed += delta => { Drag(delta); };
    }

    // Update is called once per frame
    void Update()
    {
        if (DragDetection.Get(DragDetection.Button.MMB).IsDragging) return;

        if (transform.rotation != target.transform.rotation)
        {
            var step = swipeSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target.transform.rotation, step);
        }
    }

    void Drag(Vector2 delta)
    {
        // Old way, from tutorial - turning the cube like swipe
        // Vector2 cappedDelta = delta * dragSpeed;
        // transform.rotation = Quaternion.Euler(cappedDelta.y, -cappedDelta.x, 0) * transform.rotation;
        // New way, turning the cube around camera's right and up axes
        Camera cam = Camera.main;
        float angleX = delta.y * dragSpeed;
        float angleY = -delta.x * dragSpeed;
        Quaternion rotX = Quaternion.AngleAxis(angleX, cam.transform.right);
        Quaternion rotY = Quaternion.AngleAxis(angleY, cam.transform.up);
        transform.rotation = rotX * rotY * transform.rotation;
    }

    void Swipe(Vector2 direction)
    {
        if (direction == Vector2.left)
        {
            target.transform.Rotate(0, 90, 0, Space.World);
        }
        if (direction == Vector2.right)
        {
            target.transform.Rotate(0, -90, 0, Space.World);
        }
        if (direction == Vector2Extension.upRight)
        {
            target.transform.Rotate(0, 0, -90, Space.World);
        }
        if (direction == Vector2Extension.upLeft)
        {
            target.transform.Rotate(90, 0, 0, Space.World);
        }
        if (direction == Vector2Extension.downRight)
        {
            target.transform.Rotate(-90, 0, 0, Space.World);
        }
        if (direction == Vector2Extension.downLeft)
        {
            target.transform.Rotate(0, 0, 90, Space.World);
        }
    }
}
