using UnityEngine;
using System.Collections.Generic;

public class PathSpline_LT : MonoBehaviour
{

    public Transform[] cubeTransforms;
    
    public GameObject path;
    public List<Vector3> cubeList;

    public void CreatePath(List<Transform> pathElements)
    {
        Debug.Log("create path");
        cubeTransforms = pathElements.ToArray();
        for (int i = 0; i < cubeTransforms.Length; i++)
        {
            Vector3 pos = cubeTransforms[i].position;

            if (i == 0)
            {
                cubeList.Add(pos);
            }
            if (i == cubeTransforms.Length - 1)
            {
                cubeList.Add(pos);
                cubeList.Add(cubeTransforms[0].position);
                cubeList.Add(pos);
            }
            else
            {
                cubeList.Add(pos);
            }
        }
        //cr = new LTSpline(cubeList.ToArray());
        float dist = Vector3.Distance(transform.position, cubeTransforms[1].position);

    }
}