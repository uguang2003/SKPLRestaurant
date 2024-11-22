using UnityEngine;
using UnityEngine.UI;

using Whilefun.FPEKit;

//
// FPEInputTester
// This script is attached to a canvas with text child components, and outputs input state
// from the FPEInputManager prefab. It can be used for debugging changes to controls, or for
// verifying new key mappings from new controllers, control schemes, etc. to ensure that
// they work with the required inputs for your game.
//
// To use:
// 1) Create an empty canvas, and add a child Text object called 'InputResults'. 
// 2) Add this script to the Canvas object and run the scene.
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class FPEInputTester : MonoBehaviour {

    private static FPEInputTester _instance;
    public static FPEInputTester Instance {
        get { return _instance; }
    }

    private Text resultsText = null;
    private Text dataLogText = null;

    private GameObject pauseIndicator = null;
    private bool paused = false;

    private int maxStringLengh = 1024;

    string inputState = "";
    string dataLog = "";

    void Awake()
    {

        if (_instance != null)
        {
            Debug.LogWarning("FPEInputManager:: Duplicate instance of FPEInputManager, deleting second one.");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

    }

    void Start()
    {

        resultsText = transform.Find("InputResults").GetComponent<Text>();
        dataLogText = transform.Find("DataLog").GetComponent<Text>();
        pauseIndicator = transform.Find("PauseIndicator").gameObject;

        if (!resultsText || !dataLogText || !pauseIndicator)
        {
            Debug.LogError("FPEInputTester:: Canvas is missing Text child called 'InputResults', 'DataLog', or 'PauseIndicator'! Testing won't work.'");
        }

        if(FPEInputManager.Instance == null)
        {
            Debug.LogError("FPEInputTester:: There is no instance of FPEInputManager in your scene. Add one for input testing to work correctly.");
        }

        pauseIndicator.SetActive(false);

    }

    void Update()
    {

        // Pausing //
        if (Input.GetKeyDown(KeyCode.Space))
        {

            paused = !paused;

            if (paused)
            {
                Time.timeScale = 0.0f;
                pauseIndicator.SetActive(true);
            }
            else
            {
                Time.timeScale = 1.0f;
                pauseIndicator.SetActive(false);
            }

        }

        if (!paused)
        {

            // All Inputs //
            inputState = "";

            inputState += "Interact: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_INTERACT) + "\n";
            inputState += "Examine: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_EXAMINE) + "\n";
            inputState += "Zoom: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_ZOOM) + "\n";
            inputState += "Put In Inventory: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_PUT_IN_INVENTORY) + "\n";

            inputState += "Jump: " + FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_JUMP) + "\n";
            inputState += "Crouch: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_CROUCH) + "\n";
            inputState += "Run: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_RUN) + "\n";

            inputState += "Menu: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_MENU) + "\n";
            inputState += "Menu Prev Tab: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_MENU_PREVIOUS_TAB) + "\n";
            inputState += "Menu Next Tab: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_MENU_NEXT_TAB) + "\n";
            inputState += "Menu Prev Page: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_MENU_PREVIOUS_PAGE) + "\n";
            inputState += "Menu Next Page: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_MENU_NEXT_PAGE) + "\n";
            inputState += "Close: " + FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_CLOSE) + "\n";

            inputState += "Horizontal (ButtonDown): " + FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_HORIZONTAL) + "\n";
            inputState += "Horizontal (ButtonUp): " + FPEInputManager.Instance.GetButtonUp(FPEInputManager.eFPEInput.FPE_INPUT_HORIZONTAL) + "\n";
            inputState += "Vertical (ButtonDown): " + FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_VERTICAL) + "\n";
            inputState += "Vertical (ButtonUp): " + FPEInputManager.Instance.GetButtonUp(FPEInputManager.eFPEInput.FPE_INPUT_VERTICAL) + "\n";

            inputState += "Look X/Y: (" + FPEInputManager.Instance.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_LOOKX) + "," + FPEInputManager.Instance.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_LOOKY) + ")\n";
            inputState += "Look X/Y (RAW): (" + FPEInputManager.Instance.GetAxisRaw(FPEInputManager.eFPEInput.FPE_INPUT_LOOKX) + "," + FPEInputManager.Instance.GetAxisRaw(FPEInputManager.eFPEInput.FPE_INPUT_LOOKY) + ")\n";

            inputState += "Horizontal: " + FPEInputManager.Instance.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_HORIZONTAL) + "\n";
            inputState += "Vertical: " + FPEInputManager.Instance.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_VERTICAL) + "\n";
            inputState += "Horizontal (RAW): " + FPEInputManager.Instance.GetAxisRaw(FPEInputManager.eFPEInput.FPE_INPUT_HORIZONTAL) + "\n";
            inputState += "Vertical (RAW): " + FPEInputManager.Instance.GetAxisRaw(FPEInputManager.eFPEInput.FPE_INPUT_VERTICAL) + "\n";

            resultsText.text = inputState;

            // Additional Data //
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                dataLog = "";
            }
            if (dataLog.Length > maxStringLengh)
            {
                dataLog = dataLog.Substring(maxStringLengh/2);
            }

            dataLog += "Look X/Y: (" + FPEInputManager.Instance.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_LOOKX) + "," + FPEInputManager.Instance.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_LOOKY) + ")\n";
            dataLogText.text = dataLog;

        }

    }

}
