using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Whilefun.FPEKit;

//
// DemoInventoryConsumer
//
// This script is an example of how you can make arbitrary things
// happen when the player consumes an inventory item.
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class DemoInventoryConsumer : FPEInventoryConsumer
{

    [SerializeField, Tooltip("This sound is played when the item is consumed")]
    private AudioClip consumptionAudioClip = null;

    [SerializeField, Tooltip("This prefab is created and thrown on the ground when the item is consumed")]
    private GameObject discardedPiece = null;

    private AudioSource myAudio = null;

    void OnEnable()
    {

        myAudio = gameObject.GetComponent<AudioSource>();

        if (!myAudio)
        {
            myAudio = gameObject.AddComponent<AudioSource>();
            myAudio.loop = false;
            myAudio.playOnAwake = false;
        }

    }

    public override void Update()
    {

        base.Update();

        //
        // Here, we wait until our sound has finished player, then call to 
        // finishConsumption(). This function can be called whenever you like:
        // -After some event
        // -From another script
        // -At the end of an animation
        // -And any other time you want!
        //
        if (ConsumptionStarted && !myAudio.isPlaying)
        {
            finishConsumption();
        }

    }

    /// <summary>
    /// This function is called automatically when the player consumes a consumable type inventory item from the inventory screen.
    /// </summary>
    public override void consumeItem()
    {

        base.consumeItem();

        Debug.Log("DemoInventoryConsumer:: Note that you can do anything else you want right here!");

        //
        // Here, we are simulating the player eating an apple by playing a sound and dropping an apple core in front of the player.
        //
        // BUT: You can make your consumeItem function do whatever you want. For example:
        // -Increase player health
        // -Activate a potion or spell
        // -Decrease player hunger or fear
        // -Spawn objects or play sounds
        // -Anything else you can think of!
        //

        myAudio.clip = consumptionAudioClip;
        myAudio.Play();

        GameObject discardedCore = Instantiate(discardedPiece) as GameObject;
        FPEInteractionManagerScript.Instance.tossObject(discardedCore.GetComponent<FPEInteractablePickupScript>());

    }

}
