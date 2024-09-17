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
    //protected Quaternion initialRotation;
    //// Current waypoint id = the integer in the array of path points
    //public int currentWayPointID;

    
    //// The distance between the pivot point of the object and point in the curve. The small the distance the smoother the movement on the path.
    //private float reachDistance = 1.0f;

    //// The rotation speed on the curve when we are looking at the next point
    //public float rotationSpeed = 5.0f;

    protected LTSpline spline;
    public GameObject player;
    protected float iter = 0;
    protected List<Vector3> pathElements; // The cubes to be interpolated along the path
    private float stimuli_speed = 2f;
    public override void Spawned()
    {
        pathToFollow = GameObject.FindGameObjectWithTag("StimuliPath").GetComponent<EditorPathScripts>();
        GetComponent<PathSpline_LT>().CreatePath(pathToFollow.path_objs);
        InitStimuliOnPath();
    }

    public void InitStimuliOnPath()
    {
        pathElements = this.GetComponent<PathSpline_LT>().cubeList;

        // Place the bike in the starting position
        transform.position = pathElements[0];

        // Create the spline
        spline = new LTSpline(pathElements.ToArray());
    }
    public override void FixedUpdateNetwork()
    {
        //if (ASBPlayer.LocalPlayer.GetComponent<MoveOnPathScript>().PlayerSpeed > 0)
            if (ASBPlayer.LocalPlayer.GetComponent<CycleErgometerManager>().CurrentRPM > 0)
            {

            // Only move own player and not every other player. Each player controls its own player object.
            if (HasStateAuthority == false)
            {
                return;
            }

            
                //transform.position += PlayerSpeed * transform.forward * Runner.DeltaTime;
                //PlayerSpeed = cycleErgometerManager.CurrentRPM * conversionFactor;
                //transform.position += PlayerSpeed * transform.forward * Runner.DeltaTime;
                spline.place(transform, iter);
                List<float> ss = new List<float>();
                foreach(ASBPlayer p in ASBPlayer.ASBPlayerRefs) 
                {
                    ss.Add(p.GetComponent<CycleErgometerManager>().CurrentRPM);
                    Debug.Log(ss.ToString());
                }
                float r = System.Linq.Enumerable.Average(ss);
                Debug.Log("r " + r.ToString());
                //float r = (s1 + s2) / 2;
                stimuli_speed = r;
                //Debug.Log(s1.ToString() + "-" + s2.ToString() + "-" + r);
                stimuli_speed = 0.01f * (ASBPlayer.LocalPlayer.GetComponent<CycleErgometerManager>().CurrentRPM / 500);
                Debug.Log("stimuli speed " + stimuli_speed);
                iter += Runner.DeltaTime * stimuli_speed;
            
                if (iter >= 1)
                {
                    transform.position = pathElements[0];
                    iter = 0;
                }
            


            //mySpeed = ASBPlayer.LocalPlayer.GetComponent<CycleErgometerManager>().CurrentRPM*0.035f;
            //Debug.Log("my speed is" + mySpeed);
            //float distance = Vector3.Distance(pathToFollow.path_objs[currentWayPointID].position, transform.position);
            //transform.position = Vector3.MoveTowards(transform.position, pathToFollow.path_objs[currentWayPointID].position, Runner.DeltaTime * mySpeed);

            //var rotation = Quaternion.LookRotation(pathToFollow.path_objs[currentWayPointID].position - transform.position);
            //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Runner.DeltaTime * rotationSpeed);

            //if (distance <= reachDistance)
            //{
            //    currentWayPointID++;
            //}

            //// Loop
            //if (currentWayPointID >= pathToFollow.path_objs.Count) // gestire la fine e ripartire dall'inizio
            //{
            //    currentWayPointID = 0;
            //}
        }
    }



}
