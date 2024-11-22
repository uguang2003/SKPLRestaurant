using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Whilefun.FPEKit;

public class DemoCutscene : MonoBehaviour {

    private DemoForcePlayerLookAt myForcedLookTarget;
    private bool cutsceneHasPlayed = false;

    private GameObject revealCurtain = null;
    private AudioSource mySpeaker = null;

    private void Awake()
    {

        myForcedLookTarget = transform.Find("DemoForceLookAt").gameObject.GetComponent<DemoForcePlayerLookAt>();
        revealCurtain = transform.Find("DemoCutsceneStage/CurtainReveal").gameObject;
        mySpeaker = transform.Find("DemoCutsceneStage").gameObject.GetComponent<AudioSource>();

        if (!myForcedLookTarget || !revealCurtain || !mySpeaker)
        {
            Debug.LogError("DemoCutscene:: Cannot find child DemoForcePlayerLookAt and other child components in object '" + gameObject.name + "'");
        }

    }

    public void startMyCutscene()
    {

        if (!cutsceneHasPlayed)
        {

            revealCurtain.SetActive(false);
            mySpeaker.Play();

            cutsceneHasPlayed = true;

            // This is the core of the cinematic. We want to look at the stage for 2 seconds, at default movement speed, and wait 0.25 seconds before moving the camera
            myForcedLookTarget.forceLookAt(2.0f, 5.0f, 0.25f);

        }

    }

}
