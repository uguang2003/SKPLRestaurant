using System;
using UnityEngine;

using Whilefun.FPEKit;

//
// DemoRadioSimpleScript
// This script demonstrates the most basic extension of the
// Activate type of Interactable object. It just manages one
// state variable, and plays/stops a sound. See 
// DemoRadioComplexScript for a more involved version.
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
[RequireComponent(typeof (AudioSource))]
public class DemoRadioSimpleScript : MonoBehaviour {

    public void turnRadioOn()
    {
        gameObject.GetComponent<FPEInteractableBaseScript>().interactionString = "Turn off simple radio";
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void turnRadioOff()
    {
        gameObject.GetComponent<FPEInteractableBaseScript>().interactionString = "Turn on simple radio";
        gameObject.GetComponent<AudioSource>().Stop();
    }

}
