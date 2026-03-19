using System.Collections.Generic;
using UnityEngine;

public class PivotRotation : MonoBehaviour
{
    private List<GameObject> activeSide;
    private Vector3 localForward;
    private Vector3 tangentLocal; // tangent in pivot space: direction on face for "positive" rotation (rotates with face)
    private Vector3 mouseRef;
    private float sensitivity = 0.25f;
    private bool isAutoRotating = false;
    private static int autoRotatingCount;

    private Quaternion targetQuaternion;
    private float speed = 300f;

    private ReadCube readCube;
    private CubeState cubeState;
    private Camera mainCam;

    private DragDetection lmbDrag;

    private Vector3 currentMousePosition => (Vector3)lmbDrag.CurrentPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readCube = FindAnyObjectByType<ReadCube>();
        cubeState = FindAnyObjectByType<CubeState>();
        mainCam = Camera.main;

        lmbDrag = DragDetection.Get(DragDetection.Button.LMB);
        lmbDrag.DragCancelled += () => { if (activeSide != null) RotateToRightAngle(); };
    }

    // Update is called once per frame
    void Update()
    {
        if(isAutoRotating)
        {
            AutoRotate();
        }

        if (lmbDrag.IsDragging && activeSide != null && !isAutoRotating)
        {
            SpinSide(activeSide);
        }
    }

    private void SpinSide(List<GameObject> side)
    {
        Vector2 mouseOffset = (Vector2)(currentMousePosition - mouseRef);

        // Tangent in world space (rotates with face); project to screen for consistent feel at any zoom
        Vector3 T_world = transform.TransformDirection(tangentLocal);
        Vector3 pivotWorld = transform.position;
        Vector2 screenOrigin = mainCam.WorldToScreenPoint(pivotWorld);
        Vector2 screenTangentEnd = mainCam.WorldToScreenPoint(pivotWorld + T_world);
        Vector2 screenTangent = screenTangentEnd - screenOrigin;

        float screenTangentLen = screenTangent.magnitude;
        if (screenTangentLen > 0.001f)
        {
            Vector2 screenTangentNorm = screenTangent / screenTangentLen;
            float rotationDelta = Vector2.Dot(mouseOffset, screenTangentNorm) * sensitivity;
            transform.Rotate(localForward, rotationDelta, Space.Self);
        }

        mouseRef = currentMousePosition;
    }


    /// <summary>Clears active side so this pivot stops reacting to drag. Call on all pivots before Rotate() on the one being picked.</summary>
    public void ClearActiveSide()
    {
        activeSide = null;
    }

    public void Rotate(List<GameObject> side, Vector3 hitPointWorld)
    {
        activeSide = side;
        mouseRef = currentMousePosition;

        // Axis to rotate around (from cube center to face center)
        localForward = Vector3.zero - side[4].transform.parent.transform.localPosition;

        // Tangent in face plane at grab point: N × (P - C). Dragging along this direction on screen = positive rotation.
        Vector3 faceCenterWorld = side[4].transform.position;
        Vector3 N_world = transform.TransformDirection(localForward);
        Vector3 T_world = Vector3.Cross(N_world, hitPointWorld - faceCenterWorld);

        if (T_world.sqrMagnitude < 0.0001f)
        {
            // Hit near center: use direction from face to camera projected onto face plane
            Vector3 toCam = (mainCam.transform.position - faceCenterWorld).normalized;
            Vector3 inPlane = toCam - Vector3.Dot(toCam, N_world) * N_world;
            T_world = Vector3.Cross(N_world, inPlane);
        }
        if (T_world.sqrMagnitude >= 0.0001f)
            tangentLocal = transform.InverseTransformDirection(T_world.normalized);
        else
            tangentLocal = Vector3.right; // fallback
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
        autoRotatingCount++;
    }

    /// <summary>True if any face is currently snapping to 90°. Don't start a new rotation until this is false.</summary>
    public static bool IsAnyAutoRotating => autoRotatingCount > 0;

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
            autoRotatingCount--;
        }
    }
}
