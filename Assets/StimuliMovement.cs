using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class StimuliMovement : NetworkBehaviour
{
    public ASBManager asbManager;
    protected float mySpeed;
    protected bool moved = false;
    // Start is called before the first frame update

    public override void FixedUpdateNetwork()
    {
        if (moved) 
        {
            transform.position += mySpeed * transform.forward * Runner.DeltaTime;
        }
    }
    // Update is called once per frame
    public void Move(float speed)
    {
        moved = true;
        mySpeed = speed;
    }
}
