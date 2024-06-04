using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;

public class PlayerMovement : NetworkBehaviour
{
    
    public float PlayerSpeed = 2f;
    [Tooltip("Which answer did the player choose.  0 is always the correct answer, but the answers are randomized locally.")]
    [Networked]
    public bool isMoving { get; set; }


    public override void FixedUpdateNetwork()
    {
        // Only move own player and not every other player. Each player controls its own player object.
        if (HasStateAuthority == false)
        {
            return;
        }
        transform.position += PlayerSpeed * transform.forward * Runner.DeltaTime;      
    }
}
