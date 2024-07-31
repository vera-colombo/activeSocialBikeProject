using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MoveOnPathScript : NetworkBehaviour
{
    // The editorpathscript that creates the path
    public EditorPathScripts pathToFollow;
    // The initial position
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;
    // Current waypoint id = the integer in the array of path points
    public int currentWayPointID;

    public float PlayerSpeed = 2f;
    // The distance between the pivot point of the object and point in the curve. The small the distance the smoother the movement on the path.
    private float reachDistance = 1.0f;

    // The rotation speed on the curve when we are looking at the next point
    public float rotationSpeed = 5.0f;

    // The name of the path that get passed in the script when we are instantiating our game object
    public string pathName;

    public float conversionFactor = 0.05f;

    public CycleErgometerManager cycleErgometerManager;
    [Networked]
    public bool isMoving { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }
    

    public override void FixedUpdateNetwork()
    {
        // Only move own player and not every other player. Each player controls its own player object.
        if (HasStateAuthority == false)
        {
            return;
        }

        // Move the player only if isMoving is true
        if (isMoving)
        {
            //transform.position += PlayerSpeed * transform.forward * Runner.DeltaTime;
            PlayerSpeed = cycleErgometerManager.CurrentRPM * conversionFactor;
                
            float distance = Vector3.Distance(pathToFollow.path_objs[currentWayPointID].position, transform.position);
            transform.position = Vector3.MoveTowards(transform.position, pathToFollow.path_objs[currentWayPointID].position, Runner.DeltaTime * PlayerSpeed);

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
