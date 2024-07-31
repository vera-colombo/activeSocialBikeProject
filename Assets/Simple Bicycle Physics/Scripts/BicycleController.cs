using FluffyUnderware.Curvy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Please use using SBPScripts; directive to refer to or append the SBP library
namespace SBPScripts
{
    // Cycle Geometry Class - Holds Gameobjects pertaining to the specific bicycle
    [System.Serializable]
    public class CycleGeometry
    {
        public GameObject handles, lowerFork, fWheelVisual, RWheel, crank, lPedal, rPedal, fGear, rGear;
    }
    //Pedal Adjustments Class - Manipulates pedals and their positioning.  
    [System.Serializable]
    public class PedalAdjustments
    {
        public float crankRadius;
        public Vector3 lPedalOffset, rPedalOffset;
        public float pedalingSpeed;
    }
    // Wheel Friction Settings Class - Uses Physics Materials and Physics functions to control the static / dynamic slipping of the wheels 

    [System.Serializable]
    public class WheelFrictionSettings
    {
        public PhysicMaterial fPhysicMaterial, rPhysicMaterial;
        public Vector2 fFriction, rFriction;
    }
    // Way Point System Class - Replay Ghosting system
    [System.Serializable]
    public class WayPointSystem
    {
        public enum RecordingState { DoNothing, Record, Playback };
        public RecordingState recordingState = RecordingState.DoNothing;
        [Range(1, 10)]
        public int frameIncrement;
        [HideInInspector]
        public List<Vector3> bicyclePositionTransform;
        [HideInInspector]
        public List<Quaternion> bicycleRotationTransform;
        [HideInInspector]
        public List<Vector2Int> movementInstructionSet;
        [HideInInspector]
        public List<bool> sprintInstructionSet;
        [HideInInspector]
        public List<int> bHopInstructionSet;
    }
    ////[System.Serializable]
    ////public class AirTimeSettings
    ////{
    ////    public bool freestyle;
    ////    public float airTimeRotationSensitivity;
    ////    [Range(0.5f, 10)]
    ////    public float heightThreshold;
    ////    public float groundSnapSensitivity;
    ////}
    public class BicycleController : MonoBehaviour
    {
        public CycleGeometry cycleGeometry;
        public GameObject fPhysicsWheel, rPhysicsWheel;
        public WheelFrictionSettings wheelFrictionSettings;
        // Curve of Power Exerted over Input time by the cyclist
        // This class sets the physics materials on to the tires of the bicycle. F Friction pertains to the front tire friction and R Friction to the rear.
        // They are of the Vector2 type. X field edits the static friction information and Y edits the dynamic friction. Please keep the values over 0.5.
        // For more information, please read the commented scripts.

        public AnimationCurve accelerationCurve;
        [Tooltip("Steer Angle over Speed")]
        public AnimationCurve steerAngle;
        public float axisAngle;
        // Defines the leaning curve of the bicycle
        public AnimationCurve leanCurve;
        // The slider refers to the ratio of Relaxed mode to Top Speed. 
        // Torque is a physics based function which acts as the actual wheel driving force.
        public float torque, topSpeed;
        [Range(0.1f, 0.9f)]
        [Tooltip("Ratio of Relaxed mode to Top Speed")]
        public float relaxedSpeed;
        public float reversingSpeed;
        public Vector3 centerOfMassOffset;
        
        [HideInInspector]
        public bool isReversing;
        public bool isAirborne;
        public bool stuntMode;

        // Controls Cycle sway from left to right.
        // The degree of cycle waddling side to side upon pedaling.
        // Higher values correspond to higher waddling. This property also affects character IK. 

        [Range(0, 8)]
        public float oscillationAmount;
        // Following the natural movement of a cyclist, the oscillation of the cycle from side to side also affects the steering to a certain extent.
        // This value refers to the counter steer upon cycle oscillation. Higher values correspond to a higher percentage of the oscillation being transferred to the steering handles. 

        [Range(0, 1)]
        public float oscillationAffectSteerRatio;
        float oscillationSteerEffect;
        [HideInInspector]
        public float cycleOscillation;
        [HideInInspector]
        public Rigidbody rb, fWheelRb, rWheelRb;
        float turnAngle;
        float xQuat, zQuat;
        [HideInInspector]
        public float crankSpeed, crankCurrentQuat, crankLastQuat, restingCrank;
        public PedalAdjustments pedalAdjustments;
        [HideInInspector]
        public float turnLeanAmount;
        RaycastHit hit;
        [HideInInspector]
        public float customSteerAxis, customLeanAxis, customAccelerationAxis, rawCustomAccelerationAxis;
        bool isRaw, sprint;
        [HideInInspector]
        public bool wheelieInput;
        [HideInInspector]
        public float wheeliePower;
        public bool wheelieToggle;
        [HideInInspector]
        public int bunnyHopInputState;
        [HideInInspector]
        public float currentTopSpeed, pickUpSpeed;
        Quaternion initialLowerForkLocalRotaion, initialHandlesRotation;
        ConfigurableJoint fPhysicsWheelConfigJoint, rPhysicsWheelConfigJoint;
        // Ground Conformity refers to vehicles that do not need a gyroscopic force to keep them upright.
        // For non-gyroscopic wheel systems like the tricycle,
        // enabling ground conformity ensures that the tricycle is not always upright and
        // follows the curvature of the terrain. 
        public bool groundConformity;
        RaycastHit hitGround;
        Vector3 theRay;
        float groundZ;
        JointDrive fDrive, rYDrive, rZDrive;
        // Attempts to Reduce/eliminate bouncing of the bicycle after a fall impact 
        public bool inelasticCollision;
        [HideInInspector]
        public Vector3 lastVelocity, deceleration, lastDeceleration;
        int impactFrames;
        bool isBunnyHopping;
        
        [HideInInspector]
        public float bunnyHopAmount;
        // The upward force the rider can bunny hop with. 
        public float bunnyHopStrength;
        public WayPointSystem wayPointSystem;
        ////public AirTimeSettings airTimeSettings;

        //// New booleans to keep track of pedaling state 
        public bool isPedaling = false;


        //// <summary>
        //// New attempt to follow path


        void Awake()
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        void Start()
        {

            rb = GetComponent<Rigidbody>();
            rb.maxAngularVelocity = Mathf.Infinity;

            fWheelRb = fPhysicsWheel.GetComponent<Rigidbody>();
            fWheelRb.maxAngularVelocity = Mathf.Infinity;

            rWheelRb = rPhysicsWheel.GetComponent<Rigidbody>();
            rWheelRb.maxAngularVelocity = Mathf.Infinity;

            currentTopSpeed = topSpeed;

            initialHandlesRotation = cycleGeometry.handles.transform.localRotation;
            initialLowerForkLocalRotaion = cycleGeometry.lowerFork.transform.localRotation;

            fPhysicsWheelConfigJoint = fPhysicsWheel.GetComponent<ConfigurableJoint>();
            rPhysicsWheelConfigJoint = rPhysicsWheel.GetComponent<ConfigurableJoint>();

            

            //Recording is set to 0 to remove the recording previous data if not set to playback
            //////if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Record || wayPointSystem.recordingState == WayPointSystem.RecordingState.DoNothing)
            //////{
            //////    wayPointSystem.bicyclePositionTransform.Clear();
            //////    wayPointSystem.bicycleRotationTransform.Clear();
            //////    wayPointSystem.movementInstructionSet.Clear();
            //////    wayPointSystem.sprintInstructionSet.Clear();
            //////    wayPointSystem.bHopInstructionSet.Clear();
            //////}
        }

        void FixedUpdate()
        {

            //Physics based Steering Control.
            fPhysicsWheel.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + customSteerAxis * steerAngle.Evaluate(rb.velocity.magnitude) + oscillationSteerEffect, 0);
            fPhysicsWheelConfigJoint.axis = new Vector3(1, 0, 0);

            //Power Control. Wheel Torque + Acceleration curves//

            //cache rb velocity
            float currentSpeed = rb.velocity.magnitude;
            //Debug.Log("velocity: " + currentSpeed);

            ////if (!sprint)
            ////    currentTopSpeed = Mathf.Lerp(currentTopSpeed, topSpeed * relaxedSpeed, Time.deltaTime);
            ////else
            ////    currentTopSpeed = Mathf.Lerp(currentTopSpeed, topSpeed, Time.deltaTime);

            // Torque Application on a Wheel Rigidbody and indicates ROTATIONAL acceleration.
            if (currentSpeed < currentTopSpeed && rawCustomAccelerationAxis > 0)
                rWheelRb.AddTorque(transform.right * torque * customAccelerationAxis);

            // Force Application on a Main Rigidbody and indicates FORWARD acceleration. 
            ////////////if (currentSpeed < currentTopSpeed && rawCustomAccelerationAxis > 0 /*&& !isAirborne && !isBunnyHopping*/)
            ////////////    rb.AddForce(transform.forward * accelerationCurve.Evaluate(customAccelerationAxis));

            
            //////if (currentSpeed < reversingSpeed && rawCustomAccelerationAxis < 0 /*&& !isAirborne && !isBunnyHopping*/)
            //////    rb.AddForce(-transform.forward * accelerationCurve.Evaluate(customAccelerationAxis) * 0.5f);

            ////if (transform.InverseTransformDirection(rb.velocity).z < 0)
            ////    isReversing = true;
            ////else
            ////    isReversing = false;

            ////if (rawCustomAccelerationAxis < 0 && isReversing == false /*&& !isAirborne && !isBunnyHopping*/)
            ////    rb.AddForce(-transform.forward * accelerationCurve.Evaluate(customAccelerationAxis) * 2);

            // Center of Mass handling
            if (stuntMode)
                rb.centerOfMass = GetComponent<BoxCollider>().center;
            else
                rb.centerOfMass = Vector3.zero + centerOfMassOffset;

            //Handles
            ////cycleGeometry.handles.transform.localRotation = Quaternion.Euler(0, customSteerAxis * steerAngle.Evaluate(currentSpeed) + oscillationSteerEffect * 5, 0) * initialHandlesRotation;

            //LowerFork
            //////cycleGeometry.lowerFork.transform.localRotation = Quaternion.Euler(0, customSteerAxis * steerAngle.Evaluate(currentSpeed) + oscillationSteerEffect * 5, customSteerAxis * -axisAngle) * initialLowerForkLocalRotaion;



            //FWheelVisual (keep it, rotation of front wheel)
            xQuat = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            zQuat = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            cycleGeometry.fWheelVisual.transform.rotation = Quaternion.Euler(xQuat * (customSteerAxis * -axisAngle), customSteerAxis * steerAngle.Evaluate(currentSpeed) + oscillationSteerEffect * 5, zQuat * (customSteerAxis * -axisAngle));
            cycleGeometry.fWheelVisual.transform.GetChild(0).transform.localRotation = cycleGeometry.RWheel.transform.rotation;

            //Crank
            crankCurrentQuat = cycleGeometry.RWheel.transform.rotation.eulerAngles.x;
            if (customAccelerationAxis > 0 /*&& !isAirborne && !isBunnyHopping*/)
            {
                crankSpeed += Mathf.Sqrt(customAccelerationAxis * Mathf.Abs(Mathf.DeltaAngle(crankCurrentQuat, crankLastQuat) * pedalAdjustments.pedalingSpeed));
                crankSpeed %= 360;
            }
            else if (Mathf.Floor(crankSpeed) > restingCrank)
                crankSpeed += -6;
            else if (Mathf.Floor(crankSpeed) < restingCrank)
                crankSpeed = Mathf.Lerp(crankSpeed, restingCrank, Time.deltaTime * 5);

            crankLastQuat = crankCurrentQuat;
            cycleGeometry.crank.transform.localRotation = Quaternion.Euler(crankSpeed, 0, 0);

            //Pedals (keep this part pedals rotation)
            cycleGeometry.lPedal.transform.localPosition = pedalAdjustments.lPedalOffset + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * (crankSpeed + 180)) * pedalAdjustments.crankRadius, Mathf.Sin(Mathf.Deg2Rad * (crankSpeed + 180)) * pedalAdjustments.crankRadius);
            cycleGeometry.rPedal.transform.localPosition = pedalAdjustments.rPedalOffset + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * (crankSpeed)) * pedalAdjustments.crankRadius, Mathf.Sin(Mathf.Deg2Rad * (crankSpeed)) * pedalAdjustments.crankRadius);

            //FGear (keep this part)
            if (cycleGeometry.fGear != null)
                cycleGeometry.fGear.transform.rotation = cycleGeometry.crank.transform.rotation;

            //RGear (keep this part)
            if (cycleGeometry.rGear != null)
                cycleGeometry.rGear.transform.rotation = rPhysicsWheel.transform.rotation;

            //CycleOscillation
            if ((/*sprint &&*/ currentSpeed > 5/* && isReversing == false*/) /*|| isAirborne || isBunnyHopping*/)
                pickUpSpeed += Time.deltaTime * 2;
            else
                pickUpSpeed -= Time.deltaTime * 2;

            pickUpSpeed = Mathf.Clamp(pickUpSpeed, 0.1f, 1);

            cycleOscillation = -Mathf.Sin(Mathf.Deg2Rad * (crankSpeed + 90)) * (oscillationAmount * (Mathf.Clamp(currentTopSpeed / currentSpeed, 1f, 1.5f))) * pickUpSpeed;
            turnLeanAmount = -leanCurve.Evaluate(customLeanAxis) * Mathf.Clamp(currentSpeed * 0.1f, 0, 1);
            oscillationSteerEffect = cycleOscillation * Mathf.Clamp01(customAccelerationAxis) * (oscillationAffectSteerRatio * (Mathf.Clamp(topSpeed / currentSpeed, 1f, 1.5f)));



            //FrictionSettings
            //////wheelFrictionSettings.fPhysicMaterial.staticFriction = wheelFrictionSettings.fFriction.x;
            //////wheelFrictionSettings.fPhysicMaterial.dynamicFriction = wheelFrictionSettings.fFriction.y;
            //////wheelFrictionSettings.rPhysicMaterial.staticFriction = wheelFrictionSettings.rFriction.x;
            //////wheelFrictionSettings.rPhysicMaterial.dynamicFriction = wheelFrictionSettings.rFriction.y;

            //////if (Physics.Raycast(fPhysicsWheel.transform.position, Vector3.down, out hit, Mathf.Infinity))
            //////    if (hit.distance < 0.5f)
            //////    {
            //////        Vector3 velf = fPhysicsWheel.transform.InverseTransformDirection(fWheelRb.velocity);
            //////        velf.x *= Mathf.Clamp01(1 / (wheelFrictionSettings.fFriction.x + wheelFrictionSettings.fFriction.y));
            //////        fWheelRb.velocity = fPhysicsWheel.transform.TransformDirection(velf);
            //////    }
            //////if (Physics.Raycast(rPhysicsWheel.transform.position, Vector3.down, out hit, Mathf.Infinity))
            //////    if (hit.distance < 0.5f)
            //////    {
            //////        Vector3 velr = rPhysicsWheel.transform.InverseTransformDirection(rWheelRb.velocity);
            //////        velr.x *= Mathf.Clamp01(1 / (wheelFrictionSettings.rFriction.x + wheelFrictionSettings.rFriction.y));
            //////        rWheelRb.velocity = rPhysicsWheel.transform.TransformDirection(velr);
            //////    }

            //Impact sensing
            ////deceleration = (fWheelRb.velocity - lastVelocity) / Time.fixedDeltaTime;
            ////lastVelocity = fWheelRb.velocity;
            ////impactFrames--;
            ////impactFrames = Mathf.Clamp(impactFrames, 0, 15);
            ////if (deceleration.y > 200 && lastDeceleration.y < -1)
            ////    impactFrames = 30;

            ////lastDeceleration = deceleration;

            ////if (impactFrames > 0 && inelasticCollision)
            ////{
            ////    fWheelRb.velocity = new Vector3(fWheelRb.velocity.x, -Mathf.Abs(fWheelRb.velocity.y), fWheelRb.velocity.z);
            ////    rWheelRb.velocity = new Vector3(rWheelRb.velocity.x, -Mathf.Abs(rWheelRb.velocity.y), rWheelRb.velocity.z);
            ////}

            ////AirControl
            //if (Physics.Raycast(transform.position + new Vector3(0, 1f, 0), Vector3.down, out hit, Mathf.Infinity))
            //{
            //    if (hit.distance > 2f || impactFrames > 0)
            //    {
            //        isAirborne = true;
            //        restingCrank = 100;
            //    }
            //    else if (isBunnyHopping)
            //    {
            //        restingCrank = 100;
            //    }
            //    else
            //    {
            //        isAirborne = false;
            //        restingCrank = 10;
            //    }

            //    // For stunts
            //    // 5f is the snap to ground distance
            //    if (hit.distance > airTimeSettings.heightThreshold && airTimeSettings.freestyle)
            //    {
            //        stuntMode = true;
            //        // Stunt + flips controls (Not available for Waypoint system as of yet)
            //        // You may use Numpad Inputs as well.
            //        rb.AddTorque(Vector3.up * customSteerAxis * 4 * airTimeSettings.airTimeRotationSensitivity, ForceMode.Impulse);
            //        rb.AddTorque(transform.right * rawCustomAccelerationAxis * -3 * airTimeSettings.airTimeRotationSensitivity, ForceMode.Impulse);
            //    }
            //    else
            //        stuntMode = false;
            //}

            // Setting the Main Rotational movements of the bicycle

            //if (airTimeSettings.freestyle)
            //{
            //    if (!stuntMode && isAirborne)
            //        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, turnLeanAmount + cycleOscillation + GroundConformity(groundConformity)), Time.deltaTime * airTimeSettings.groundSnapSensitivity);
            //    else if (!stuntMode && !isAirborne)
            //        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, turnLeanAmount + cycleOscillation + GroundConformity(groundConformity)), Time.deltaTime * 10 * airTimeSettings.groundSnapSensitivity);
            //}
            //else
            //{
            //    //Pre-version 1.5
            //    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, turnLeanAmount + cycleOscillation + GroundConformity(groundConformity));
            //}


            ////Wheelie
            //if (/*!isAirborne && */wheelieInput && rawCustomAccelerationAxis > 0)
            //{
            //    rb.angularDrag = 15;
            //    wheeliePower = customAccelerationAxis * 150 * System.Convert.ToInt32(wheelieToggle);
            //    var rot = Quaternion.FromToRotation(transform.forward, new Vector3(transform.forward.x, 0.75f, transform.forward.z));
            //    rb.AddTorque(new Vector3(rot.x, rot.y, rot.z) * wheeliePower, ForceMode.Acceleration);
            //}
            //else
            //{
            //    rb.angularDrag = 1;
            //}


        }
        void Update()
        {
            ApplyCustomInput();


            ////GetKeyUp/Down requires an Update Cycle
            ////BunnyHopping
            //if (bunnyHopInputState == 1)
            //{
            //    isBunnyHopping = true;
            //    bunnyHopAmount += Time.deltaTime * 8f;
            //}
            //if (bunnyHopInputState == -1)
            //    StartCoroutine(DelayBunnyHop());

            //if (bunnyHopInputState == -1 && !isAirborne)
            //    rb.AddForce(transform.up * bunnyHopAmount * bunnyHopStrength, ForceMode.VelocityChange);
            //else
            //    bunnyHopAmount = Mathf.Lerp(bunnyHopAmount, 0, Time.deltaTime * 8f);

            //bunnyHopAmount = Mathf.Clamp01(bunnyHopAmount);
        }

        //float GroundConformity(bool toggle)
        //{
        //    if (toggle)
        //    {
        //        groundZ = transform.rotation.eulerAngles.z;
        //    }
        //    return groundZ;

        //}


        

        void ApplyCustomInput()
        {
            ////
            ////I added this part earlier but check it is still needed.//////////
            //Toggle pedaling state when the up arrow key is pressed
            //if (Input.GetKeyDown(KeyCode.UpArrow))
            //{
                isPedaling = true;
            //}


            // Set customAccelerationAxis based on isPedaling
            customAccelerationAxis = isPedaling ? 1 : 0;
            rawCustomAccelerationAxis = isPedaling ? 1 : 0;


            /////////////
            //////////////

            //if (wayPointSystem.recordingState == WayPointSystem.RecordingState.DoNothing || wayPointSystem.recordingState == WayPointSystem.RecordingState.Record)
            //{
            //    CustomInput("Horizontal", ref customSteerAxis, 5, 5, false);
            //    CustomInput("Vertical", ref customAccelerationAxis, 1, 1, false);
            //    CustomInput("Horizontal", ref customLeanAxis, 1, 1, false);
            //    CustomInput("Vertical", ref rawCustomAccelerationAxis, 1, 1, true);


            //    //sprint = Input.GetKey(KeyCode.LeftShift);

            //    //wheelieInput = Input.GetKey(KeyCode.LeftControl);

            //    ////Stateful Input - bunny hopping
            //    //if (Input.GetKey(KeyCode.Space))
            //    //    bunnyHopInputState = 1;
            //    //else if (Input.GetKeyUp(KeyCode.Space))
            //    //    bunnyHopInputState = -1;
            //    //else
            //    //    bunnyHopInputState = 0;

            //    //Record
            //    if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Record)
            //    {
            //        if (Time.frameCount % wayPointSystem.frameIncrement == 0)
            //        {
            //            wayPointSystem.bicyclePositionTransform.Add(new Vector3(Mathf.Round(transform.position.x * 100f) * 0.01f, Mathf.Round(transform.position.y * 100f) * 0.01f, Mathf.Round(transform.position.z * 100f) * 0.01f));
            //            wayPointSystem.bicycleRotationTransform.Add(transform.rotation);
            //            wayPointSystem.movementInstructionSet.Add(new Vector2Int((int)Input.GetAxisRaw("Horizontal"), (int)Input.GetAxisRaw("Vertical")));
            //            wayPointSystem.sprintInstructionSet.Add(sprint);
            //            wayPointSystem.bHopInstructionSet.Add(bunnyHopInputState);
            //        }
            //    }
            //}

            //else
            //{
            //    if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Playback)
            //    {
            //        if (wayPointSystem.movementInstructionSet.Count - 1 > Time.frameCount / wayPointSystem.frameIncrement)
            //        {
            //            transform.position = Vector3.Lerp(transform.position, wayPointSystem.bicyclePositionTransform[Time.frameCount / wayPointSystem.frameIncrement], Time.deltaTime * wayPointSystem.frameIncrement);
            //            transform.rotation = Quaternion.Lerp(transform.rotation, wayPointSystem.bicycleRotationTransform[Time.frameCount / wayPointSystem.frameIncrement], Time.deltaTime * wayPointSystem.frameIncrement);
            //            WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].x, ref customSteerAxis, 5, 5, false);
            //            WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].y, ref customAccelerationAxis, 1, 1, false);
            //            WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].x, ref customLeanAxis, 1, 1, false);
            //            WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].y, ref rawCustomAccelerationAxis, 1, 1, true);
            //            sprint = wayPointSystem.sprintInstructionSet[Time.frameCount / wayPointSystem.frameIncrement];
            //            bunnyHopInputState = wayPointSystem.bHopInstructionSet[Time.frameCount / wayPointSystem.frameIncrement];
            //        }
            //    }
            //}
        }

        //Input Manager Controls
        float CustomInput(string name, ref float axis, float sensitivity, float gravity, bool isRaw)
        {
            var r = Input.GetAxisRaw(name);
            var s = sensitivity;
            var g = gravity;
            var t = Time.unscaledDeltaTime;

            if (isRaw)
                axis = r;
            else
            {
                if (r != 0)
                    axis = Mathf.Clamp(axis + r * s * t, -1f, 1f);
                else
                    axis = Mathf.Clamp01(Mathf.Abs(axis) - g * t) * Mathf.Sign(axis);
            }

            return axis;
        }

        

        //float WayPointInput(float instruction, ref float axis, float sensitivity, float gravity, bool isRaw)
        //{
        //    var r = instruction;
        //    var s = sensitivity;
        //    var g = gravity;
        //    var t = Time.unscaledDeltaTime;

        //    if (isRaw)
        //        axis = r;
        //    else
        //    {
        //        if (r != 0)
        //            axis = Mathf.Clamp(axis + r * s * t, -1f, 1f);
        //        else
        //            axis = Mathf.Clamp01(Mathf.Abs(axis) - g * t) * Mathf.Sign(axis);
        //    }

        //    return axis;
        //}

        //IEnumerator DelayBunnyHop()
        //{
        //    yield return new WaitForSeconds(0.5f);
        //    isBunnyHopping = false;
        //    yield return null;
        //}

    }

}