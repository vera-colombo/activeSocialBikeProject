using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;


public static class ConfigurationParameters
{

    static string path = Application.streamingAssetsPath + "\\configuration.txt"; //"C:\\Progetti\\Goji\\Goji_new\\LevelXMLFile\\configuration.txt";
    static int bikeCom = -1;
    static int bikeBaudRate = -1;
    static int saturation = -1;
    static string bleaddress = string.Empty;

    public static int BikeCOM
    {
        get
        {
            if (bikeCom < 0)
                bikeCom = int.Parse(ReadASetting("COM"));

            return bikeCom;
        }
    }

    public static int BikeBaudRate
    {
        get
        {
            if (bikeBaudRate < 0)
                bikeBaudRate = int.Parse(ReadASetting("BaudRate"));

            return bikeBaudRate;
        }
    }

        
    public static bool SaturationOn
    {
        get
        {
            if (saturation < 0)
                saturation = int.Parse(ReadASetting("Saturation"));

            if (saturation == 1)
                return true;
            else
                return false;
        }
    }



    public static string BLEaddress
    {
        get
        {
            if (bleaddress == string.Empty)
                bleaddress = ReadASetting("BLEaddress");

            return bleaddress;
        }
    }


    public static string ReadASetting(string settingName)
    {
           
        string value = String.Empty;
        string line;

        using (StreamReader sr = new StreamReader(path))
        {
            while ((line = sr.ReadLine()) != null)
            {
                if (line != String.Empty && line[0] != '#' && line[0] != '[')
                {
                    line.Trim();
                    string[] name_and_value = line.Split('=');
                    string stringToCompare = name_and_value[0].Trim();

                    if (String.CompareOrdinal(stringToCompare, settingName) == 0)
                    {
                        value = name_and_value[1].Trim();
                        break;
                    }
                }
            }
        }

        return value;
    }
}

