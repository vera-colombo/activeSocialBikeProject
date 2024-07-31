using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.Curvy;

using FluffyUnderware.Curvy.Controllers;
using SBPScripts;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class PathForward : NetworkBehaviour
{
    public SplineController bikeSplineController;  // Reference to the SplineController component

    ////////// to match the speed with pedaling speed
    public float speedScalingFactor = 0.001f;    // Scaling factor to adjust pedaling speed for the spline
    private BicycleController bicycleController;  // Reference to the BicycleController component

    public CycleErgometerManager ergometerManager;

    private bool isCycling = false;

    [Networked]
    public bool isMoving { get; set; }

    private void Awake()
    {
        bikeSplineController.Spline = GameObject.FindGameObjectWithTag("Spline").GetComponent<CurvySpline>();
    }
    public override void Spawned()
    {
        //isCycling = false;
        bikeSplineController.Speed = 0f;
        bikeSplineController.PlayAutomatically = false;

        bicycleController = GetComponent<BicycleController>();
        ergometerManager = GetComponent<CycleErgometerManager>();

        // Initialize cycling state
        if (ergometerManager != null)
        {
            float initialRPM = ergometerManager.CurrentRPM;
            Debug.Log("Initial RPM: " + initialRPM);

            if (initialRPM > 0)
            {
                isCycling = true;
                bikeSplineController.PlayAutomatically = true;
                bikeSplineController.Speed = CalculateSpeedFromRPM(initialRPM);
            }
            else
            {
                isCycling = false;
                bikeSplineController.Speed = 0f;
                bikeSplineController.PlayAutomatically = false;
            }
        }
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
            if (ergometerManager != null)
            {
                float currentRPM = ergometerManager.CurrentRPM;
                Debug.Log("Current RPM: " + currentRPM);

                // Check if RPM is greater than zero to determine if the user is cycling
                if (currentRPM > 0)
                {
                    if (!isCycling)
                    {
                        // Start cycling
                        isCycling = true;
                        bikeSplineController.PlayAutomatically = true;
                    }

                    // Calculate and set the speed based on RPM
                    float adjustedSpeed = CalculateSpeedFromRPM(currentRPM);
                    bikeSplineController.Speed = adjustedSpeed;
                }

                else
                {
                    if (isCycling)
                    {
                        // Stop cycling
                        isCycling = false;
                        bikeSplineController.Speed = 0f;
                        bikeSplineController.PlayAutomatically = false;
                    }
                }

            }
        }
    }

    private float CalculateSpeedFromRPM(float rpm)
    {
        // Example conversion from RPM to speed
        return rpm * speedScalingFactor;
    }
}



/// Original Script
//////    //public GameObject bike;
//////    public SplineController bikeSplineController;
//////    public SplineController targetSplineController;


//////    public BikeManager bikeManager; //recalling script Cicloergometro
//////    //public Text Velocity;

//////    void Start()
//////    {
//////        bikeSplineController.PositionMode = CurvyPositionMode.Relative;
//////    }

//////    // Update is called once per frame
//////    void Update()
//////    {
//////        //a = FindObjectOfType<BikeManager>(); //Value from other script (Arduino/Cicloergometro)
//////        //Velocity.text = a.speed + "\n" + "rpm";
//////        //float newVel = a.speed*0.05f;  
//////        //  Debug.Log("API "+ newVel); //Pars value for console visualization

//////        // var controller = bike.AddComponent<SplineController>();
//////        //Value
//////        bikeSplineController.Speed = bikeManager.CurrentRPM*0.05f;
//////        targetSplineController.Speed = bikeManager.CurrentRPM * 0.05f;
//////    }
//////}