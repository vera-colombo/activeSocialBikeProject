using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorPathScripts : MonoBehaviour
{
    // Ray color is the color of our path in the editor
    public Color raycolor = Color.white;

    // Every path object will be stored in this list
    public List<Transform> path_objs = new List<Transform>();

    // 
    private Transform[] theArray;

    // It gives option to draw anything in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = raycolor;
        theArray = GetComponentsInChildren<Transform>();
        path_objs.Clear();

        foreach (Transform path_obj in theArray)
        {
            if (path_obj != this.transform) // We want to make sure that we do not use the parent object
            {
                path_objs.Add(path_obj);
            }
        }

        for (int i = 0; i < path_objs.Count; i++)
        {
            Vector3 position = path_objs[i].position;
            if (i > 0)
            {
                Vector3 previous = path_objs[i - 1].position;
                Gizmos.DrawLine(previous, position);
                Gizmos.DrawWireSphere(position, 0.4f);
            }
        }
    }
}
