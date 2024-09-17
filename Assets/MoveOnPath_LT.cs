using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MoveOnPath_LT : NetworkBehaviour
{
    protected LTSpline spline;
    public GameObject player;
    protected float iter = 0;
    protected List<Vector3> pathElements; // The cubes to be interpolated along the path

    public float conversionFactor = 0.007f;
    public CycleErgometerManager cycleErgometerManager;

    [Networked]
    public bool isMoving { get; set; }

   

    private float player_speed = 2f;

    public float PlayerSpeed
    {
        get { return player_speed; }
    }

    // Start is called before the first frame 
    public override void Spawned()
    {
        base.Spawned();
    }

    public void InitPlayerOnPath()
    {
        pathElements = this.GetComponent<PathSpline_LT>().cubeList;

        // Place the bike in the starting position
        transform.position = pathElements[0];

        // Create the spline
        spline = new LTSpline(pathElements.ToArray());
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
            Debug.Log("is moving");
            //transform.position += PlayerSpeed * transform.forward * Runner.DeltaTime;
            //PlayerSpeed = cycleErgometerManager.CurrentRPM * conversionFactor;
            //transform.position += PlayerSpeed * transform.forward * Runner.DeltaTime;
            spline.place(transform, iter);
            player_speed = conversionFactor * (cycleErgometerManager.CurrentRPM / 500);
            iter += Runner.DeltaTime * PlayerSpeed;

            if (iter >= 1) 
            {
                transform.position = pathElements[0];
                iter = 0;
            }
        }



    }

    
}

