using System;
using UnityEngine;

using Whilefun.FPEKit;

//
// DemoToiletScript
// This script manages the core toilet state and animations.
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class DemoToiletScript :  FPEGenericSaveableGameObject {

	private bool canFlush = true;
	private float flushCooldown = 1.5f;
	private float reflushCountdown = 0.0f;

    private DemoToiletSeatScript mySeat;
	
    void Awake()
    {
        mySeat = gameObject.GetComponentInChildren<DemoToiletSeatScript>();
        if (!mySeat)
        {
            Debug.LogError("DemoToiletScript:: No child gameobject with DemoToiletSeatScript component! I need a seat.");
        }
    }

	void Update(){

		if(!canFlush)
        {

			reflushCountdown -= Time.deltaTime;

			if(reflushCountdown <= 0.0f)
            {
				canFlush = true;
			}

		}

	}

	public bool flushToilet()
    {

		bool flushResult = false;

		if(canFlush)
        {

			gameObject.GetComponent<Animator>().SetTrigger("PressToiletHandle");
			canFlush = false;
			reflushCountdown = flushCooldown;
			flushResult = true;

		}

		return flushResult;

	}

    public override FPEGenericObjectSaveData getSaveGameData()
    {
        return new FPEGenericObjectSaveData(gameObject.name, mySeat.GetSeatState(), 0f, false);
    }

    public override void restoreSaveGameData(FPEGenericObjectSaveData data)
    {
        mySeat.RestorSeatState(data.SavedInt);
    }

}
