using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEInputManager
    // This script is an abstract layer between the game logic and hardware input. All input for the 
    // asset is handled by this script. For other scripts that need input, they should use
    // FPEInputManager.Instance.* to get latest input state, rather than checking hardware
    // directly using Unity Input.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInputManager : MonoBehaviour
    {

        [Header("User Input options")]
        [SerializeField, Tooltip("If true, Only Mouse Y Axis will be flipped for Look")]
        private bool _flipYAxisMouseOnly = false;
        [SerializeField, Tooltip("If true, Only Gamepad Y Axis will be flipped for Look")]
        private bool _flipYAxisGamepadOnly = false;

        [SerializeField, Tooltip("Look sensitivity (basic multiplier)")]
        private Vector2 lookSensitivity = new Vector2(3.0f,3.0f);

        // An additional factor automatically applied to all gamepad analog stick inputs for a reasonable baseline
        private float gamepadAnalogStickSensitivityMultplier = 60.0f;

        [SerializeField, Tooltip("If true, player look movements will be smoothed")]
        private bool lookSmoothing = false;
        public bool LookSmoothing {
            get { return lookSmoothing; }
            set { lookSmoothing = value; }
        }

        [SerializeField, Tooltip("If true, player look movement sensitivity will be boosted up (by gamepadLookSensitivityBoost)")]
        private bool useGamepad = false;
        public bool UseGamepad {
            get { return useGamepad; }
            set { useGamepad = value; }
        }
        [SerializeField,Tooltip("If Use Gamepad is true, gamepad look input is multiplied by this factor.")]
        private float gamepadBoostMultiplier = 2.0f;
        private float appliedGamepadBoost = 1.0f;

        [SerializeField, Tooltip("If smoothing is on, this factor determines how quickly the mouse slows down. Higher value means snappier look movements.")]
        private float lookSmoothFactor = 10.0f;
        public float LookSmoothFactor { get { return lookSmoothFactor; } }

        [SerializeField, Tooltip("The absolute value the right stick must meet or exceed before a virtual 'right' button press is captured.")]
        private float rightStickButtonThreshold = 0.9f;
        [SerializeField, Tooltip("The number of seconds before a right stick 'button press' will be repeated if the stick is held left or right in position.")]
        private float rightStickButtonRepeatTime = 0.2f;
        private float rightStickButtonRepeatCounter = 0.0f;

        [SerializeField, Tooltip("Deadzone for game pad analog sticks. Any value less than this will be considered 'zero'")]
        private float analogStickDeadzone = 0.1f;

        private float triggerDeadzone = 0.05f;
        private float previousLeftTriggerValue = 0.0f;

        // These are to be called from external options menu, etc. where needed
        public bool FlipYAxisMouseOnly {
            get { return _flipYAxisMouseOnly; }
            set { _flipYAxisMouseOnly = value; }
        }

        public bool FlipYAxisGamepadOnly {
            get { return _flipYAxisGamepadOnly; }
            set { _flipYAxisGamepadOnly = value; }
        }

        public Vector2 LookSensitivity {
            get { return lookSensitivity; }
            set { lookSensitivity = value; }
        }

        protected Dictionary<eFPEInput, FPEInputAxis> FPEVirtualAxes = new Dictionary<eFPEInput, FPEInputAxis>();
        protected Dictionary<eFPEInput, FPEInputButton> FPEVirtualButtons = new Dictionary<eFPEInput, FPEInputButton>();

        // Buttons and Axes
        public enum eFPEInput
        {

            // Action buttons
            FPE_INPUT_INTERACT = 0,
            FPE_INPUT_EXAMINE,
            FPE_INPUT_ZOOM,
            FPE_INPUT_CLOSE,
            FPE_INPUT_PUT_IN_INVENTORY,
            FPE_INPUT_MENU,
            FPE_INPUT_MENU_PREVIOUS_TAB,
            FPE_INPUT_MENU_NEXT_TAB,
            FPE_INPUT_MENU_PREVIOUS_PAGE,
            FPE_INPUT_MENU_NEXT_PAGE,

            // Looking
            FPE_INPUT_MOUSELOOKX,
            FPE_INPUT_MOUSELOOKY,
            FPE_INPUT_LOOKX,
            FPE_INPUT_LOOKY,

            // Movement
            FPE_INPUT_HORIZONTAL,
            FPE_INPUT_VERTICAL,
            FPE_INPUT_JUMP,
            FPE_INPUT_CROUCH,
            FPE_INPUT_RUN

            //
            // To add a new input, define it in this enum.
            //

        }

        private static FPEInputManager _instance;
        public static FPEInputManager Instance {
            get { return _instance; }
        }

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

            // Note: The friendly names given to each button/axis below is NOT the same as the strings defined 
            // in the Project Settings > Input list. These friendly names are here for user-friendly debugging
            // and other display that might be required.

            // Add all the virtual buttons you want to poll later from other scripts
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_INTERACT, new FPEInputButton(eFPEInput.FPE_INPUT_INTERACT, "Interact"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_EXAMINE, new FPEInputButton(eFPEInput.FPE_INPUT_EXAMINE, "Examine"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_ZOOM, new FPEInputButton(eFPEInput.FPE_INPUT_ZOOM, "Zoom"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_CLOSE, new FPEInputButton(eFPEInput.FPE_INPUT_CLOSE, "Close"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_PUT_IN_INVENTORY, new FPEInputButton(eFPEInput.FPE_INPUT_PUT_IN_INVENTORY, "Put In Inventory"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_MENU, new FPEInputButton(eFPEInput.FPE_INPUT_MENU, "Menu"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_MENU_PREVIOUS_TAB, new FPEInputButton(eFPEInput.FPE_INPUT_MENU_PREVIOUS_TAB, "Menu Previous Tab"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_MENU_NEXT_TAB, new FPEInputButton(eFPEInput.FPE_INPUT_MENU_NEXT_TAB, "Menu Next Tab"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_MENU_PREVIOUS_PAGE, new FPEInputButton(eFPEInput.FPE_INPUT_MENU_PREVIOUS_PAGE, "Menu Previous Page"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_MENU_NEXT_PAGE, new FPEInputButton(eFPEInput.FPE_INPUT_MENU_NEXT_PAGE, "Menu Next Page"));

            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_JUMP, new FPEInputButton(eFPEInput.FPE_INPUT_JUMP, "Jump"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_CROUCH, new FPEInputButton(eFPEInput.FPE_INPUT_CROUCH, "Crouch"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_RUN, new FPEInputButton(eFPEInput.FPE_INPUT_RUN, "Run"));

            // Special first/last use detection, we also add VERTICAL and HORIZONTAL Virtual Buttons
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_HORIZONTAL, new FPEInputButton(eFPEInput.FPE_INPUT_HORIZONTAL, "Horizontal Button"));
            FPEVirtualButtons.Add(eFPEInput.FPE_INPUT_VERTICAL, new FPEInputButton(eFPEInput.FPE_INPUT_VERTICAL, "Vertical Button"));

            // Add all the virtual axes you want to poll later from other scripts
            FPEVirtualAxes.Add(eFPEInput.FPE_INPUT_MOUSELOOKX, new FPEInputAxis(eFPEInput.FPE_INPUT_MOUSELOOKX, "Mouse Look X"));
            FPEVirtualAxes.Add(eFPEInput.FPE_INPUT_MOUSELOOKY, new FPEInputAxis(eFPEInput.FPE_INPUT_MOUSELOOKY, "Mouse Look Y"));
            FPEVirtualAxes.Add(eFPEInput.FPE_INPUT_LOOKX, new FPEInputAxis(eFPEInput.FPE_INPUT_LOOKX, "Look X"));
            FPEVirtualAxes.Add(eFPEInput.FPE_INPUT_LOOKY, new FPEInputAxis(eFPEInput.FPE_INPUT_LOOKY, "Look Y"));
            FPEVirtualAxes.Add(eFPEInput.FPE_INPUT_HORIZONTAL, new FPEInputAxis(eFPEInput.FPE_INPUT_HORIZONTAL, "Horizontal"));
            FPEVirtualAxes.Add(eFPEInput.FPE_INPUT_VERTICAL, new FPEInputAxis(eFPEInput.FPE_INPUT_VERTICAL, "Vertical"));

            //
            // Add any new inputs via their enum here
            //

        }


        void Start()
        {
            // Nothing to do yet
        }

        //
        // This Update is the core of all FPE input. 
        // The hardware for your various platform and implementation is polled directly from Unity Input in this update.
        //
        // Note: The Input settings that ship with this asset are assumed to still be in place for this Update function
        // to work properly. If changes or additions are made, the names referenced in the Update() below must also change.
        // For example, the string "Gamepad Interact" must exist in Edit > Project Settings > Input if it is to be checked
        // against and have its value/state put into the virtual FPE_INPUT_INTERACT button.
        //
        void Update()
        {

            // -- Interact -- //

            // Keyboard and Mouse
            if (Input.GetButtonDown("Interact"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_INTERACT].Pressed();
            }
            if (Input.GetButtonUp("Interact"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_INTERACT].Released();
            }
            // Gamepad
            if (Input.GetButtonDown("Gamepad Interact"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_INTERACT].Pressed();
            }
            if (Input.GetButtonUp("Gamepad Interact"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_INTERACT].Released();
            }

            // -- Examine -- //

            // Keyboard and Mouse
            if (Input.GetButtonDown("Examine"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_EXAMINE].Pressed();
            }
            if (Input.GetButtonUp("Examine"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_EXAMINE].Released();
            }
            // Gamepad
            if (Input.GetButtonDown("Gamepad Examine"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_EXAMINE].Pressed();
            }
            if (Input.GetButtonUp("Gamepad Examine"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_EXAMINE].Released();
            }

            // -- Zoom -- //

            // NOTE: "Zoom" is by default mapped to the same hardware keys as examine, but 
            // the game handles the logic of "examining" nothing as examining the player's 
            // surroundings. Below is what seems like duplicate code, but it's here in case
            // you want to remap the zoom button to be something else in hardware.

            // Keyboard and Mouse
            if (Input.GetButtonDown("Zoom"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_ZOOM].Pressed();
            }
            if (Input.GetButtonUp("Zoom"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_ZOOM].Released();
            }
            // Gamepad
            if (Input.GetButtonDown("Gamepad Examine"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_ZOOM].Pressed();
            }
            if (Input.GetButtonUp("Gamepad Examine"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_ZOOM].Released();
            }


            // -- Close -- //

            // Keyboard and Mouse
            if (Input.GetButtonDown("Close"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_CLOSE].Pressed();
            }
            if (Input.GetButtonUp("Close"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_CLOSE].Released();
            }
            // Gamepad
            if (Input.GetButtonDown("Gamepad Close"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_CLOSE].Pressed();
            }
            if (Input.GetButtonUp("Gamepad Close"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_CLOSE].Released();
            }


            // -- Put in Inventory -- //

            // Keyboard and Mouse
            if (Input.GetButtonDown("Put In Inventory"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_PUT_IN_INVENTORY].Pressed();
            }
            if (Input.GetButtonUp("Put In Inventory"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_PUT_IN_INVENTORY].Released();
            }
            // Gamepad
            if (Input.GetButtonDown("Gamepad Put In Inventory"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_PUT_IN_INVENTORY].Pressed();
            }
            if (Input.GetButtonUp("Gamepad Put In Inventory"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_PUT_IN_INVENTORY].Released();
            }


            // -- Open Menu -- //

            // Keyboard and Mouse
            if (Input.GetButtonDown("Menu"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU].Pressed();
            }
            if (Input.GetButtonUp("Menu"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU].Released();
            }
            // Gamepad
            if (Input.GetButtonDown("Gamepad Menu"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU].Pressed();
            }
            if (Input.GetButtonUp("Gamepad Menu"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU].Released();
            }

            // -- Menu Previous Category -- //
            
            // Gamepad
            if (Input.GetButtonDown("Gamepad Previous Tab"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_PREVIOUS_TAB].Pressed();
            }
            if (Input.GetButtonUp("Gamepad Previous Tab"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_PREVIOUS_TAB].Released();
            }


            // -- Menu Next Category -- //

            // Gamepad
            if (Input.GetButtonDown("Gamepad Next Tab"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_NEXT_TAB].Pressed();
            }
            if (Input.GetButtonUp("Gamepad Next Tab"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_NEXT_TAB].Released();
            }


            // -- Menu Previous Page -- //

            if (FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_PREVIOUS_PAGE].GetButton)
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_PREVIOUS_PAGE].Released();
                rightStickButtonRepeatCounter = rightStickButtonRepeatTime;
            }

            if ((Input.GetAxis("Gamepad Look X") * lookSensitivity.x * gamepadAnalogStickSensitivityMultplier) < -rightStickButtonThreshold && rightStickButtonRepeatCounter <= 0.0f)
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_PREVIOUS_PAGE].Pressed();
                rightStickButtonRepeatCounter = rightStickButtonRepeatTime;
            }
            if ((Input.GetAxis("Gamepad Look X") * lookSensitivity.x * gamepadAnalogStickSensitivityMultplier) < 0 && (Input.GetAxis("Gamepad Look X") * lookSensitivity.x * gamepadAnalogStickSensitivityMultplier) > rightStickButtonThreshold)
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_PREVIOUS_PAGE].Released();
                rightStickButtonRepeatCounter = 0;
            }

            // -- Menu Next Page -- //

            if (FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_NEXT_PAGE].GetButton)
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_NEXT_PAGE].Released();
                rightStickButtonRepeatCounter = rightStickButtonRepeatTime;
            }

            if ((Input.GetAxis("Gamepad Look X") * lookSensitivity.x * gamepadAnalogStickSensitivityMultplier) > rightStickButtonThreshold && rightStickButtonRepeatCounter <= 0.0f)
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_NEXT_PAGE].Pressed();
                rightStickButtonRepeatCounter = rightStickButtonRepeatTime;
            }
            if ((Input.GetAxis("Gamepad Look X") * lookSensitivity.x * gamepadAnalogStickSensitivityMultplier) > 0 && (Input.GetAxis("Gamepad Look X") * lookSensitivity.x * gamepadAnalogStickSensitivityMultplier) < rightStickButtonThreshold)
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_MENU_NEXT_PAGE].Released();
                rightStickButtonRepeatCounter = 0;
            }

            //// Since we're using a virtual button (via axis) for "Menu Previous Page" and "Menu Next Page", we need to manually repeat the 'press' every N seconds
            if (rightStickButtonRepeatCounter > 0.0f)
            {
                rightStickButtonRepeatCounter -= Time.unscaledDeltaTime;
            }


            // -- Jump -- //

            if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Gamepad Jump"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_JUMP].Pressed();
            }
            if (Input.GetButtonUp("Jump") || Input.GetButtonUp("Gamepad Jump"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_JUMP].Released();
            }

            // -- Crouch -- //

            if (Input.GetButtonDown("Crouch") || Input.GetButtonDown("Gamepad Crouch"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_CROUCH].Pressed();
            }
            if (Input.GetButtonUp("Crouch") || Input.GetButtonUp("Gamepad Crouch"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_CROUCH].Released();
            }


            // -- Run -- //

            if (Input.GetButton("Run") || (Input.GetAxis("Gamepad Run") > triggerDeadzone && previousLeftTriggerValue <= triggerDeadzone))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_RUN].Pressed();
            }
            // This release check also handles a weird edge case when changing scenes with "Run" held down. This ensures that run is never stuck "on" when a new scene is finished loading
            if ((!Input.GetButton("Run") && Input.GetAxis("Gamepad Run") <= triggerDeadzone) || (Input.GetAxis("Gamepad Run") <= triggerDeadzone && previousLeftTriggerValue > triggerDeadzone))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_RUN].Released();
            }

            // Using left trigger for Run requires some additional book keeping
            previousLeftTriggerValue = Input.GetAxis("Gamepad Run");


            // -- Player Movement -- //

            // We store the Horizontal and Vertical axes as button up/down so we can detect when they are 
            // first pressed and released. Though for general case use, their axis values should be used.

            if (Input.GetButtonDown("Horizontal"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_HORIZONTAL].Pressed();
            }
            if (Input.GetButtonUp("Horizontal"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_HORIZONTAL].Released();
            }

            if (Input.GetButtonDown("Vertical"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_VERTICAL].Pressed();
            }
            if (Input.GetButtonUp("Vertical"))
            {
                FPEVirtualButtons[eFPEInput.FPE_INPUT_VERTICAL].Released();
            }

            // Continuous axis values for things like player movement
            FPEVirtualAxes[eFPEInput.FPE_INPUT_HORIZONTAL].Update(Input.GetAxis("Horizontal"));
            FPEVirtualAxes[eFPEInput.FPE_INPUT_VERTICAL].Update(Input.GetAxis("Vertical"));


            // -- Look -- //

            // Mouse
            FPEVirtualAxes[eFPEInput.FPE_INPUT_MOUSELOOKX].Update(Input.GetAxis("Mouse X") * lookSensitivity.x);

            if (_flipYAxisMouseOnly)
            {
                FPEVirtualAxes[eFPEInput.FPE_INPUT_MOUSELOOKY].Update(Input.GetAxis("Mouse Y") * lookSensitivity.y * -1.0f);
            }
            else
            {
                FPEVirtualAxes[eFPEInput.FPE_INPUT_MOUSELOOKY].Update(Input.GetAxis("Mouse Y") * lookSensitivity.y);
            }

            // Only boost if the use gamepad toggle is checked
            appliedGamepadBoost = (useGamepad) ? gamepadBoostMultiplier : 1.0f;

            // Gamepad
            if (Mathf.Abs(Input.GetAxisRaw("Gamepad Look X")) > analogStickDeadzone)
            {
                FPEVirtualAxes[eFPEInput.FPE_INPUT_LOOKX].Update(Input.GetAxisRaw("Gamepad Look X") * lookSensitivity.x * gamepadAnalogStickSensitivityMultplier * appliedGamepadBoost);
            }
            else
            {
                FPEVirtualAxes[eFPEInput.FPE_INPUT_LOOKX].Update(0.0f);
            }

            if (Mathf.Abs(Input.GetAxisRaw("Gamepad Look Y")) > analogStickDeadzone)
            {

                if (_flipYAxisGamepadOnly)
                {
                    FPEVirtualAxes[eFPEInput.FPE_INPUT_LOOKY].Update(Input.GetAxisRaw("Gamepad Look Y") * lookSensitivity.y * gamepadAnalogStickSensitivityMultplier * appliedGamepadBoost);
                }
                else
                {
                    FPEVirtualAxes[eFPEInput.FPE_INPUT_LOOKY].Update(Input.GetAxisRaw("Gamepad Look Y") * lookSensitivity.y * gamepadAnalogStickSensitivityMultplier * appliedGamepadBoost * -1.0f);
                }

            }
            else
            {
                FPEVirtualAxes[eFPEInput.FPE_INPUT_LOOKY].Update(0.0f);
            }


            //
            // Update any new virtual buttons or axes from their appropriate hardware state here.
            //



        }

        public bool GetButton(eFPEInput buttonID)
        {

            if (FPEVirtualButtons.ContainsKey(buttonID))
            {
                return FPEVirtualButtons[buttonID].GetButton;
            }
            else
            {
                Debug.LogError("FPEInputManager.GetButtonDown:: No button ID '" + buttonID + "'. Are you looking for an axis instead?");
                return false;
            }

        }

        public bool GetButtonDown(eFPEInput buttonID)
        {

            if (FPEVirtualButtons.ContainsKey(buttonID))
            {
                return FPEVirtualButtons[buttonID].GetButtonDown;
            }
            else
            {
                Debug.LogError("FPEInputManager.GetButtonDown:: No button ID '" + buttonID + "'. Are you looking for an axis instead?");
                return false;
            }

        }

        public bool GetButtonUp(eFPEInput buttonID)
        {

            if (FPEVirtualButtons.ContainsKey(buttonID))
            {
                return FPEVirtualButtons[buttonID].GetButtonUp;
            }
            else
            {
                Debug.LogError("FPEInputManager.GetButtonDown:: No button ID '" + buttonID + "'. Are you looking for an axis instead?");
                return false;
            }

        }

        /// <summary>
        /// Returns a cleaned version of the axis, yielding to deadzone value.
        /// </summary>
        /// <param name="axisID">The axis to check</param>
        /// <returns>The cleaned version of the axis value, adhering to deadzone values</returns>
        public float GetAxis(eFPEInput axisID)
        {

            if (FPEVirtualAxes.ContainsKey(axisID))
            {
                return FPEVirtualAxes[axisID].GetValue;
            }
            else
            {
                Debug.LogError("FPEInputManager.GetAxis:: No axis ID '" + axisID + "'. Are you looking for a button instead?");
                return 0.0f;
            }

        }
        
        /// <summary>
        /// Returns the raw axis value, ignoring deadzone
        /// </summary>
        /// <param name="axisID">The axis to check</param>
        /// <returns>The raw axis value</returns>
        public float GetAxisRaw(eFPEInput axisID)
        {

            if (FPEVirtualAxes.ContainsKey(axisID))
            {
                return FPEVirtualAxes[axisID].GetValue;
            }
            else
            {
                Debug.LogError("FPEInputManager.GetAxis:: No axis ID '" + axisID + "'. Are you looking for a button instead?");
                return 0.0f;
            }

        }

        /// <summary>
        /// This function flushes a subset of the input to ensure clean state is ready after operations like saving and loading a saved game, changing scene, etc.
        /// NOTE: Not all inputs are flushed. If you add custom buttons or axes, you should play test to check if flushing those inputs is required or not.
        /// </summary>
        public void FlushInputs()
        {

            // Prevent "sticky interact" on scene change, which required an extra key up and key down event before first interaction could happen
            FPEVirtualButtons[eFPEInput.FPE_INPUT_INTERACT].Flush();


        }

        //
        // FPEInputButton
        // A simple helper class to house general button data frame over frame.
        //
        public class FPEInputButton
        {

            public eFPEInput id { get; private set; }
            public string friendlyName { get; private set; }
            private int lastPressedFrame = -5;
            private int releasedFrame = -5;
            private bool pressed = false;

            public FPEInputButton(eFPEInput id, string name)
            {
                this.id = id;
                this.friendlyName = name;
            }

            public void Pressed()
            {

                if (pressed)
                {
                    return;
                }

                pressed = true;
                lastPressedFrame = Time.frameCount;

            }

            public void Released()
            {
                pressed = false;
                releasedFrame = Time.frameCount;
            }

            public bool GetButton {
                get { return pressed; }
            }

            public bool GetButtonDown {
                get { return (lastPressedFrame - Time.frameCount) == -1; }
            }

            public bool GetButtonUp {
                get { return (releasedFrame == (Time.frameCount - 1)); }
            }

            public void Flush()
            {
                pressed = false;
                releasedFrame = Time.frameCount;
            }

        }

        //
        // FPEInputAxis
        // A simple helper class to house general axis data frame over frame.
        //
        public class FPEInputAxis
        {

            public eFPEInput id { get; private set; }
            public string name { get; private set; }
            private float value;

            public FPEInputAxis(eFPEInput id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public void Update(float latestValue)
            {
                value = latestValue;
            }

            public float GetValue {
                get { return value; }
            }

            public float GetValueRaw {
                get { return value; }
            }

        }

    }

}