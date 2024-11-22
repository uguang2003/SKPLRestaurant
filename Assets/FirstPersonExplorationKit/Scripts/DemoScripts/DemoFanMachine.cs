using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Whilefun.FPEKit;

//
// DemoFanMachine
// This script demonostrates how to use the Activate type in combination with 
// the Inventory type. The machine prefab is configured to require a battery
// be placed in the machine before it will turn on.
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class DemoFanMachine :  FPEGenericSaveableGameObject {

    public Transform mySwitch;
    public Transform fanBlade;
    public float mySwitchMotion = 0.05f;
    public MeshRenderer fanPowerLight;
    public MeshRenderer fanBatteryLight;
    public GameObject battery;
    public Material fanPowerLightOn;
    public Material fanPowerLightOff;
    public Material fanBatteryLightOn;
    public Material fanBatteryLightOff;
    public AudioClip errorLightBlink;

    private bool fanBatteryLightLit = false;
    private bool hasBattery = false;
    private float failDelay = 0.5f;
    private float failCountdown = 0.0f;
    private bool fanFailure = false;
    private bool fanSwitchIsOn = false;
    private bool fanTurning = false;
    private float fanAcceleration = 1.2f;
    private float fanDeceleration = 0.99f;
    private float fanSpeed = 0.0f;
    private float maxFanSpeed = 1000.0f;

    void Start () {

        fanPowerLight.material = fanPowerLightOff;
        fanBatteryLight.material = fanBatteryLightOff;
        battery.SetActive(false);

    }
	
	void Update () {

        if (fanFailure)
        {

            failCountdown -= Time.deltaTime;

            if (failCountdown <= 0.0f)
            {

                failCountdown = failDelay;
                fanBatteryLightLit = !fanBatteryLightLit;

                if (fanBatteryLightLit)
                {
                    fanBatteryLight.material = fanBatteryLightOn;
                    gameObject.GetComponent<AudioSource>().PlayOneShot(errorLightBlink);
                }
                else
                {
                    fanBatteryLight.material = fanBatteryLightOff;
                }


            }

        }

        if (fanTurning)
        {

            if(fanSpeed < fanAcceleration)
            {
                fanSpeed = fanAcceleration;
            }

            fanBlade.transform.Rotate(new Vector3(fanSpeed * Time.deltaTime, 0f, 0f));

            if (fanSpeed < maxFanSpeed)
            {
                fanSpeed *= fanAcceleration;
            }
            else
            {
                fanSpeed = maxFanSpeed;
            }

            fanBlade.GetComponent<AudioSource>().volume = (fanSpeed / maxFanSpeed);

        }
        else
        {
            
            if (fanSpeed > 0.05f)
            {

                fanBlade.transform.Rotate(new Vector3(fanSpeed * Time.deltaTime, 0f, 0f));
                fanSpeed *= fanDeceleration;
                fanBlade.GetComponent<AudioSource>().volume = (fanSpeed / maxFanSpeed);

            }
            else
            {

                fanBlade.GetComponent<AudioSource>().Stop();
                fanSpeed = 0.0f;
                fanBlade.GetComponent<AudioSource>().Stop();

            }

        }

        

    }


    public void turnOnFan()
    {

        if (!fanSwitchIsOn)
        {

            mySwitch.transform.Translate(new Vector3(0f, mySwitchMotion, 0f));
            fanSwitchIsOn = true;

            if (hasBattery)
            {
                fanTurning = true;
                fanBlade.GetComponent<AudioSource>().Play();
                fanPowerLight.material = fanPowerLightOn;
            }
            else
            {
                fanFailed();
            }

        }


    }

    public void turnOffFan()
    {

        if (fanSwitchIsOn)
        {

            mySwitch.transform.Translate(new Vector3(0f, -mySwitchMotion, 0f));
            fanSwitchIsOn = false;

            fanTurning = false;
            fanPowerLight.material = fanPowerLightOff;

            fanFailure = false;
            fanBatteryLightLit = false;
            fanBatteryLight.material = fanBatteryLightOff;

        }

    }

    public void placeBattery()
    {

        hasBattery = true;
        battery.SetActive(true);

        if (fanSwitchIsOn)
        {

            fanFailure = false;
            fanBatteryLightLit = false;
            fanBatteryLight.material = fanBatteryLightOff;
            gameObject.GetComponent<AudioSource>().PlayOneShot(errorLightBlink);
            fanTurning = true;
            fanBlade.GetComponent<AudioSource>().Play();
            fanPowerLight.material = fanPowerLightOn;

        }

    }

    public void fanFailed()
    {

        fanFailure = true;
        failCountdown = failDelay;
        fanBatteryLightLit = true;
        fanBatteryLight.material = fanBatteryLightOn;
        gameObject.GetComponent<AudioSource>().PlayOneShot(errorLightBlink);

    }

    public override FPEGenericObjectSaveData getSaveGameData()
    {
        return new FPEGenericObjectSaveData(gameObject.name, ((fanSwitchIsOn)?1:0), fanSpeed, hasBattery);
    }

    public override void restoreSaveGameData(FPEGenericObjectSaveData data)
    {

        fanSpeed = data.SavedFloat;
        hasBattery = data.SavedBool;
        fanSwitchIsOn = ((data.SavedInt == 1) ? true : false);

        if (hasBattery)
        {
            placeBattery();
        }

    }

}
