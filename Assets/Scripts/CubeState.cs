using System.Collections.Generic;
using UnityEngine;

public class CubeState : MonoBehaviour
{
    public List<GameObject> front = new List<GameObject>();
    public List<GameObject> back = new List<GameObject>();
    public List<GameObject> right = new List<GameObject>();
    public List<GameObject> left = new List<GameObject>();
    public List<GameObject> up = new List<GameObject>();
    public List<GameObject> down = new List<GameObject>();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PickUp(List<GameObject> cubeSide) {
        foreach(GameObject face in cubeSide) {
            // Attach the parent of each face(the little cube)
            // to the parent of the 4th index(the little cube in the middle)
            // Unless it is already the 4th index
            if(face != cubeSide[4]) {
                face.transform.parent.transform.parent = cubeSide[4].transform.parent;
                // start the side rotation logic
            }
        }

        // Start the side rotation logic
        var pivot = cubeSide[4].transform.parent.GetComponent<PivotRotation>();
        if(pivot != null) {
            pivot.Rotate(cubeSide);
        } else {
            Debug.LogWarning($"PivotRotation not found on {cubeSide[4].transform.parent.name}. " +
                "Make sure PivotRotation is attached to the parent of each center cube piece.");
        }
    }
}
