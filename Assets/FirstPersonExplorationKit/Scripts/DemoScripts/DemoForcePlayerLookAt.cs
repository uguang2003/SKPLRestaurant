using UnityEngine;
using Whilefun.FPEKit;

//
// DemoLetterbox
// This script demonstrates how you can combine a screen space letterbox 
// effect during a cutscene to provide extra focus to cinematic moments 
// in your game.
//
// To use:
// 1) Create an empty game object and attach this script.
// 2) Create a child UI canvas, call it MyCanvas.
// 3) Set MyCanvas to be Screen Space Overlay
// 4) Create two child images and anchor them to top and bottom.
// 5) Name the child images TopBox and BottomBox
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class DemoForcePlayerLookAt : MonoBehaviour
{

    private float gazeCounter = 0.0f;
    private Vector3 currentFocusPosition = Vector3.zero;
    private float focusLerpFactor = 5.0f;
    private float delayCounter = 0.0f;

    [SerializeField, Tooltip("The letterbox you want to be viewed when this forced look moment occurs")]
    private DemoLetterbox myLetterBoxCanvas = null;

    void Update()
    {

        if (gazeCounter > 0.0f)
        {

            if (delayCounter > 0.0f)
            {
                delayCounter -= Time.deltaTime;
            }
            else
            {

                currentFocusPosition = Vector3.Lerp(currentFocusPosition, transform.position, focusLerpFactor * Time.deltaTime);
                FPEPlayer.Instance.GetComponent<FPEFirstPersonController>().forcePlayerLookToPosition(currentFocusPosition);

                gazeCounter -= Time.deltaTime;

                if (gazeCounter <= 0.0f)
                {
                    myLetterBoxCanvas.moveOutLetterBox();
                    FPEInteractionManagerScript.Instance.EndCutscene();
                }

            }

        }

    }

    public void forceLookAt(float gazeDurationInSeconds = 2.0f, float focusChangeLerpFactor = 5.0f, float delayInSeconds = 0.0f)
    {

        myLetterBoxCanvas.moveInLetterBox();
        FPEInteractionManagerScript.Instance.BeginCutscene();
        gazeCounter = gazeDurationInSeconds;
        focusLerpFactor = focusChangeLerpFactor;
        delayCounter = delayInSeconds;
        currentFocusPosition = FPEPlayer.Instance.GetComponent<FPEFirstPersonController>().GetCurrentPlayerFocalPoint();

    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {

        Color c = Color.blue;
        c.a = 0.5f;
        Gizmos.color = c;

        Gizmos.DrawCube(transform.position, Vector3.one * 0.25f);
        Gizmos.DrawIcon(transform.position, "Whilefun/forceLookAt.png", false);

        if (UnityEditor.EditorApplication.isPlaying)
        {

            c = Color.yellow;
            c.a = 0.5f;
            Gizmos.color = c;

            Gizmos.DrawSphere(currentFocusPosition, 0.25f);

        }

    }

#endif

}
