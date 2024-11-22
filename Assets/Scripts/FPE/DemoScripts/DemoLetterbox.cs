using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
public class DemoLetterbox : MonoBehaviour
{

    private enum eLetterBoxState
    {
        OUT = 0,
        MOVING_IN = 1,
        IN = 2,
        MOVING_OUT = 3
    }
    private eLetterBoxState currentLetterBoxState = eLetterBoxState.OUT;

    private GameObject myCanvas;
    private RectTransform topBox;
    private RectTransform bottomBox;
    private Vector3 homePositionTop;
    private Vector3 homePositionBottom;
    private Vector3 viewedPositionTop;
    private Vector3 viewedPositionBottom;
    private float transitionTimeInSeconds = 1.0f;
    private Vector3 transitionStepSize;

    private float movementAmountInCanvasUnits = 125.0f;

    private void Awake()
    {

        myCanvas = transform.Find("MyCanvas").gameObject;
        topBox = myCanvas.transform.Find("TopBox").GetComponent<RectTransform>();
        bottomBox = myCanvas.transform.Find("BottomBox").GetComponent<RectTransform>();

        if(!myCanvas || !topBox || !bottomBox)
        {
            Debug.LogError("DemoLetterbox:: Could not find one of the letterbox canvas elements on object '" + gameObject.name + "'. Letterbox effect will not work as expected.");
        }

    }

    void Start()
    {

        homePositionTop = topBox.anchoredPosition3D;
        viewedPositionTop = homePositionTop + new Vector3(0f, -movementAmountInCanvasUnits, 0f);
        homePositionBottom = bottomBox.anchoredPosition3D;
        viewedPositionBottom = homePositionBottom + new Vector3(0f, movementAmountInCanvasUnits, 0f);
        transitionStepSize = new Vector3(0f, Mathf.Abs(homePositionTop.y - viewedPositionTop.y) / transitionTimeInSeconds, 0f);

    }

    void Update()
    {

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Debug.Log("debug show letterbox");
            moveInLetterBox();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("debug hide letterbox");
            moveOutLetterBox();
        }
#endif

        if (currentLetterBoxState == eLetterBoxState.MOVING_IN)
        {

            topBox.Translate(-transitionStepSize * Time.deltaTime);
            bottomBox.Translate(transitionStepSize * Time.deltaTime);

            if (topBox.anchoredPosition3D.y <= viewedPositionTop.y)
            {

                topBox.anchoredPosition3D = viewedPositionTop;
                bottomBox.anchoredPosition3D = viewedPositionBottom;
                currentLetterBoxState = eLetterBoxState.IN;

            }

        }
        else if (currentLetterBoxState == eLetterBoxState.MOVING_OUT)
        {

            topBox.Translate(transitionStepSize * Time.deltaTime);
            bottomBox.Translate(-transitionStepSize * Time.deltaTime);

            if (topBox.anchoredPosition3D.y >= homePositionTop.y)
            {

                topBox.anchoredPosition3D = homePositionTop;
                bottomBox.anchoredPosition3D = homePositionBottom;
                cancelLetterBox();

            }

        }

    }

    public void moveInLetterBox()
    {

        myCanvas.SetActive(true);
        currentLetterBoxState = eLetterBoxState.MOVING_IN;
        topBox.anchoredPosition3D = homePositionTop;
        bottomBox.anchoredPosition3D = homePositionBottom;
        topBox.gameObject.SetActive(true);
        bottomBox.gameObject.SetActive(true);

    }

    public void moveOutLetterBox()
    {

        myCanvas.SetActive(true);
        currentLetterBoxState = eLetterBoxState.MOVING_OUT;
        topBox.anchoredPosition3D = viewedPositionTop;
        bottomBox.anchoredPosition3D = viewedPositionBottom;
        topBox.gameObject.SetActive(true);
        bottomBox.gameObject.SetActive(true);

    }

    public void cancelLetterBox()
    {

        myCanvas.SetActive(false);
        currentLetterBoxState = eLetterBoxState.OUT;
        topBox.gameObject.SetActive(false);
        bottomBox.gameObject.SetActive(false);

    }

}
