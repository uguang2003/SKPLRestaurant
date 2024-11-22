using UnityEngine;

using Whilefun.FPEKit;

//
// DemoSwitchActivationScript
// This script demonostrates how to apply the generic "Activate" type
// to do things that are interesting and relevant to your game world.
// This example shows how you can play an animation, sound, and change 
// lights and materials when the player interacts with this object.
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class DemoSwitchActivationScript : FPEInteractableActivateScript {

    [Header("Additional Demo Switch Stuff")]
	public Material lightOnMaterial;
	public Material lightOffMaterial;
	private Transform childSwitch = null;
	private bool buttonActionInProgress = false;
	private GameObject demoSwitchLightBulb = null;
	private GameObject demoSwitchLightSource = null;

	public override void Awake(){

		// Always call back to base class Awake function
		base.Awake();

		// Some stuff specific to our switch and how it behaves
		Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform t in childTransforms) {
			if(t.name == "testSwitch"){
				childSwitch = t;
			}
		}

		if(!childSwitch){
			Debug.Log("demoSwitchActivationScript:: Cannot find test switch!");
		}

		demoSwitchLightBulb = transform.parent.Find("LightFixture/demoSwitchLightBulb").gameObject;
		if(!demoSwitchLightBulb){
			Debug.Log("demoSwitchActivationScript:: Cannot find demo Switch Light Bulb!");
		}

		demoSwitchLightSource = transform.parent.Find("LightFixture/demoSwitchLightSource").gameObject;
		if(!demoSwitchLightSource){
			Debug.Log("demoSwitchActivationScript:: Cannot find demo Switch Light Source!");
		}

	}

	void Update(){

		if(buttonActionInProgress){

			if(!transform.parent.GetComponent<AudioSource>().isPlaying){

				buttonActionInProgress = false;
				demoSwitchLightBulb.GetComponent<Renderer>().material = lightOffMaterial;
				demoSwitchLightSource.GetComponent<Light>().enabled = false;

				// Here, the interaction string is updated to reflect the discovery of what the switch did
				// This means that the object now has a different interaction string to the player.
				// This is useful when discovering what things in the game world are or what they are for.
				interactionString = "Yay, what a fun switch!";

			}

		}

	}

	public void buttonAction(){

		if(!buttonActionInProgress){

			base.interact();

			buttonActionInProgress = true;
			childSwitch.GetComponent<Animation>()["SwitchTop|PushButton"].speed = 1.5f;
			childSwitch.GetComponent<Animation>().Play("SwitchTop|PushButton");

			demoSwitchLightBulb.GetComponent<Renderer>().material = lightOnMaterial;
			demoSwitchLightSource.GetComponent<Light>().enabled = true;
			transform.parent.GetComponent<AudioSource>().Play();

		}
		
	}

}
