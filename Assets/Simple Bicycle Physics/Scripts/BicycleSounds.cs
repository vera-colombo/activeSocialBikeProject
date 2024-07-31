using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

//namespace SBPScripts
//{


//    public class BicycleSounds : MonoBehaviour
//    {
//        public AudioSource pedallingAudioSource, freeWheelAudioSource, bodyHitAudioSource, tyreHitFAudioSource, tyreHitRAudioSource;
//        float fHitCoolDown,rHitCoolDown,bHitCoolDown, hopCoolDown;
//        AudioMixer mixer;
//        BicycleController bicycleController;
//        // Start is called before the first frame update
//        void Start()
//        {
//            pedallingAudioSource = pedallingAudioSource.GetComponent<AudioSource>();
//            freeWheelAudioSource = freeWheelAudioSource.GetComponent<AudioSource>();
//            tyreHitFAudioSource = tyreHitFAudioSource.GetComponent<AudioSource>();
//            bicycleController = FindObjectOfType<BicycleController>();
//        }

        
//        // Update is called once per frame
//        void Update()
//        {
//            if (bicycleController.rb.velocity.magnitude > 2f)
//            {
//                if (bicycleController.rawCustomAccelerationAxis > 0 && !bicycleController.isAirborne)
//                {
//                    pedallingAudioSource.pitch = Mathf.Clamp(1 + bicycleController.rb.velocity.magnitude * 0.05f,1,3);
//                    pedallingAudioSource.volume = 0.5f + bicycleController.customAccelerationAxis;
//                }

//                else
//                {
//                    pedallingAudioSource.volume -= Time.deltaTime;
//                    pedallingAudioSource.volume = Mathf.Clamp(pedallingAudioSource.volume, 0.2f, 1);
//                }
//                if (bicycleController.rawCustomAccelerationAxis < 1 || bicycleController.isAirborne)
//                {
//                    freeWheelAudioSource.pitch = Mathf.Clamp(1 + bicycleController.rb.velocity.magnitude * 0.05f, 1,3);
//                    freeWheelAudioSource.volume += Time.deltaTime * 2;
//                }
//                else
//                {
//                    freeWheelAudioSource.volume -= Time.deltaTime * 2;
//                }
//            }
//            else
//            {
//                pedallingAudioSource.volume = bicycleController.rb.velocity.magnitude * 0.25f;
//                freeWheelAudioSource.volume = bicycleController.rb.velocity.magnitude * 0.25f;
//            }

//            if(bicycleController.bunnyHopInputState == 1)
//            {
//                hopCoolDown = 1;
//            }
//            hopCoolDown -=Time.deltaTime*5;
//            hopCoolDown = Mathf.Clamp01(hopCoolDown);

//            if(bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude > 500 && fHitCoolDown == 0 && hopCoolDown == 0)
//            {
//                tyreHitFAudioSource.Play();
//                tyreHitFAudioSource.pitch = Mathf.Clamp(bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 1000, 0.75f,1.5f);
//                tyreHitFAudioSource.volume = Mathf.Clamp(bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 5000, 0 ,0.2f);
//                fHitCoolDown = 1;
//            }
//            fHitCoolDown -=Time.deltaTime;
//            fHitCoolDown = Mathf.Clamp01(fHitCoolDown);

//            if(bicycleController.rWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude > 500 && rHitCoolDown == 0 && hopCoolDown == 0)
//            {
//                tyreHitRAudioSource.Play();
//                tyreHitRAudioSource.pitch = Mathf.Clamp(bicycleController.rWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 1000, 0.75f,1.5f);
//                tyreHitRAudioSource.volume = Mathf.Clamp(bicycleController.rWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 5000,0,0.2f);
//                rHitCoolDown = 1;
//            }
//            rHitCoolDown -=Time.deltaTime;
//            rHitCoolDown = Mathf.Clamp01(rHitCoolDown);

//            if(bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude > 1000 && bHitCoolDown == 0)
//            {
//                bodyHitAudioSource.Play();
//                bodyHitAudioSource.pitch = bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 1000;
//                bodyHitAudioSource.volume = bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 1000;
//                bHitCoolDown = 1;
//            }
//            bHitCoolDown -=Time.deltaTime;
//            bHitCoolDown = Mathf.Clamp01(bHitCoolDown);
            
            





//        }
//    }
//}
