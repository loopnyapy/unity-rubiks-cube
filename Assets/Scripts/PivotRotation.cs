using System.Collections.Generic;
using UnityEngine;

public class PivotRotation : MonoBehaviour
{
    private List<GameObject> activeSide;
    private Vector3 localForward;
    private Vector3 mouseRef;
    private float sensitivity = 0.25f;
    private bool isAutoRotating = false;
    private Quaternion targetQuaternion;
    private float speed = 300f;

    private ReadCube readCube;
    private CubeState cubeState;

    private DragDetection lmbDrag;

    private Vector3 currentMousePosition => (Vector3)lmbDrag.CurrentPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readCube = FindAnyObjectByType<ReadCube>();
        cubeState = FindAnyObjectByType<CubeState>();

        lmbDrag = DragDetection.Get(DragDetection.Button.LMB);
        lmbDrag.DragCancelled += () => RotateToRightAngle();
    }

    // Update is called once per frame
    void Update()
    {
        if(isAutoRotating)
        {
            AutoRotate();
        }

        if (lmbDrag.IsDragging && activeSide != null)
        {
            SpinSide(activeSide);
        }
    }

    private void SpinSide(List<GameObject> side)
    {
        // current mouse position minus the last mouse position
        Vector3 mouseOffset = currentMousePosition - mouseRef;

        // Rotate around the computed axis for this side
        if (side == cubeState.front)
        {
            transform.Rotate(localForward, (mouseOffset.x + mouseOffset.y) * sensitivity * -1f, Space.Self);
        }
        if (side == cubeState.back)
        {
            transform.Rotate(localForward, (mouseOffset.x + mouseOffset.y) * sensitivity * -1f, Space.Self);
        }
        if (side == cubeState.up)
        {
            transform.Rotate(localForward, (mouseOffset.x + mouseOffset.y) * sensitivity * 1f, Space.Self);
        }
        if (side == cubeState.down)
        {
            transform.Rotate(localForward, (mouseOffset.x + mouseOffset.y) * sensitivity * 1f, Space.Self);
        }
        if (side == cubeState.left)
        {
            transform.Rotate(localForward, (mouseOffset.x + mouseOffset.y) * sensitivity * -1f, Space.Self);
        }
        if (side == cubeState.right)
        {
            transform.Rotate(localForward, (mouseOffset.x + mouseOffset.y) * sensitivity * 1f, Space.Self);
        }

        // store mouse
        mouseRef = currentMousePosition;
    }


    public void Rotate(List<GameObject> side)
    {
        activeSide = side;
        mouseRef = currentMousePosition;

        // Create a vector to rotate around
        localForward = Vector3.zero - side[4].transform.parent.transform.localPosition;
    }

    public void RotateToRightAngle()
    {
        print("Rotating initiated");
        Vector3 vec = transform.localEulerAngles;
        // round vec to nearest 90 degrees
        vec.x = Mathf.Round(vec.x / 90) * 90;
        vec.y = Mathf.Round(vec.y / 90) * 90;
        vec.z = Mathf.Round(vec.z / 90) * 90;

        targetQuaternion.eulerAngles = vec;
        isAutoRotating = true;
    }

    private void AutoRotate()
    {
        var step = speed * Time.deltaTime;
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetQuaternion, step);

        // if within one degree, set angle to target angle and end the rotation
        if(Quaternion.Angle(transform.localRotation, targetQuaternion) <= 1)
        {
            transform.localRotation = targetQuaternion;
            // unparent the little cubes
            readCube.ReadState();

            isAutoRotating = false;
        }
    }
}
