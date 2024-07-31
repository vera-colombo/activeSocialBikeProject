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
        //stimuliSplineController = GetComponent<SplineController>();
        //stimuliSplineController.Spline = GameObject.FindGameObjectWithTag("Spline").GetComponent<CurvySpline>();
        pathToFollow = GameObject.FindGameObjectWithTag("StimuliPath").GetComponent<EditorPathScripts>();
    }
    public override void FixedUpdateNetwork()
    {
        if (ASBPlayer.LocalPlayer.GetComponent<MoveOnPathScript>().PlayerSpeed > 0) 
        {
            //// Calculate and set the speed based on RPM
            //float adjustedSpeed = CalculateSpeedFromRPM(mySpeed);
            //stimuliSplineController.Speed = adjustedSpeed;
            //if (mySpeed > 0)
            //{
            //    if (!isCycling)
            //    {
            //        // Start cycling
            //        isCycling = true;
            //        stimuliSplineController.PlayAutomatically = true;
            //    }

            //}
            //else
            //{
            //    if (isCycling)
            //    {
            //        // Stop cycling
            //        isCycling = false;
            //        stimuliSplineController.Speed = 0f;
            //        stimuliSplineController.PlayAutomatically = false;
            //    }
            //}

            //transform.position += mySpeed * transform.forward * Runner.DeltaTime;
            mySpeed = ASBPlayer.LocalPlayer.GetComponent<MoveOnPathScript>().PlayerSpeed - 0.4f;
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
    // Update is called once per frame
    //public void Move(float speed)
    //{
    //    moved = true;
    //    mySpeed = speed;        
    //}


}
