using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Whilefun.FPEKit;

//
// DemoSecuritySystem
// A basic implementation of a security system used to control external locks on doors
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
[RequireComponent(typeof(AudioSource))]
public class DemoSecuritySystem : MonoBehaviour
{

    [SerializeField, Tooltip("The door(s) the security system will lock or unlock")]
    private FPEDoor[] doorsToControl = null;
    [SerializeField, Tooltip("The code required to open the door (e.g. 0451)")]
    private int[] doorCode = { 0, 4, 5, 1 };
    [SerializeField, Tooltip("Sound that plays when correct code is entered")]
    private AudioClip correctCodeSound = null;
    [SerializeField, Tooltip("Sound that plays when incorrect code is entered")]
    private AudioClip incorrectCodeSound = null;
    [SerializeField, Tooltip("Material for Neutral status light")]
    private Material statusLightNeutral = null;
    [SerializeField, Tooltip("Material for OK status light")]
    private Material statusLightOkay = null;
    [SerializeField, Tooltip("Material for Error status light")]
    private Material statusLightError = null;

    private AudioSource securitySpeaker = null;
    private int numberOfDigitsEntered = 0;
    private int[] digits = new int[4];
    private Text displayText = null;
    private MeshRenderer statusLight = null;
    private bool haveResult = false;
    private float resultDuration = 1.5f;
    private float resultCounter = 0.0f;

    void Start()
    {

        displayText = transform.Find("DisplayCanvas/DisplayBackground/DisplayText").GetComponent<Text>();

        if (!displayText)
        {
            Debug.LogError("DemoSecuritySystem:: '" + gameObject.name + "' has no 'DisplayText' child object. Display will not work.", gameObject);
        }

        securitySpeaker = gameObject.GetComponent<AudioSource>();
        if (!securitySpeaker)
        {
            Debug.LogError("DemoSecuritySystem:: '" + gameObject.name + "'  has no AudioSource. Securty System will not make sounds.", gameObject);
        }

        statusLight = transform.Find("StatusLight").GetComponent<MeshRenderer>();
        if (!statusLight)
        {
            Debug.LogError("DemoSecuritySystem:: '" + gameObject.name + "' has no StatusLight child or it is missing a MeshRenderer.", gameObject);
        }

        if(doorsToControl.Length == 0)
        {
            Debug.LogError("DemoSecuritySystem:: '"+gameObject.name+"' has no doors assigned to control. This security system won't do anything.", gameObject);
        }

        refreshDisplay();

    }


    void Update()
    {

        if (haveResult)
        {

            resultCounter -= Time.deltaTime;

            if (resultCounter <= 0.0f)
            {
                haveResult = false;
                resultCounter = resultDuration;
                statusLight.material = statusLightNeutral;
            }

        }

    }

    public void EnterDigit(int nextDigit)
    {

        digits[numberOfDigitsEntered] = nextDigit;
        numberOfDigitsEntered++;

        refreshDisplay();

        if (numberOfDigitsEntered == 4)
        {
            
            if(digits[0] == doorCode[0] && digits[1] == doorCode[1] && digits[2] == doorCode[2] && digits[3] == doorCode[3])
            {
                correctCodeEntered();
            }
            else
            {
                incorrectCodeEntered();
            }

            numberOfDigitsEntered = 0;

        }

    }

    private void correctCodeEntered()
    {

        statusLight.material = statusLightOkay;
        securitySpeaker.PlayOneShot(correctCodeSound);
        haveResult = true;
        resultCounter = resultDuration;
        unlockDoors();

    }

    private void incorrectCodeEntered()
    {

        statusLight.material = statusLightError;
        securitySpeaker.PlayOneShot(incorrectCodeSound);
        haveResult = true;
        resultCounter = resultDuration;
        numberOfDigitsEntered = 0;

    }

    private void refreshDisplay()
    {

        displayText.text = "";

        for (int i = 0; i < numberOfDigitsEntered; i++)
        {
            displayText.text += "" + digits[i];
        }

    }

    private void unlockDoors()
    {

        foreach (FPEDoor d in doorsToControl)
        {

            if (d != null)
            {

                bool success = d.ExternallyUnlockDoor();

                if (success == false)
                {
                    // TODO: Your code here if you make a door that cannot be unlocked externally
                }

            }

        }

    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {

        if(doorsToControl != null)
        {

            Color c = Color.cyan;
            Gizmos.color = c;

            foreach(FPEDoor d in doorsToControl)
            {

                if (d != null)
                {
                    Gizmos.DrawLine(transform.position, d.transform.position);
                }

            }

        }
       
    }

#endif

}
