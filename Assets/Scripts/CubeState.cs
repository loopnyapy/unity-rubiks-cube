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

    public void PickUp(List<GameObject> cubeSide, Vector3 hitPointWorld) {
        foreach(GameObject face in cubeSide) {
            // Attach the parent of each face(the little cube)
            // to the parent of the 4th index(the little cube in the middle)
            // Unless it is already the 4th index
            if(face != cubeSide[4]) {
                face.transform.parent.transform.parent = cubeSide[4].transform.parent;
                // start the side rotation logic
            }
        }

        // Clear active side on all pivots so only the one we pick reacts to drag
        foreach (var center in new[] { front[4], back[4], left[4], right[4], up[4], down[4] })
        {
            var p = center.transform.parent.GetComponent<PivotRotation>();
            if (p != null) p.ClearActiveSide();
        }

        var pivot = cubeSide[4].transform.parent.GetComponent<PivotRotation>();
        if(pivot != null) {
            pivot.Rotate(cubeSide, hitPointWorld);
        } else {
            Debug.LogWarning($"PivotRotation not found on {cubeSide[4].transform.parent.name}. " +
                "Make sure PivotRotation is attached to the parent of each center cube piece.");
        }
    }
}
