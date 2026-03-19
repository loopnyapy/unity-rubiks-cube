using System.Collections.Generic;
using UnityEngine;

public class SelectFace : MonoBehaviour
{
    CubeState cubeState;
    ReadCube readCube;

    int layerMask = 1 << 8;

    private DragDetection lmbDrag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readCube = FindAnyObjectByType<ReadCube>();
        cubeState = FindAnyObjectByType<CubeState>();

        lmbDrag = DragDetection.Get(DragDetection.Button.LMB);
        lmbDrag.PressPerformed += OnLeftMousePressed;
    }

    void OnLeftMousePressed()
    {
        readCube.ReadState();

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(lmbDrag.CurrentPosition);
        if(Physics.Raycast(ray, out hit, 100.0f, layerMask))
        {
            GameObject face = hit.collider.gameObject;

            List<List<GameObject>> cubeSides = new List<List<GameObject>>()
            {
                cubeState.up,
                cubeState.down,
                cubeState.left,
                cubeState.right,
                cubeState.front,
                cubeState.back,
            };

            // if the face hit exists within a side
            foreach(List<GameObject> cubeSide in cubeSides) {
                if(cubeSide.Contains(face)) {
                    cubeState.PickUp(cubeSide);
                }
            }
        }
    }
}
