using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.UI;

public class CycleErgometerManager : MonoBehaviour
{
    public string serialPort;

    //public GameObject errGameObj;
    //public GameObject msgGameObj_RPM;

    [SerializeField]
    protected int currentWL;
    protected float speed;
    protected int heartRate = 0;
    protected int heartRate_threshold;
    protected int spO2_threshold;
    protected float spO2 = 100;

    // Cycle ergometer object
    protected CycleErgometerP10 cycleErgometer;

    public bool simulation;
    public int minSimSpeed = 50;
    public int maxSimSpeed = 60;

    public CycleErgometerP10 CycleErgometer
    {
        get
        {
            if (!simulation)
            {
                if (cycleErgometer == null)
                    cycleErgometer = new CycleErgometerP10();

                return cycleErgometer;
            }
            else
                return null;

        }
    }

    // Flag for cycle ergometer data update
    protected bool _enableUpdate = true;
    protected bool isSaturation = false;
    //public Image saturationImage;

    public bool IsSaturation
    {
        get { return isSaturation; }
        protected set
        {
            if (isSaturation != value && !isSaturation)
            {
                isSaturation = true;
                //saturationImage.enabled = true;
            }
        }
    }

    #region public properties to access cycle-ergometer data 

    public int CurrentWorkLoad { get { return currentWL; } }
    public float CurrentRPM { get { return speed; } }
    public int CurrentHeartRate { get { return heartRate; } }
    public float CurrentSpO2 { get { return spO2; } }





    // String with a summary of cycle ergometer data
    System.String GetDataString()
    {
        return System.String.Format("{0}\nSpeed = {1:D3}\nLoad = {2:D3}\nLoad (set) = {3:D3}",
            cycleErgometer.ProductInfo,
            cycleErgometer.CurrSpeed,
            cycleErgometer.CurrLoad,
            cycleErgometer.SetLoad);
    }

    #endregion



    // UI ToggleUpdate check box
    public void ToggleUpdate(bool on)
    {
        _enableUpdate = on;
    }


    // UI ToggleUpdate check box
    public void SetLoad(float val)
    {
        if (!_enableUpdate || CycleErgometer == null)
        {
            Debug.LogError("ERRORE: verificare che il cicloergometro sia acceso e alimentato");
            //errGameObj.GetComponentInChildren<Text>().text = "ERRORE: verificare che il cicloergometro sia acceso e alimentato";
            //errGameObj.SetActive(true);
            return;
        }
        try
        {
            CycleErgometer.SetLoad = (int)val;
        }
        catch (Exception e)
        {
            Debug.LogError("Error during setLoad - Bike manager:  " + e.ToString());
        }
    }

    public void InitCycleergometer()
    {
        if (!CycleErgometer.Start())
        {
            if (cycleErgometer.LastError != null)
                Debug.LogError(cycleErgometer.LastError);

            //errGameObj.SetActive(true);
            //errGameObj.GetComponentInChildren<Text>().text = "Cicloergometro non trovato";
            Debug.LogError("**** Cycle-ergometer error");
            return;
        }

        //errGameObj.SetActive(false);

        isSaturation = ConfigurationParameters.SaturationOn;

        Debug.Log("****Cycle ergometer configured on port " + serialPort);


        cycleErgometer.EnableUpdate(true, true, true, isSaturation);
        Debug.Log("****Cycle-ergometer enable update");

        //if (isSaturation)
        //    saturationImage.enabled = true;

        //heartRate_threshold = GameObject.FindGameObjectWithTag("exercise_managers").GetComponent<ExerciseMananger>().options.HR_threshold;
        //spO2_threshold = GameObject.FindGameObjectWithTag("exercise_managers").GetComponent<ExerciseMananger>().options.SpO2_threshold;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!simulation)
        {
            InitCycleergometer();
        }
        else
        {
            //Debug.LogError("*** setting simulated data");
            float r = UnityEngine.Random.Range(minSimSpeed, maxSimSpeed);

            speed = r;
            Debug.Log("I am " + gameObject.name + " and speed is " + speed.ToString());
            heartRate = 98;
            spO2 = 98;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!simulation)
        {
            if (!_enableUpdate)
                return;

            int ms = (int)(Time.deltaTime * 1000.0f);
            if (ms > 20) ms = 20;// (int)(Time.deltaTime * 1000.0f);

            if (cycleErgometer != null && cycleErgometer.Update(ms))
            {
                // Retrieve data about RPM and heart rate
                speed = cycleErgometer.CurrSpeed;
                heartRate = cycleErgometer.CurrHeartRate;
                spO2 = cycleErgometer.CurrSpO2;
                currentWL = cycleErgometer.CurrLoad;

            }
            else if (cycleErgometer == null)
            {
                Debug.LogError("*** cycle-ergometer is null");
                simulation = true;
            }
            //TODO test only
            if (Input.GetKeyDown(KeyCode.W)) 
            {
                if (cycleErgometer != null) 
                {
                    SetLoad(currentWL + 5);
                    Debug.Log("increase load +5 - now WL is " + cycleErgometer.CurrLoad.ToString());
                }
            }

        }

        else // Simulation
        {

        }
    }
}
