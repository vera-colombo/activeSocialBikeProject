using System;
using System.IO.Ports;
using System.Threading;
using System.Text;
using UnityEngine;

/// <summary>
/// Cycle ergometer P10 protocol data link
/// </summary>
public class CycleErgometerP10
{
    private SerialPort serialPort;

    private bool isStarted = false;
    private int commPhase = 0;
    private int readTries = 0;

    private bool getSpeed = true;
    private bool getLoad = false;
    private bool getHeartRate = false;
    private bool getSpO2 = false;
    private string productInfo;
    private int loadSet = 0;

    /// <summary>
    /// The last read speed value.
    /// </summary>
    public int CurrSpeed { get; private set; }
    /// <summary>
    /// The last value for the current load (in Watt)
    /// </summary>
    public int CurrLoad { get; private set; }
    /// <summary>
    /// The current heart rate beats/min
    /// </summary>
    public int CurrHeartRate { get; private set; }

    /// <summary>
    /// The current heart rate beats/min
    /// </summary>
    public int CurrSpO2 { get; private set; }

    /// <summary>
    /// The value set for the load (in Watt)
    /// </summary>
    public int SetLoad
    {
        get
        {
            if (loadSet == 0) serialPort.WriteLine("G\r");
            return loadSet;
        }
        set
        {
            loadSet = value;
            serialPort.WriteLine(String.Format("W{0:D3}\r", loadSet));
            serialPort.WriteLine("G\r");
        }
    }

    /// <summary>
    /// The last error (empty string or serial port error)
    /// </summary>
    public string LastError { get; private set; }

    /// <summary>
    /// The product info text (available after the first successful call to Update())
    /// </summary>
    public string ProductInfo
    {
        get
        {
            if (productInfo == "") serialPort.WriteLine("I\r");
            return productInfo;
        }
    }

    /// <summary>
    /// Create a cycle ergometer with default settings (COM1 port)
    /// </summary>
    public CycleErgometerP10()
    {
        Init();
    }

    /// <summary>
    /// Create a cycle ergometer setting the given serial port
    /// </summary>
    /// <param name="serialPortName">COM port number (e.g. "COM1", etc.)</param>
    public CycleErgometerP10(string serialPortName)
    {
        Init();
    }

    ////// Destructor: stop the cycle ergometer (close the serial port)
    ////~CycleErgometerP10()
    ////{
    ////    if (isStarted) Stop();
    ////}

    /// <summary>
    /// Create a new SerialPort object with default settings.
    /// </summary>
    private void Init()
    {
        serialPort = new SerialPort();

        // Configure Sanabike
        if (ConfigurationParameters.BikeCOM != -1)
            serialPort.PortName = "COM" + ConfigurationParameters.BikeCOM.ToString();
        else
        {
            Debug.Log("Cannot read port from configuration file");
            serialPort.PortName = "COM3";
        };

        if (ConfigurationParameters.BikeBaudRate != -1)
            serialPort.BaudRate = ConfigurationParameters.BikeBaudRate;
        else
        {
            Debug.Log("Cannot read baud rate from configuration file");
            serialPort.BaudRate = 9600;
        };

        //serialPort.BaudRate = 4800;// 9600;//4800;//
        serialPort.Parity = Parity.None;
        serialPort.DataBits = 8;
        serialPort.StopBits = StopBits.One;
        serialPort.Handshake = Handshake.None;

        // Set the read/write timeouts
        serialPort.ReadTimeout = 5;
        serialPort.WriteTimeout = 10;
    }

    /// <summary>
    /// Set which values must be queried when calling Update()
    /// </summary>
    /// <param name="getSpeed">Get the current speed</param>
    /// <param name="getLoad">Get the current load</param>
    /// <param name="getHeartRate">Get the current heart rate</param>
    public void EnableUpdate(bool getSpeed, bool getLoad, bool getHeartRate, bool getSpO2)
    {
        this.getSpeed = getSpeed;
        this.getLoad = getLoad;
        this.getHeartRate = getHeartRate;
        if (ConfigurationParameters.SaturationOn)
            this.getSpO2 = getSpO2;
        else
            this.getSpO2 = false;
    }

    /// <summary>
    /// Update the data: read data and query new data (get pending data until the given time is elapsed)
    /// </summary>
    /// <param name="maxMs">Try for the given time in milliseconds when no data is received</param>
    /// <returns>True if new data was received, false otherwise.</returns>
    public bool Update(int maxMs)
    {
        if (!isStarted) return false;
        int iterations = maxMs / serialPort.ReadTimeout;
        if (iterations < 1) iterations = 1;
        switch (commPhase)
        {
            case 0:
                if (!ReadData())
                {
                    readTries++;
                    if (readTries < iterations) return false;
                    readTries = 0;
                }
                commPhase++;
                return true;

            case 1:
                if (getSpeed) serialPort.WriteLine("D\r");
                if (getLoad) serialPort.WriteLine("B\r");
                if (getHeartRate) serialPort.WriteLine("H\r");
                if (getSpO2) serialPort.WriteLine("L\r");
                commPhase++;
                break;

            case 2:
                bool result = false;
                for (int count = 0; count < iterations; count++)
                {
                    if (!ReadData())
                    {
                        commPhase = 1;
                        break;
                    }
                    else result = true;
                }
                return result;

            case 3:
                break;
        }
        return false;
    }

    /// <summary>
    /// Read and decode incoming data
    /// </summary>
    /// <returns>True if any data was read, false otherwise.</returns>
    public bool ReadData()
    {
        if (!isStarted) return false;

        string reply;
        try
        {
            reply = serialPort.ReadTo("\r");
            if (reply != "")
            {
                if (reply.Length == 4)
                {
                    int cmd = reply[0];
                    string param = reply.Substring(1);
                    int val;
                    if (!int.TryParse(param, out val))
                    {
                        return false;
                    }
                    switch (cmd)
                    {
                        case 'n':
                            CurrSpeed = val;
                            break;
                        case 'B':
                            CurrLoad = val;
                            break;
                        case 'G':
                            loadSet = val;
                            break;
                        case 'H':
                            CurrHeartRate = val;
                            break;
                    }
                }

                else if (reply.StartsWith("sanabike"))
                {
                    productInfo = reply;
                }


                else
                {
                    int cmd = reply[0];
                    if (cmd == 'L')
                    {
                        string param = reply.Substring(1, 3);
                        int val;
                        if (!int.TryParse(param, out val))
                        {
                            return false;
                        }
                        CurrSpO2 = val;
                    }
                }
            }
        }
        catch (TimeoutException)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Open the serial port and query the product info
    /// </summary>
    /// <returns></returns>
    public bool Start()
    {
        try
        {
            serialPort.Open();
        }
        catch (System.SystemException e)
        {
            LastError = "Error opening serial port :" + e.Message;
            return false;
        }
        isStarted = true;
        serialPort.WriteLine("I\r");

        return true;
    }

    /// <summary>
    /// Stop the cycle ergometer (close serial port)
    /// </summary>
    public void Stop()
    {
        if (isStarted) serialPort.Close();
        isStarted = false;
    }
}