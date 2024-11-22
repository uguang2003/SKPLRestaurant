using UnityEngine;
using System.Collections;

using Whilefun.FPEKit;
using System;

//
// DemoIdolScript
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class DemoIdolScript : MonoBehaviour {

    private DemoIdolTrapScript theTrap;
    private bool pickedUpOnce = false;

	void Awake()
    {
        theTrap = GameObject.FindObjectOfType<DemoIdolTrapScript>();
	}
    
    public void idolPickupEvent()
    {
        if (!pickedUpOnce && theTrap)
        {
            pickedUpOnce = true;
            theTrap.idolPickedUp();
        }
    }

    public void idolReturnEvent()
    {
        gameObject.GetComponent<FPEInteractablePickupScript>().interactionString = "It's the artifact I returned. Nearly died for this thing.";
    }
    
}
