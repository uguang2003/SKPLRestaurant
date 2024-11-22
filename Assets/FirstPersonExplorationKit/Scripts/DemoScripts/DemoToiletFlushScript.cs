using UnityEngine;

using Whilefun.FPEKit;

//
// DemoToiletFlushScript
// This script is attached to the flush handle, and flushes the toilet
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
[RequireComponent(typeof (AudioSource))]
public class DemoToiletFlushScript : FPEInteractableActivateScript {

	private GameObject toiletRoot;

	public override void Awake(){

		// Always call back to base class Awake function
		base.Awake();

		toiletRoot = GameObject.Find("toiletRoot");
		if(!toiletRoot){
			Debug.LogError("DemoToiletFlushScript:: Cannot find toiletRoot Game Object!");
		}

	}

	public override void activate(){

		if(toiletRoot.GetComponent<DemoToiletScript>().flushToilet()){
			gameObject.GetComponent<AudioSource>().Play();
		}
		
	}

}
