using UnityEngine;
using System.Collections;

using Whilefun.FPEKit;
using System;

//
// DemoIdolTrapScript
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class DemoIdolTrapScript : FPEGenericSaveableGameObject {

	[Header("Custom Idol Items")]
	public Material lightOn;
	public Material lightOff;
	public AudioClip stoneScrape;
	public AudioClip trapStartSound;
	public AudioClip trapReleaseSound;

	private GameObject trapTriggerPlate = null;
	private GameObject trapSign = null;
	private GameObject trapLight = null;
	private GameObject trapLightbulb = null;
	private GameObject trapBars = null;
	private Vector3 signPosition = Vector3.zero;
	private Vector3 plateDownPosition = Vector3.zero;
	private Vector3 releasedBarsPosition = Vector3.zero;
	private Vector3 barsLockedPosition = Vector3.zero;

    private enum eTrapState
    {
        IDLE = 0,
        PLATE_MOVING,
        SIGN_LAUGH,
        BARS_RELEASE,
        COMPLETE
    }
    private eTrapState currentTrapState = eTrapState.IDLE;
    private float trapStateCountdown = 0.0f;

	void Awake(){


		trapTriggerPlate = GameObject.Find("TrapTriggerPlate");
		trapSign = GameObject.Find("TrapSign");
		trapLight = GameObject.Find("TrapLight");
		trapLightbulb = GameObject.Find("TrapLightBulb");
		trapBars = GameObject.Find("TrapBars");

		if(!trapTriggerPlate || !trapSign || !trapLight || !trapLightbulb || !trapBars){
			Debug.LogError("DemoIdolScript:: Objects are missing from demoTrap. Did you change or delete the demoTrap prefab?");
		}

		signPosition = trapSign.transform.position;
		signPosition.z -= 0.8f;

        plateDownPosition = trapTriggerPlate.transform.position;
        plateDownPosition.y -= 0.1f;

		barsLockedPosition = trapBars.transform.position;
		releasedBarsPosition = trapBars.transform.position;
		releasedBarsPosition.y -= 3.0f;

		trapBars.transform.position = releasedBarsPosition;

	}

    // This update function handles the base Update call, and does some other fancy custom state and event stuff for the idol
    void Update()
    {


        if (currentTrapState == eTrapState.PLATE_MOVING)
        {

            trapTriggerPlate.transform.position = Vector3.Lerp(trapTriggerPlate.transform.position, plateDownPosition, 2f * Time.deltaTime);
            trapStateCountdown -= Time.deltaTime;

            if (trapStateCountdown <= 0.0f)
            {
                MoveToState(eTrapState.SIGN_LAUGH);
            }

        }
        else if (currentTrapState == eTrapState.SIGN_LAUGH)
        {

            trapSign.transform.position = Vector3.Lerp(trapSign.transform.position, signPosition, 2f * Time.deltaTime);
            trapStateCountdown -= Time.deltaTime;

            if (trapStateCountdown <= 0.0f)
            {
                MoveToState(eTrapState.BARS_RELEASE);
            }

        }
        else if (currentTrapState == eTrapState.BARS_RELEASE)
        {

            trapBars.transform.position = Vector3.Lerp(trapBars.transform.position, releasedBarsPosition, 2f * Time.deltaTime);
            
            trapStateCountdown -= Time.deltaTime;

            if (trapStateCountdown <= 0.0f)
            {
                MoveToState(eTrapState.COMPLETE);
            }

        }

	}
	
    public void idolPickedUp()
    {

        // When the idol is picked up, let's set off a trap
        if (currentTrapState == eTrapState.IDLE)
        {
            MoveToState(eTrapState.PLATE_MOVING);

        }

    }

    private void setBoxColliderState(bool collidersEnabled)
    {

        BoxCollider[] childColliders = trapBars.GetComponentsInChildren<BoxCollider>();

        foreach(BoxCollider bc in childColliders)
        {
            bc.enabled = collidersEnabled;
        }

    }

    public void idolReturnEvent()
    {
        gameObject.GetComponent<FPEInteractablePickupScript>().interactionString = "It's the artifact I returned. Nearly died for this thing.";
    }

    private void MoveToState(eTrapState state)
    {

        currentTrapState = state;

        switch (state)
        {

            case eTrapState.IDLE:
                break;

            case eTrapState.PLATE_MOVING:
                trapBars.transform.position = barsLockedPosition;
                //trapBars.GetComponent<BoxCollider>().enabled = true;
                setBoxColliderState(true);
                trapStateCountdown = 2.5f;
                trapTriggerPlate.GetComponent<AudioSource>().clip = stoneScrape;
                trapTriggerPlate.GetComponent<AudioSource>().Play();
                break;

            case eTrapState.SIGN_LAUGH:
                trapBars.transform.position = barsLockedPosition;
                //trapBars.GetComponent<BoxCollider>().enabled = true;
                setBoxColliderState(true);
                trapTriggerPlate.transform.position = plateDownPosition;
                trapLight.GetComponent<Light>().color = Color.red;
                trapLightbulb.GetComponent<MeshRenderer>().material = lightOff;
                trapStateCountdown = 3.0f;
                trapTriggerPlate.GetComponent<AudioSource>().clip = trapStartSound;
                trapTriggerPlate.GetComponent<AudioSource>().Play();
                break;

            case eTrapState.BARS_RELEASE:
                trapBars.transform.position = barsLockedPosition;
                //trapBars.GetComponent<BoxCollider>().enabled = true;
                setBoxColliderState(true);
                trapSign.transform.position = signPosition;
                trapStateCountdown = 2.0f;
                trapLight.GetComponent<Light>().enabled = false;
                trapTriggerPlate.GetComponent<AudioSource>().clip = trapReleaseSound;
                trapTriggerPlate.GetComponent<AudioSource>().Play();
                break;

            case eTrapState.COMPLETE:
                //trapBars.GetComponent<BoxCollider>().enabled = false;
                setBoxColliderState(false);
                trapBars.transform.position = releasedBarsPosition;
                break;

            default:
                Debug.LogError("DemoIdolScript.MoveToState():: Given bad state '"+ state + "'.");
                break;

        }

    }

    public override FPEGenericObjectSaveData getSaveGameData()
    {
        return new FPEGenericObjectSaveData(gameObject.name, (int)currentTrapState, trapStateCountdown, false);
    }

    public override void restoreSaveGameData(FPEGenericObjectSaveData data)
    {
        MoveToState((eTrapState)data.SavedInt);
    }

}
