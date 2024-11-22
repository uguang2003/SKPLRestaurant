using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Whilefun.FPEKit;


//
// DemoTriggerSequenceHelper
// This script demonstrates how you can make the outcomes from a trigger 
// sequence saveable and loadable.
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class DemoTriggerSequenceHelper : FPEGenericSaveableGameObject {

    [SerializeField]
    private MeshRenderer triggerIndicatorA = null;
    [SerializeField]
    private MeshRenderer triggerIndicatorB = null;
    [SerializeField]
    private MeshRenderer triggerIndicatorC = null;

    [SerializeField]
    private Material triggerArmed = null;
    [SerializeField]
    private Material triggerDisarmed = null;

    [SerializeField]
    private GameObject triggeredWall = null;

    private int sequenceState = 0;

    void Awake()
    {

        if(!triggerIndicatorA || !triggerIndicatorB || !triggerIndicatorC || !triggeredWall)
        {
            Debug.LogError("DemoTriggerSequenceHelper:: Missing objects from inspector!");
        }

    }

    public void armA()
    {
        triggerIndicatorA.material = triggerArmed;
        sequenceState = 0;
    }

    public void tripA()
    {
        triggerIndicatorA.material = triggerDisarmed;
        triggerIndicatorB.material = triggerArmed;
        triggerIndicatorC.material = triggerDisarmed;
        sequenceState = 1;
    }

    public void tripB()
    {
        triggerIndicatorA.material = triggerDisarmed;
        triggerIndicatorB.material = triggerDisarmed;
        triggerIndicatorC.material = triggerArmed;
        triggeredWall.SetActive(true);
        sequenceState = 2;
    }

    public void tripC()
    {
        triggerIndicatorA.material = triggerDisarmed;
        triggerIndicatorB.material = triggerDisarmed;
        triggerIndicatorC.material = triggerArmed;
        sequenceState = 3;
    }

    public void resetSequence()
    {

        triggeredWall.SetActive(false);
        triggerIndicatorA.material = triggerArmed;
        triggerIndicatorB.material = triggerDisarmed;
        triggerIndicatorC.material = triggerDisarmed;
        sequenceState = 0;

    }

    public override FPEGenericObjectSaveData getSaveGameData()
    {
        return new FPEGenericObjectSaveData(gameObject.name, sequenceState, 0f, false);
    }

    public override void restoreSaveGameData(FPEGenericObjectSaveData data)
    {

        int loadedState = data.SavedInt;

        switch (loadedState)
        {

            case 0:
                armA();
                break;
            case 1:
                tripA();
                break;
            case 2:
                tripB();
                break;
            case 3:
                tripC();
                break;
            default:
                Debug.LogError("DemoTriggerSequenceHelper:: given bad state '"+loadedState+"'");
                break;

        }

    }
}
