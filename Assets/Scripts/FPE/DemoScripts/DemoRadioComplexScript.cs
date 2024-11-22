using UnityEngine;

using Whilefun.FPEKit;

//
// DemoRadioComplexScript
// This script demonstrates a more complex version
// of making an Activate type object. This example
// shows how to play an animation, sounds, turn on
// lights, change materials, and update interaction
// string to reflect object state.
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
[RequireComponent(typeof (AudioSource))]
public class DemoRadioComplexScript : MonoBehaviour {

	// These are all custom variables for the demoRadioComplex prefab
	public AudioClip switchOn;
	public AudioClip switchOff;
	public Material radioIlluminated;
	public Material radioRegular;
	private GameObject switchSound = null;
	private Light radioLight;
	private bool radioOn = false;
	private Light[] radioLights;
	private float lightFlickerInterval = 1.0f;
	private float lightFlickerCounter = 0.0f;
	private Transform powerKnob = null;
	private Quaternion powerOffKnobRotation = Quaternion.identity;
	private Quaternion powerOnKnobRotation = Quaternion.identity;

	void Awake(){

		radioLights = gameObject.GetComponentsInChildren<Light>();
		Transform[] children = gameObject.GetComponentsInChildren<Transform>();

		foreach (Transform t in children)
        {

			if(t.name == "RightKnob")
            {
				powerKnob = t;
			}
            else if(t.name == "SwitchClick")
            {
				switchSound = t.gameObject;
			}

		}

		powerOffKnobRotation = powerKnob.transform.rotation;
		powerOnKnobRotation = powerKnob.transform.rotation * Quaternion.Euler(new Vector3(50.0f,0.0f,0.0f));

	}

	void Update()
    {
	
		if(radioOn)
        {

			powerKnob.transform.rotation = Quaternion.Slerp(powerKnob.transform.rotation, powerOnKnobRotation, 0.5f);

			if(lightFlickerCounter <= 0.0f)
            {

				foreach(Light l in radioLights)
                {

					if(l.name == "RadioLight2")
                    {
						l.range = Random.Range(2.2f, 2.65f);
					}

				}

				lightFlickerCounter = lightFlickerInterval;

			}
            else
            {
				lightFlickerCounter--;
			}

		}
        else
        {
			powerKnob.transform.rotation = Quaternion.Slerp(powerKnob.transform.rotation, powerOffKnobRotation, 0.5f);
		}

	}

    public void turnRadioOn()
    {

        radioOn = true;

        foreach (Light l in radioLights)
        {
            l.enabled = true;
        }

        gameObject.GetComponent<FPEInteractableBaseScript>().interactionString = "Turn off complex radio";
        lightFlickerCounter = lightFlickerInterval;
        switchSound.GetComponent<AudioSource>().clip = switchOn;
        switchSound.GetComponent<AudioSource>().Play();
        gameObject.GetComponent<AudioSource>().Play();

        MeshRenderer[] cmr = gameObject.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer m in cmr)
        {

            if (m.name == "Radio")
            {
                m.material = radioIlluminated;
            }

        }

    }


    public void turnRadioOff()
    {

        radioOn = false;

        foreach (Light l in radioLights)
        {
            l.enabled = false;
        }

        gameObject.GetComponent<FPEInteractableBaseScript>().interactionString = "Turn on complex radio";
        switchSound.GetComponent<AudioSource>().clip = switchOff;
        switchSound.GetComponent<AudioSource>().Play();
        gameObject.GetComponent<AudioSource>().Stop();

        MeshRenderer[] cmr = gameObject.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer m in cmr)
        {

            if (m.name == "Radio")
            {
                m.material = radioRegular;
            }

        }

    }

}
