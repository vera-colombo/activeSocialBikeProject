using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FCG
{
    public class TFShiftHand : MonoBehaviour
    {

        public GameObject rightHandObjects;
        public GameObject leftHandObjects;
        public GameObject leftHandObjectsJapan;

        public void rightHand(int active)
        {
            rightHandObjects.SetActive(active == 0);
            leftHandObjects.SetActive(active == 1);
            leftHandObjectsJapan.SetActive(active == 2);
        }


    }
}