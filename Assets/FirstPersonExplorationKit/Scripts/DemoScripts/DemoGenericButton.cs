using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whilefun.FPEKit;

//
// DemoGenericButton
// A very basic button for generic interactions that require some audio visual feedback to perform simple
// tasks. When pressed, it invokes an event, plays assigned sound, and moves a little.
//
// To use, assign this script to an object that you want to move like a button. Then, attach an FPEInteractableActivateScript to it or its parent object. 
// Assign the PressButton() function to the Activation Event field along with other things you want to happen. If you want, align the "Press Time" and "Event 
// Repeat Delay " values in the DemoGenericButton and FPEInteractableActivateScript, respectively.
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
[RequireComponent(typeof(AudioSource))]
public class DemoGenericButton : MonoBehaviour {

    [SerializeField, Tooltip("The distance the button 'press' will travel relative to its own Transform when PressButton() is called. Should be pretty small most of the time.")]
    private Vector3 buttonActionVector = new Vector3(0.0f, 0.0f, -0.01f);

    [SerializeField, Tooltip("The time (in seconds) the button remains in its 'pressed' position before resetting. Foe best results, match this with an FPEInteractableActivateScript's 'Event Repeat Delay' value.")]
    private float pressTime = 0.2f;

    private float pressCounter = 0.0f;
    private AudioSource buttonSpeaker;
    private Vector3 upPosition;
    private Vector3 downPosition;

    void Awake()
    {

        upPosition = transform.position;
        //downPosition = transform.position + transform.forward * actionDistance;
        downPosition = transform.position + (transform.up * buttonActionVector.y) + (transform.right * buttonActionVector.x) + (transform.forward * buttonActionVector.z);
        buttonSpeaker = gameObject.GetComponent<AudioSource>();

    }

    void Update()
    {

        if (pressCounter > 0.0f)
        {

            pressCounter -= Time.deltaTime;

            if (pressCounter <= 0.0f)
            {
                transform.position = upPosition;
            }

        }

    }

    /// <summary>
    /// Assign this to the "Activation Event" in the Inspector field for the associated FPEInteractableActivateScript component
    /// </summary>
    public void PressButton()
    {

        pressCounter = pressTime;
        transform.position = downPosition;
        buttonSpeaker.Play();

    }

}
