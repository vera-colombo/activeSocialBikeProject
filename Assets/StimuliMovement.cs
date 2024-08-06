using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Controllers;

public class StimuliMovement : NetworkBehaviour
{
    //private SplineController stimuliSplineController;
    public ASBManager asbManager;
    // The editorpathscript that creates the path
    [SerializeField]
    public EditorPathScripts pathToFollow;
    protected float mySpeed;
    //private float speedScalingFactor = 0.001f;
    protected bool moved = false;
    //private bool isCycling = false;
    // Start is called before the first frame update


    // The initial position
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;
    // Current waypoint id = the integer in the array of path points
    public int currentWayPointID;

    
    // The distance between the pivot point of the object and point in the curve. The small the distance the smoother the movement on the path.
    private float reachDistance = 1.0f;

    // The rotation speed on the curve when we are looking at the next point
    public float rotationSpeed = 5.0f;
    public override void Spawned()
    {
        pathToFollow = GameObject.FindGameObjectWithTag("StimuliPath").GetComponent<EditorPathScripts>();
    }
    public override void FixedUpdateNetwork()
    {
        //if (ASBPlayer.LocalPlayer.GetComponent<MoveOnPathScript>().PlayerSpeed > 0)
            if (ASBPlayer.LocalPlayer.GetComponent<CycleErgometerManager>().CurrentRPM > 0)
            {

            mySpeed = ASBPlayer.LocalPlayer.GetComponent<CycleErgometerManager>().CurrentRPM*0.035f;
            Debug.Log("my speed is" + mySpeed);
            float distance = Vector3.Distance(pathToFollow.path_objs[currentWayPointID].position, transform.position);
            transform.position = Vector3.MoveTowards(transform.position, pathToFollow.path_objs[currentWayPointID].position, Runner.DeltaTime * mySpeed);

            var rotation = Quaternion.LookRotation(pathToFollow.path_objs[currentWayPointID].position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Runner.DeltaTime * rotationSpeed);

            if (distance <= reachDistance)
            {
                currentWayPointID++;
            }

            // Loop
            if (currentWayPointID >= pathToFollow.path_objs.Count) // gestire la fine e ripartire dall'inizio
            {
                currentWayPointID = 0;
            }
        }
    }



}
