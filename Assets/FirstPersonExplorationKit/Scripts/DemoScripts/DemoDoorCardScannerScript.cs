using UnityEngine;

using Whilefun.FPEKit;

// Ignoring known deprecation notice related to DemoSlidingDoorScript not being maintained beyond v2.2
#pragma warning disable 0618

//
// DemoDoorCardScannerScript
// This script demonostrates how to apply the generic "Activate" type
// to do things that are interesting and relevant to your game world.
// This example shows how you can manage a simple object state to react
// to different door states.
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class DemoDoorCardScannerScript : FPEInteractableActivateScript {

    [Header("Additional Stuff for Card Reader")]
	[SerializeField]
	private Material lockedMaterial = null;
	[SerializeField]
	private Material errorMaterial = null;
	[SerializeField]
	private Material unlockedMaterial = null;

	[SerializeField]
	private AudioClip doorUnlockFailure = null;
	[SerializeField]
	private AudioClip doorUnlockSuccess = null;

	private GameObject statusLight = null;
	private Light lightSource = null;

	private float errorLightTime = 0.0f;
	private float errorLightBlinkInterval = 0.2f;

	private float doorOpenDelay = 0.0f;

    public override void Awake()
    {

        // Always call back to base class Awake function
        base.Awake();
        statusLight = transform.parent.Find("doubleSlidingDoor/StatusLight").gameObject;
        lightSource = transform.Find("StatusLightSource").GetComponent<Light>();

        statusLight.GetComponent<Renderer>().material = lockedMaterial;
        lightSource.color = Color.red;

    }

    void Update()
    {

        if (errorLightTime > 0.0f)
        {

            errorLightTime -= Time.deltaTime;

            if (errorLightBlinkInterval > 0.0f)
            {

                errorLightBlinkInterval -= Time.deltaTime;

                if (errorLightBlinkInterval <= 0.0f)
                {

                    lightSource.enabled = !lightSource.enabled;
                    errorLightBlinkInterval = 0.2f;

                    if (statusLight.GetComponent<Renderer>().material == errorMaterial)
                    {

                        statusLight.GetComponent<Renderer>().material = lockedMaterial;
                        lightSource.color = Color.red;

                    }
                    else
                    {

                        statusLight.GetComponent<Renderer>().material = errorMaterial;
                        lightSource.color = Color.yellow;

                    }

                }

            }

            if (errorLightTime <= 0.0f)
            {

                lightSource.enabled = true;
                statusLight.GetComponent<Renderer>().material = lockedMaterial;
                lightSource.color = Color.red;

            }

        }

        if (doorOpenDelay > 0.0f)
        {

            doorOpenDelay -= Time.deltaTime;

            if (doorOpenDelay <= 0.0f)
            {
                gameObject.transform.parent.GetComponent<DemoSlidingDoorScript>().unlockTheDoor();
            }

        }

    }

    public void attemptToOpen()
    {
        
        if (gameObject.transform.parent.GetComponent<DemoSlidingDoorScript>().isDoorLocked())
        {

            doorOpenDelay = 1.0f;

            errorLightTime = 0.0f;
            statusLight.GetComponent<Renderer>().material = unlockedMaterial;
            lightSource.enabled = true;
            lightSource.color = Color.green;

            gameObject.GetComponent<AudioSource>().clip = doorUnlockSuccess;
            gameObject.GetComponent<AudioSource>().Play();

            interactionString = "It's unlocked";

        }

    }

    public void cardError()
    {

        errorLightTime = 3.0f;
        gameObject.GetComponent<AudioSource>().clip = doorUnlockFailure;
        gameObject.GetComponent<AudioSource>().Play();
        interactionString = "I need a key card";

    }

    public void setLockLightColor(bool locked)
    {

        lightSource.enabled = true;

        if (locked)
        {
            lightSource.color = Color.red;
            statusLight.GetComponent<Renderer>().material = lockedMaterial;
        }
        else
        {
            lightSource.color = Color.green;
            statusLight.GetComponent<Renderer>().material = unlockedMaterial;
        }

    }

}
