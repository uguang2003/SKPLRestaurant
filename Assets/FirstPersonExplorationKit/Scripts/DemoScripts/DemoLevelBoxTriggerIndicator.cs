using UnityEngine;
using System.Collections;

using Whilefun.FPEKit;
using System;

//
// DemoLevelBoxTriggerIndicator
// This script demonstrates how to create a trigger for 
// a Pickup type Interactable object. This type of script
// is useful for detecting if an object was put back or
// simply moved into a location in the game world.
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class DemoLevelBoxTriggerIndicator : FPEGenericSaveableGameObject {

    private Vector3 myRotation = Vector3.zero;
	public AudioClip alarmSound;
	private GameObject indicatorMesh;
    private bool taskComplete = false;
    private Light myLight = null;
    private float startTime = 0.0f;

	void Start(){

        myRotation.y = 0.8f;

		Transform[] ct = gameObject.GetComponentsInChildren<Transform> ();

		foreach (Transform t in ct)
        {

            if (t.name == "IndicatorMesh")
            {
                indicatorMesh = t.gameObject;
            }
            else if (t.name == "PointLight")
            {
                myLight = t.gameObject.GetComponent<Light>();
            }

        }

		if(!indicatorMesh){
			Debug.LogError("DemoLevelBoxTriggerIndicator:: Indicator Mesh is missing.");
		}

        startTime = Time.time;

    }
	
	void Update(){

		indicatorMesh.transform.Rotate(myRotation);

		if(taskComplete && !gameObject.GetComponent<AudioSource>().isPlaying)
        {
            turnOffIndicator();
        }

	}

    void OnTriggerStay(Collider other){

		if(other.gameObject.name == "demoCardboardBoxSpecial")
        {

			if(taskComplete == false && other.GetComponent<FPEInteractablePickupScript>().isCurrentlyPickedUp() == false && (Time.time - startTime) > 1.5f)
            {

				gameObject.GetComponent<AudioSource>().clip = alarmSound;
				gameObject.GetComponent<AudioSource>().Play();
                taskComplete = true;

			}

        }

    }

    private void turnOffIndicator()
    {

        indicatorMesh.SetActive(false);
        gameObject.GetComponent<BoxCollider>().enabled = false;
        myLight.enabled = false;

    }

    public override FPEGenericObjectSaveData getSaveGameData()
    {
        return new FPEGenericObjectSaveData(gameObject.name, 0, 0f, taskComplete);
    }

    public override void restoreSaveGameData(FPEGenericObjectSaveData data)
    {

        taskComplete = data.SavedBool;

        if (taskComplete)
        {
            turnOffIndicator();
        }

    }

}
