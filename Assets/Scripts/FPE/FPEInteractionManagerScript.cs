using Unity.VisualScripting;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEInteractionManagerScript
    // This script handles all player actions with respect to Interactable objects in the 
    // game world. All raycast hit detection, input handling, object manipulation, etc. occurs
    // in this script. All interfaces from other scripts to the Player Controller are done
    // through this script as well.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInteractionManagerScript : MonoBehaviour
    {

        private static FPEInteractionManagerScript _instance;
        public static FPEInteractionManagerScript Instance {
            get { return _instance; }
        }

        [Header("Examination Options")]
        [Tooltip("When examining an object, it will only rotate if axis input is greater than this deadzone value. Default is 0.1.")]
        public float examinationDeadzone = 0.1f;
        [Tooltip("When examining an object, this value acts as a multiplier to Input Mouse X/Y values. Default is 300.")]
        public float examineRotationSpeed = 300.0f;

        [Header("Custom Journal Cursor. Note: Only works when using Unity 5+")]
        public Texture2D journalCursor;

        [Header("Special Interaction Masks")]
        [Tooltip("This should be assigned to FPEPutbackObjects layer")]
        public LayerMask putbackLayerMask;
        [Tooltip("This should be assigned to be mixed to include everything except the FPEPutbackObjects and FPEIgnore layers")]
        public LayerMask interactionLayerMask;

        // Max range you can interact with an object. Note that interactions are governed ultimately by the Interactable 
        // Object's interactionDistance. For performance reasons, you may want to lower this to something less than Mathf.Infinity
        private float interactionRange = Mathf.Infinity;
        
        private Sprite[] currentJournalPages = null;
        private int currentJournalPageIndex = 0;
        
        private bool playingAudioDiary = false;
        private bool audioDiaryPlaybackIsReplay = false;
        private string currentDiaryTitle = "";
        private GameObject currentAudioDiary = null;

        public GameObject currentInteractableObject = null;
        public GameObject currentHeldObject = null;
        private GameObject currentPutbackObject = null;
        private GameObject interactionObjectPickupLocation = null;
        private GameObject interactionObjectExamineLocation = null;
        private GameObject interactionObjectTossLocation = null;
        private GameObject interactionInventoryLocation = null;
        private GameObject audioDiaryPlayer = null;
        private GameObject secondaryInteractionSoundPlayer = null;
        private GameObject journalSFXPlayer = null;
        private GameObject genericSFXPlayer = null;

        private bool examiningObject = false;

        // Camera zoom and mouse stuff
        [Header("Mouse Zoom and Sensitivity Options")]
        [Tooltip("This is the FOV the camera will use when player Right-Clicks to zoom in. Un-zoomed FOV is set to be that of Main Camera when scene starts. If you change FOV in Main Camera, also change it in ExaminationCamera.")]
        public float zoomedFOV = 24.0f;
        private float unZoomedFOV = 0.0f;
        private bool cameraZoomedIn = false;
        [SerializeField, Tooltip("The FOV degrees per second for zoom changes in and out")]
        private float cameraZoomFOVChangePerSecond = 6.0f;
        [Tooltip("Zoomed mouse sensitivity multiplier (e.g. 0.5 would be half as fast)")]
        public float zoomedMouseSensitivityMultiplier = 0.5f;
        private Vector2 zoomedMouseSensitivity = new Vector2(1.0f, 1.0f);
        [Tooltip("Apply special mouse sensitivity when reticle is over an interactable object")]
        public bool slowMouseOnInteractableObjectHighlight = true;
        [Tooltip("Mouse sensitivity when reticle is over an interactable object (0.5 would be half as sensitive)")]
        public float highlightedMouseSensitivityMultiplier = 0.5f;
        private Vector2 highlightedMouseSensitivity = new Vector2(1.0f, 1.0f);
        private Vector2 startingMouseSensitivity = Vector2.zero;
        // Used to ensure smooth sensitivity changes when mouse is slowed on reticle highlight of object vs. unhighlighted
        private Vector2 targetMouseSensitivity = Vector2.zero;
        private bool smoothMouseChange = false;
        private float smoothMouseChangeRate = 2.0f;

        private GameObject thePlayer = null;
        private bool mouseLookEnabled = true;

        // Examination stuff
        Quaternion lastObjectHeldRotation = Quaternion.identity;
        // This is multiplied with tossStrength of held object. Seems to be an okay value.
        private float tossImpulseFactor = 2.5f;

        // Journal stuff
        [Header("The sounds journals make when used. Use 2D sounds for best effect.")]
        public AudioClip journalOpen;
        public AudioClip journalClose;
        public AudioClip journalPageTurn;
        private GameObject currentJournal = null;

        // Dock stuff
        private GameObject currentDock = null;

        [Header("Other generic sound effects. Use 2D sounds for best effect.")]
        public AudioClip inventoryPickup;
        public AudioClip noteAdded;

        // Audio Diary stuff
        [Tooltip("Volume fade out amount per 100ms (0.0 to 1.0, with 1.0 being 100% of the volume)")]
        public float fadeAmountPerTenthSecond = 0.1f;
        private bool fadingDiaryAudio = false;
        private float fadeCounter = 0.0f;

        // Docking stuff
        private bool dockingInProgress = false;
        private FPEFirstPersonController.ePlayerDockingState currentDockActionType = FPEFirstPersonController.ePlayerDockingState.IDLE;
        // Save/Load stuff
        private bool playerSuspendedForSaveLoad = false;
        public bool PlayerSuspendedForSaveLoad { get { return playerSuspendedForSaveLoad; } }
        // Inventory Stuff
        private FPEInventoryManagerScript inventoryManager = null;
        // HUD
        private FPEHUD myHUD = null;
        private FPEHUDData myHUDData = null;

        GameObject emptyObj;
        GameObject BurgerObj;
        Transform BurgerPos;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField, Tooltip("If true, debug gizmos will be drawn (Editor Only)")]
        private bool drawDebugGizmos = true;
#endif

        // Top level interaction FSM 
        public enum eInteractionState
        {
            FREE = 0,
            IN_MENU = 1,
            SUSPENDED = 2
        }
        private eInteractionState currentInteractionState = eInteractionState.FREE;

        private bool initialized = false;
        

        void Awake()
        {

            if (_instance != null)
            {
                Debug.LogWarning("FPEInteractionManager:: Duplicate instance of FPEInteractionManager, deleting second one.");
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
            BurgerObj = Resources.Load<GameObject>("Pickups/汉堡");
            initialize();
        }

        public void initialize()
        {

            if (!initialized)
            {

                initialized = true;

                inventoryManager = gameObject.GetComponent<FPEInventoryManagerScript>();
                if (!inventoryManager)
                {
                    Debug.LogError("FPEInteractionManager:: There was no FPEInventoryManagerScript attached to '" + gameObject.name + "'. Did you change the FPEInteractionManager prefab?");
                }

                myHUD = FPEHUD.Instance;
                if (!myHUD)
                {
                    Debug.LogError("FPEInteractionManager:: Cannot find instance of FPEHUD!");
                }

                myHUDData = new FPEHUDData();

                thePlayer = FPEPlayer.Instance.gameObject;
                if (!thePlayer)
                {
                    Debug.LogError("FPEInteractionManagerScript:: Player could not be found!");
                }

                interactionObjectPickupLocation = thePlayer.transform.Find("MainCamera/ObjectPickupLocation").gameObject;
                interactionObjectExamineLocation = thePlayer.transform.Find("MainCamera/ObjectExamineLocation").gameObject;
                interactionObjectTossLocation = thePlayer.transform.Find("MainCamera/ObjectTossLocation").gameObject;
                interactionInventoryLocation = thePlayer.transform.Find("MainCamera/ObjectInInventoryPosition").gameObject;
                audioDiaryPlayer = thePlayer.transform.Find("FPEAudioDiaryPlayer").gameObject;
                secondaryInteractionSoundPlayer = thePlayer.transform.Find("FPESecondaryInteractionSoundPlayer").gameObject;
                journalSFXPlayer = thePlayer.transform.Find("FPEJournalSFX").gameObject;
                genericSFXPlayer = thePlayer.transform.Find("FPEGenericSFX").gameObject;

                // We don't need the debug meshes at runtime
                Destroy(interactionObjectPickupLocation.GetComponentInChildren<MeshRenderer>().gameObject);
                Destroy(interactionObjectExamineLocation.GetComponentInChildren<MeshRenderer>().gameObject);
                Destroy(interactionObjectTossLocation.GetComponentInChildren<MeshRenderer>().gameObject);
                Destroy(interactionInventoryLocation.GetComponentInChildren<MeshRenderer>().gameObject);

                if (!interactionObjectPickupLocation || !interactionObjectExamineLocation || !interactionObjectTossLocation || !interactionInventoryLocation)
                {
                    Debug.LogError("FPEInteractionManagerScript:: Player or its components are missing. Did you change the Player Controller prefab, or forget to tag with 'Player' tag?");
                }

                if (!audioDiaryPlayer || !secondaryInteractionSoundPlayer || !journalSFXPlayer || !genericSFXPlayer)
                {
                    Debug.LogError("FPEInteractionManagerScript:: FPEAudioDiaryPlayer, FPESecondaryInteractionSoundPlayer, or FPEJournalSFX are missing from Player Controller. Did you break the FPEPlayerController prefab or forget to add one or both of these prefabs to your player controller?");
                }

                // The core system expects there to be some kind of menu present. To work around this, you can simply change the openMenu() and 
                // closeMenu() functions to suit your needs, or make them blank functions, remove them, etc. as required.
                if (FPEMenu.Instance == null)
                {
                    Debug.LogError("FPEInteractionManagerScript:: There is no FPEMenu present in your scene. This will mean openMenu() and closeMenu() won't function as expected but the game will still run.");
                }

                rememberStartingMouseSensitivity();
                refreshAlternateMouseSensitivities();
                unZoomedFOV = Camera.main.fieldOfView;
                closeJournal(false);
                setCursorVisibility(false);

            }

        }


        void Update()
        {

            if (currentInteractionState == eInteractionState.IN_MENU)
            {

                // Close Menu //
                if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_MENU))
                {
                    closeMenu();
                }

            }
            else if(currentInteractionState == eInteractionState.FREE)
            {

                #region CORE_INTERACTION_LOGIC

                #region CHECK_FOR_INTERACTABLE_OBJECT

                Ray interactionRay = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
                RaycastHit hitInteractable;
                Physics.Raycast(interactionRay, out hitInteractable, interactionRange, interactionLayerMask);

                // Check if we're looking at InteractableBase-type object //
                if (!examiningObject && !dockingInProgress && hitInteractable.transform != null && hitInteractable.transform.gameObject.GetComponent<FPEInteractableBaseScript>())
                {

                    if (hitInteractable.distance < hitInteractable.transform.gameObject.GetComponent<FPEInteractableBaseScript>().getInteractionDistance())
                    {

                        if (currentInteractableObject)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().unHighlightObject();
                            currentInteractableObject = null;
                        }

                        currentInteractableObject = hitInteractable.transform.gameObject;

                        // This is the one case where we retrieve notes from objects by looking at them
                        if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.STATIC)
                        {
                            retrieveNote(currentInteractableObject);
                            retrievePassiveAudioDiary(currentInteractableObject, false);
                        }
                        else
                        {
                            // Notes are not retrieved passively, only diaries
                            retrievePassiveAudioDiary(currentInteractableObject, false);
                        }

                        if (slowMouseOnInteractableObjectHighlight)
                        {
                            setMouseSensitivity(highlightedMouseSensitivity);
                        }

                        updateObjectHighlights();

                    }
                    else
                    {

                        if (currentInteractableObject)
                        {

                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().unHighlightObject();
                            currentInteractableObject = null;
                            // If not in range, you could do some kind of additional "reticle hint" to override reticle state (icon only) to show "hey, there's something over there"?
                            updateObjectHighlights();

                        }

                    }

                }
                // Check if we're looking at Put Back Location //
                else
                {

                    if (currentInteractableObject)
                    {
                        currentInteractableObject.GetComponent<FPEInteractableBaseScript>().unHighlightObject();
                        currentInteractableObject = null;
                    }

                    // Check for Put Back Location //
                    Ray rayPutBack = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
                    RaycastHit hitPutBack2;
                    Physics.Raycast(rayPutBack, out hitPutBack2, interactionRange, putbackLayerMask);

                    // Only allow Put Back if we're holding an object and not currently examining it
                    if (!examiningObject && hitPutBack2.transform != null && hitPutBack2.transform.gameObject.GetComponent<FPEPutBackScript>() && currentHeldObject != null)
                    {

                        if (hitPutBack2.transform.gameObject.GetComponent<FPEPutBackScript>().putBackMatchesGameObject(currentHeldObject) && (hitPutBack2.distance < hitPutBack2.transform.gameObject.GetComponent<FPEPutBackScript>().getInteractionDistance()))
                        {
                            currentPutbackObject = hitPutBack2.transform.gameObject;
                        }
                        else
                        {
                            currentPutbackObject = null;
                        }

                        if (slowMouseOnInteractableObjectHighlight)
                        {
                            setMouseSensitivity(highlightedMouseSensitivity);
                        }

                        updateObjectHighlights();

                    }
                    // I guess we're looking at NOTHING //
                    else
                    {

                        if (currentInteractableObject)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().unHighlightObject();
                            currentInteractableObject = null;
                        }

                        currentPutbackObject = null;

                        updateObjectHighlights();

                        if (!cameraZoomedIn)
                        {
                            restorePreviousMouseSensitivity(true);
                        }

                    }

                }
                #endregion

                #region HANDLE_INTERACTIONS
                // Pick up / Put down / Interact / Read / Activate / Etc. //
                if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_INTERACT) && !examiningObject && !dockingInProgress)
                {

                    // Already holding an object //
                    if (currentHeldObject)
                    {

                        // If I was looking at a valid Put Back location, put the object back
                        if (currentPutbackObject)
                        {

                            currentHeldObject.GetComponent<FPEInteractablePickupScript>().doPickupPutdown(true);
                            currentHeldObject.transform.position = currentPutbackObject.transform.position;
                            currentHeldObject.transform.rotation = currentPutbackObject.transform.rotation;
                            currentHeldObject.transform.parent = null;
                            currentHeldObject.GetComponent<Collider>().isTrigger = false;
                            currentHeldObject.GetComponent<Rigidbody>().isKinematic = false;
                            currentHeldObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
                            currentHeldObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                            currentHeldObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

                            Transform[] objectTransforms = currentHeldObject.GetComponentsInChildren<Transform>();

                            foreach (Transform t in objectTransforms)
                            {
                                t.gameObject.layer = LayerMask.NameToLayer("FPEPickupObjects");
                            }

                            if (currentHeldObject.tag == "下层面包")
                            {
                                emptyObj = new GameObject();
                                emptyObj.transform.position = currentPutbackObject.transform.position;
                                emptyObj.transform.rotation = currentPutbackObject.transform.rotation;
                                emptyObj.transform.parent = currentPutbackObject.transform.parent;
                                emptyObj.name = "汉堡位置";
                                emptyObj.tag = "Burger";
                                currentHeldObject.transform.SetParent(emptyObj.transform);
                            }
                            currentHeldObject = null;
                            currentPutbackObject = null;

                            updateObjectHighlights();

                        }
                        // If I am already holding something, and allowed to interact with the currently highlighted thing while there's something in my hand, do that
                        else if (currentInteractableObject && currentInteractableObject.GetComponent<FPEInteractableBaseScript>().interactionsAllowedWhenHoldingObject() == true)
                        {

                            if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.STATIC)
                            {
                                retrieveNote(currentInteractableObject);
                                retrievePassiveAudioDiary(currentInteractableObject, true);
                                currentInteractableObject.GetComponent<FPEInteractableBaseScript>().interact();
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.ACTIVATE)
                            {
                                retrieveNote(currentInteractableObject);
                                retrievePassiveAudioDiary(currentInteractableObject, true);
                                attemptActivation(currentInteractableObject);
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.PICKUP)
                            {
                                // Player can only ever hold one thing, regardless of this value.
                                retrievePassiveAudioDiary(currentInteractableObject, true);
                            }
                            
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.JOURNAL)
                            {
                                // Journals always require two hands
                                retrievePassiveAudioDiary(currentInteractableObject, true);
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.INVENTORY)
                            {
                                // See "Put In Inventory" section below
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.DOCK)
                            {

                                if (currentInteractableObject.GetComponent<FPEInteractableDockScript>().isOccupied() == false)
                                {
                                    DockPlayer(currentInteractableObject.GetComponent<FPEInteractableDockScript>());
                                    retrieveNote(currentInteractableObject);
                                    retrievePassiveAudioDiary(currentInteractableObject, true);
                                }

                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.AUDIODIARY)
                            {
                                if (currentInteractableObject.GetComponent<FPEInteractableAudioDiaryScript>().AutomaticPlayback == false)
                                {
                                    retrieveNote(currentInteractableObject);
                                    // Note: We do NOT retrieve Audio Diary from AUDIO DIARY type, because they are already diaries :)
                                    currentInteractableObject.GetComponent<FPEInteractableAudioDiaryScript>().interact();
                                }
                            }
                            /*
                            // Note: You can add a case here to handle any new interaction types you create that should also allow interactions when object is being held. Be sure to add it to the eInteractionType enum in FPEInteractableBaseScript
                            else if(currentInteractionObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.YOUR_NEW_TYPE_HERE)
                            {
                                // YOUR CUSTOM INTERACTION LOGIC HERE
                            }
                            */
                            else
                            {
                                Debug.LogWarning("FPEInteractionManagerScript:: Unhandled eInteractionType '" + currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() + "' for object that allows interaction when object being held. No case exists to manage this.");
                            }

                            updateObjectHighlights();

                        }
                        // If I am already holding something, and NOT allowed to interact with the currently highlighted thing while there's something in my hand
                        else if (currentInteractableObject && currentInteractableObject.GetComponent<FPEInteractableBaseScript>().interactionsAllowedWhenHoldingObject() == false)
                        {

                            if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.STATIC)
                            {
                                currentInteractableObject.GetComponent<FPEInteractableBaseScript>().interact();
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.ACTIVATE)
                            {
                                // If activation is not permitted, we do nothing
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.PICKUP)
                            {
                                // Player can only ever hold one thing, regardless of this value.
                                if (currentInteractableObject.tag == "下层面包" && currentHeldObject.tag == "芝士")
                                {
                                    PutBurger();
                                }
                                if (currentInteractableObject.tag == "芝士" && currentHeldObject.tag == "番茄")
                                {
                                    PutBurger();
                                }
                                if (currentInteractableObject.tag == "番茄" && currentHeldObject.tag == "肉饼")
                                {
                                    PutBurger();
                                }
                                if (currentInteractableObject.tag == "肉饼" && currentHeldObject.tag == "生菜")
                                {
                                    PutBurger();
                                }
                                if (currentInteractableObject.tag == "生菜" && currentHeldObject.tag == "上层面包")
                                {
                                    PutBurger();
                                }
                                if (currentInteractableObject.tag == "上层面包" && currentHeldObject.tag == "芝麻")
                                {
                                    BurgerPos = GameObject.Find("制作汉堡位置").transform;
                                    PutBurger();
                                    Destroy(emptyObj);
                                    GameObject hanbao = Instantiate(BurgerObj, BurgerPos.parent);
                                    hanbao.transform.position = BurgerPos.position;
                                    emptyObj = new GameObject();
                                }
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.JOURNAL)
                            {
                                // Journals always require two hands
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.INVENTORY)
                            {
                                // See "Put In Inventory" section below
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.DOCK)
                            {
                                // If this dock requires 2 hands, do nothing
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.AUDIODIARY)
                            {
                                // Audio Diaries can always be played when something is in player's hand (see cases above)
                            }
                            /*
                            // Note: You can add a case here to handle any new interaction types you create that should also allow interactions when object is being held. Be sure to add it to the eInteractionType enum in FPEInteractableBaseScript
                            else if(currentInteractionObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.YOUR_NEW_TYPE_HERE)
                            {
                                // YOUR CUSTOM INTERACTION LOGIC HERE
                            }
                            */
                            else
                            {
                                Debug.LogWarning("FPEInteractionManagerScript:: Unhandled eInteractionType '" + currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() + "' for object that DOES NOT allow interaction when object being held. No case exists to manage this.");
                            }

                        }
                        // Otherwise, if we're not putting the object back, just toss it
                        else
                        {
                            currentHeldObject.GetComponent<FPEInteractablePickupScript>().doPickupPutdown(false);
                            tossObject(currentHeldObject.GetComponent<FPEInteractablePickupScript>());
                            currentHeldObject = null;
                            updateObjectHighlights();
                        }

                    }
                    // Not already holding an object //
                    else
                    {
                        // If we're looking at an object, we need to handle the various interaction types (pickup, activate, etc.)
                        if (currentInteractableObject)
                        {

                            lastObjectHeldRotation = Quaternion.identity;

                            if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.PICKUP)
                            {

                                retrieveNote(currentInteractableObject);
                                retrievePassiveAudioDiary(currentInteractableObject, true);

                                currentInteractableObject.GetComponent<FPEInteractablePickupScript>().doPickupPutdown(false);
                                currentInteractableObject.GetComponent<FPEInteractableBaseScript>().unHighlightObject();
                                moveObjectToPlayersHand(currentInteractableObject);
                                currentInteractableObject = null;

                                // Un-zoom, restore mouse state
                                cameraZoomedIn = false;
                                restorePreviousMouseSensitivity(false);

                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.STATIC)
                            {
                                currentInteractableObject.GetComponent<FPEInteractableBaseScript>().interact();
                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.JOURNAL)
                            {

                                currentInteractableObject.GetComponent<FPEInteractableJournalScript>().activateJournal();
                                currentJournal = currentInteractableObject;
                                cameraZoomedIn = false;
                                restorePreviousMouseSensitivity(false);
                                openJournal();
                                retrieveNote(currentInteractableObject);
                                retrievePassiveAudioDiary(currentInteractableObject, true);

                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.ACTIVATE)
                            {

                                retrieveNote(currentInteractableObject);
                                retrievePassiveAudioDiary(currentInteractableObject, true);
                                attemptActivation(currentInteractableObject);

                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.INVENTORY)
                            {

                                // Treat the same as Pickup type, if permitted
                                if (currentInteractableObject.GetComponent<FPEInteractableInventoryItemScript>().PickupPermitted)
                                {

                                    retrieveNote(currentInteractableObject);
                                    retrievePassiveAudioDiary(currentInteractableObject, true);

                                    currentInteractableObject.GetComponent<FPEInteractableInventoryItemScript>().doPickupPutdown(false);
                                    currentInteractableObject.GetComponent<FPEInteractableBaseScript>().unHighlightObject();
                                    moveObjectToPlayersHand(currentInteractableObject);
                                    currentInteractableObject = null;

                                    // Un-zoom, restore mouse state
                                    cameraZoomedIn = false;
                                    restorePreviousMouseSensitivity(false);

                                }

                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.DOCK)
                            {

                                if(currentInteractableObject.GetComponent<FPEInteractableDockScript>().isOccupied() == false)
                                {

                                    DockPlayer(currentInteractableObject.GetComponent<FPEInteractableDockScript>());
                                    retrieveNote(currentInteractableObject);
                                    retrievePassiveAudioDiary(currentInteractableObject, true);

                                }

                            }
                            else if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.AUDIODIARY)
                            {

                                if (currentInteractableObject.GetComponent<FPEInteractableAudioDiaryScript>().AutomaticPlayback == false)
                                {

                                    retrieveNote(currentInteractableObject);
                                    // Note: We do NOT retrieve Audio Diary from AUDIO DIARY type, because they are already diaries :)
                                    currentInteractableObject.GetComponent<FPEInteractableAudioDiaryScript>().interact();

                                }

                            }
                            /*
                            // Note: You can add a case here to handle any new interaction types you create. Be sure to add it to the eInteractionType enum in FPEInteractableBaseScript
                            else if(currentInteractionObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.YOUR_NEW_TYPE_HERE)
                            {
                                // YOUR CUSTOM INTERACTION LOGIC HERE
                            }
                            */
                            else
                            {
                                Debug.LogWarning("FPEInteractionManagerScript:: Unhandled eInteractionType '" + currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() + "'. No case exists to manage this (Which might be just fine, depending).");
                            }

                            updateObjectHighlights();

                        }

                    }

                }
                #endregion

                #region HANDLE_EXAMINATION_START_STOP
                // Examine held object //
                if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_EXAMINE) && currentHeldObject && !dockingInProgress)
                {

                    //hideReticleAndInteractionLabel();
                    examiningObject = true;
                    currentHeldObject.GetComponent<FPEInteractablePickupScript>().startExamination();
                    disableMouseLook();
                    disableMovement();

                    if (currentHeldObject.GetComponent<FPEInteractablePickupScript>().postExaminationInteractionString != "")
                    {
                        currentHeldObject.GetComponent<FPEInteractablePickupScript>().interactionString = currentHeldObject.GetComponent<FPEInteractablePickupScript>().postExaminationInteractionString;
                    }

                    currentHeldObject.transform.position = interactionObjectExamineLocation.transform.position;

                    if (currentHeldObject.GetComponent<FPEInteractablePickupScript>().rotationLockType == FPEInteractablePickupScript.eRotationType.FREE)
                    {

                        if (lastObjectHeldRotation == Quaternion.identity)
                        {

                            Vector3 relativePos = Camera.main.transform.position - currentHeldObject.transform.position;
                            Quaternion rotation = Quaternion.LookRotation(relativePos);
                            currentHeldObject.transform.rotation = rotation;
                            // The first time we pick something up, we apply this additional rotation offset to ensure it is oriented correctly. Subsequent pickups just yield to last rotation made when examining object
                            currentHeldObject.transform.Rotate(currentHeldObject.GetComponent<FPEInteractablePickupScript>().pickupRotationOffset);

                        }
                        else
                        {
                            currentHeldObject.transform.rotation = lastObjectHeldRotation;
                        }

                    }
                    else
                    {

                        Vector3 relativePos = Camera.main.transform.position - currentHeldObject.transform.position;
                        Quaternion rotation = Quaternion.LookRotation(relativePos);
                        currentHeldObject.transform.rotation = rotation;

                    }

                }

                // Stop examining held object //
                if (FPEInputManager.Instance.GetButtonUp(FPEInputManager.eFPEInput.FPE_INPUT_EXAMINE) && currentHeldObject)
                {

                    lastObjectHeldRotation = currentHeldObject.transform.rotation;
                    examiningObject = false;
                    //showReticleAndInteractionLabel();
                    currentHeldObject.GetComponent<FPEInteractablePickupScript>().endExamination();
                    enableMouseLook();
                    enableMovement();

                }

                #endregion

                #region HANDLE_BEHAVIOUR_WHILE_HOLDING_OBJECT
                // Behaviours when holding an object //
                if (currentHeldObject)
                {

                    #region EXAMINING
                    if (examiningObject)
                    {

                        // Examination logic: Position, Rotation, etc.
                        float examinationOffsetUp = currentHeldObject.GetComponent<FPEInteractablePickupScript>().examinationOffsetUp;
                        float examinationOffsetForward = currentHeldObject.GetComponent<FPEInteractablePickupScript>().examinationOffsetForward;
                        currentHeldObject.transform.position = interactionObjectExamineLocation.transform.position + Vector3.up * examinationOffsetUp + interactionObjectExamineLocation.transform.forward * examinationOffsetForward;
                        float rotationInputX = 0.0f;
                        float rotationInputY = 0.0f;

                        float examinationChangeX = FPEInputManager.Instance.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_MOUSELOOKX);
                        float examinationChangeY = FPEInputManager.Instance.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_MOUSELOOKY);

                        // If there was no mouse input use gamepad instead
                        if (examinationChangeX == 0 & examinationChangeY == 0)
                        {

                            // Note: We scale our gamepad by delta time because it's not a "change since last frame" like mouse 
                            // input, so we need to simulate that ourselves.
                            examinationChangeX = FPEInputManager.Instance.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_LOOKX) * Time.deltaTime;
                            examinationChangeY = FPEInputManager.Instance.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_LOOKY) * Time.deltaTime;

                        }

                        if (Mathf.Abs(examinationChangeX) > examinationDeadzone)
                        {
                            rotationInputX = -(examinationChangeX * examineRotationSpeed * Time.deltaTime);
                        }

                        if (Mathf.Abs(examinationChangeY) > examinationDeadzone)
                        {
                            rotationInputY = (examinationChangeY * examineRotationSpeed * Time.deltaTime);
                        }

                        switch (currentHeldObject.GetComponent<FPEInteractablePickupScript>().rotationLockType)
                        {

                            case FPEInteractablePickupScript.eRotationType.FREE:
                                currentHeldObject.transform.Rotate(interactionObjectExamineLocation.transform.up, rotationInputX, Space.World);
                                currentHeldObject.transform.Rotate(interactionObjectExamineLocation.transform.right, rotationInputY, Space.World);
                                break;
                            case FPEInteractablePickupScript.eRotationType.HORIZONTAL:
                                currentHeldObject.transform.Rotate(interactionObjectExamineLocation.transform.up, rotationInputX, Space.World);
                                break;
                            case FPEInteractablePickupScript.eRotationType.VERTICAL:
                                currentHeldObject.transform.Rotate(interactionObjectExamineLocation.transform.right, rotationInputY, Space.World);
                                break;
                            case FPEInteractablePickupScript.eRotationType.NONE:
                            default:
                                break;

                        }

                    }
                    #endregion

                    #region NOT_EXAMINING
                    else
                    {

                        // Update position of object to be that of holding position
                        currentHeldObject.transform.position = interactionObjectPickupLocation.transform.position;
                        // Lerp a bit so it feels less rigid and more like holding something in real life
                        currentHeldObject.transform.rotation = Quaternion.Slerp(currentHeldObject.transform.rotation, interactionObjectPickupLocation.transform.rotation * Quaternion.Euler(lastObjectHeldRotation.eulerAngles + currentHeldObject.GetComponent<FPEInteractablePickupScript>().pickupRotationOffset), 0.2f);

                        updateObjectHighlights();

                    }
                    #endregion

                }
                #endregion

                #region HANDLE_PUT_IN_INVENTORY
                // Put object in inventory //
                if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_PUT_IN_INVENTORY) && !dockingInProgress)
                {

                    // Already holding an object //
                    if (currentHeldObject)
                    {

                        if (!examiningObject)
                        {

                            // Case 1: Player is holding an item in their hand, but looking at another inventory item that they want to take. Note that this case must just yield
                            // to the "take, no pickup allowed" behaviour, as such the mouse hints must change to reflect that.
                            if (currentInteractableObject)
                            {

                                if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.INVENTORY)
                                {
                                    putObjectIntoInventory(currentInteractableObject.GetComponent<FPEInteractableInventoryItemScript>());
                                }
                                else
                                {
                                    putObjectIntoInventory(currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>());
                                    currentHeldObject = null;
                                }

                            }
                            // Case 2: Player is holding something and looking at nothing, so they can put the currently held object into (or back into) inventory
                            else
                            {

                                if (currentHeldObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.INVENTORY)
                                {

                                    putObjectIntoInventory(currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>());
                                    // In this specific case, the thing we were holding was the inventory item, so we need to nullify held object because we're no longer holding it
                                    currentHeldObject = null;

                                }

                            }

                        }

                    }
                    // Not already holding an object //
                    else
                    {

                        // Case 3: Player is looking at an inventory object in the world
                        if (currentInteractableObject)
                        {

                            if (currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() == FPEInteractableBaseScript.eInteractionType.INVENTORY)
                            {
                                putObjectIntoInventory(currentInteractableObject.GetComponent<FPEInteractableInventoryItemScript>());
                            }

                        }

                    }

                    updateObjectHighlights();

                }
                #endregion

                #region HANDLE_CLOSE_AND_MENU_BUTTONS
                // Skip Audio Diary, Close Journal, Undock player (or other future non-menu UI items) //
                if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_CLOSE))
                {

                    if (currentJournal)
                    {
                        closeJournal();
                    }
                    else if (currentDock && !examiningObject)
                    {
                        UnDockPlayer(currentDock.GetComponent<FPEInteractableDockScript>().SmoothDock);
                    }
                    else
                    {

                        // Note it's possible to be playing "Audio Log" style sounds through an Interactable object that is NOT of type AudioDiary (see demoJournal, for example)
                        if (playingAudioDiary)
                        {

                            if (currentAudioDiary)
                            {
                                currentAudioDiary.GetComponent<FPEInteractableAudioDiaryScript>().stopAudioDiary();
                            }

                            fadingDiaryAudio = true;
                            myHUD.FadeAudioDiaryLabel();

                        }

                    }

                }

                // Open Menu or Close Journal) //
                if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_MENU) && !dockingInProgress)
                {

                    if (currentJournal)
                    {
                        closeJournal();
                    }
                    else
                    {

                        if (!examiningObject)
                        {
                            openMenu();
                        }

                    }

                }
                #endregion

                #endregion

                #region DOCKING
                if (dockingInProgress)
                {

                    // Here we wait on the player controller to tell us when the player is done docking.
                    if (dockingCompleted())
                    {

                        dockingInProgress = false;

                        if(currentDockActionType == FPEFirstPersonController.ePlayerDockingState.DOCKING)
                        {
                            currentDock.GetComponent<FPEInteractableDockScript>().finishDocking();
                        }
                        else if(currentDockActionType == FPEFirstPersonController.ePlayerDockingState.UNDOCKING)
                        {
                            currentDock.GetComponent<FPEInteractableDockScript>().finishUnDocking();
                            currentDock = null;
                        }
                        else
                        {
                            Debug.LogWarning("FPEInteractionManagerScript:: Dock action finished, but was not dock or undock, which makes no sense. Did you start a dock using alternate functions?");
                        }

                        unhideAllUI();

                    }

                    updateObjectHighlights();

                }
                #endregion

                #region CAMERA_ZOOM
                // Camera Zoom - don't allow when holding an object or reading a journal //
                if (currentHeldObject == null && currentJournal == null)
                {

                    if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_ZOOM))
                    {
                        setMouseSensitivity(zoomedMouseSensitivity);
                    }
                    if (FPEInputManager.Instance.GetButtonUp(FPEInputManager.eFPEInput.FPE_INPUT_ZOOM))
                    {
                        restorePreviousMouseSensitivity(false);
                    }
                    if (FPEInputManager.Instance.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_ZOOM))
                    {
                        cameraZoomedIn = true;
                    }
                    else
                    {
                        cameraZoomedIn = false;
                    }

                }

                // Change actual camera FOV based on zoom state //
                if (cameraZoomedIn)
                {
                    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, zoomedFOV, cameraZoomFOVChangePerSecond * Time.deltaTime);
                }
                else
                {
                    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, unZoomedFOV, cameraZoomFOVChangePerSecond * Time.deltaTime);
                }
                #endregion

                #region DIARY_MANAGEMENT
                // Fading out diary audio track //
                if (playingAudioDiary && !audioDiaryPlayer.GetComponent<AudioSource>().isPlaying)
                {

                    if (currentAudioDiary)
                    {
                        currentAudioDiary.GetComponent<FPEInteractableAudioDiaryScript>().stopAudioDiary();
                    }

                    hideAudioDiaryTitle();

                }
                #endregion

                // Mouse sensitivity transition smoothing. When slowMouseOnInteractableObjectHighlight is true, we want the mouse sensitivity to somewhat smoothly restore
                // to "full" sensitivity, but not be jarring or "pop" as soon as an object is no longer highlighted by the reticle.
                // Note: Replace this as required if using a different FPS Character Controller like UFPS, etc. that has other integrated mouse sensitivity management and aim assist.
                if (smoothMouseChange)
                {
                    FPEInputManager.Instance.LookSensitivity = Vector2.MoveTowards(FPEInputManager.Instance.LookSensitivity, new Vector2(targetMouseSensitivity.x, targetMouseSensitivity.y), smoothMouseChangeRate * Time.deltaTime);
                }

            }
            else if (currentInteractionState == eInteractionState.SUSPENDED)
            {
                // Nothing, must be released by entity that suspended me
            }

            // We want to fade out audio for the diary regardless of menu state. This covers the case where the player skips a diary, then opens the menu. 
            if (fadingDiaryAudio)
            {

                // We use unscaled delta time in case the player skips then opens the menu. We want the audio to continue fading out.
                fadeCounter += Time.unscaledDeltaTime;

                if (fadeCounter >= 0.1f)
                {

                    audioDiaryPlayer.GetComponent<AudioSource>().volume -= fadeAmountPerTenthSecond;

                    if (audioDiaryPlayer.GetComponent<AudioSource>().volume <= 0.0f)
                    {
                        audioDiaryPlayer.GetComponent<AudioSource>().Stop();
                        fadingDiaryAudio = false;
                    }

                    fadeCounter = 0.0f;

                }

            }

            refreshHUDData();

        }
       

        public void playNewAudioDiary(GameObject diary)
        {

            // if currently playing an audio diary, stop current one, reset text for new one
            if (playingAudioDiary)
            {

                if (currentAudioDiary)
                {
                    currentAudioDiary.GetComponent<FPEInteractableAudioDiaryScript>().stopAudioDiary();
                }

                myHUD.ResetAudioDiaryLabel();

            }

            currentAudioDiary = diary.gameObject;
            playingAudioDiary = true;
            myHUD.StartPlayingNewAudioDiary(diary.GetComponent<FPEInteractableAudioDiaryScript>().audioDiaryTitle, false);
            fadingDiaryAudio = false;
            audioDiaryPlayer.GetComponent<AudioSource>().clip = diary.GetComponent<FPEInteractableAudioDiaryScript>().audioDiaryClip;
            audioDiaryPlayer.GetComponent<AudioSource>().volume = 1.0f;
            audioDiaryPlayer.GetComponent<AudioSource>().Play();

            // Also check to see if we should add an entry in the audio diaries menu for this specific audio diary
            if (diary.GetComponent<FPEInteractableAudioDiaryScript>().AddEntryToInventory)
            {
                inventoryManager.addAudioDiaryEntry(new FPEAudioDiaryEntry(diary.GetComponent<FPEInteractableAudioDiaryScript>().audioDiaryTitle, diary.GetComponent<FPEInteractableAudioDiaryScript>().audioDiaryClip, diary.GetComponent<FPEInteractableAudioDiaryScript>().ShowDiaryTitle));
            }

        }

        public void playAudioDiaryEntry(FPEAudioDiaryEntry diaryEntry, bool isReplay = true)
        {

            // Case where we are interupting an existing diary from in the world
            if (playingAudioDiary)
            {

                if (currentAudioDiary)
                {
                    currentAudioDiary.GetComponent<FPEInteractableAudioDiaryScript>().stopAudioDiary();
                }

                myHUD.ResetAudioDiaryLabel();

            }

            currentAudioDiary = null;
            playingAudioDiary = true;
            myHUD.StartPlayingNewAudioDiary((diaryEntry.ShowDiaryTitle ? diaryEntry.DiaryTitle : ""), isReplay);
            fadingDiaryAudio = false;
            audioDiaryPlayer.GetComponent<AudioSource>().clip = diaryEntry.DiaryAudio;
            audioDiaryPlayer.GetComponent<AudioSource>().volume = 1.0f;
            audioDiaryPlayer.GetComponent<AudioSource>().Play();

        }

        /// <summary>
        /// Provides a means to do a hard stop of all audio diary playback. No text or audio fade outs will occur.
        /// </summary>
        public void stopAllDiaryPlayback()
        {

            if (playingAudioDiary)
            {

                if (currentAudioDiary)
                {
                    currentAudioDiary.GetComponent<FPEInteractableAudioDiaryScript>().stopAudioDiary();
                }

                myHUD.StopPlayingAudioDiary();

                audioDiaryPlayer.GetComponent<AudioSource>().volume = 0.0f;
                audioDiaryPlayer.GetComponent<AudioSource>().Stop();

            }

        }

        public void playSecondaryInteractionAudio(AudioClip secondaryClip, bool playAsAudioDiary, string audioLogText = "")
        {

            // if currently playing an audio diary, stop current one, reset text for new one
            if (playingAudioDiary)
            {

                if (currentAudioDiary)
                {
                    currentAudioDiary.GetComponent<FPEInteractableAudioDiaryScript>().stopAudioDiary();
                }

                myHUD.ResetAudioDiaryLabel();

            }

            if (playAsAudioDiary && audioLogText != "")
            {

                playingAudioDiary = true;
                myHUD.StartPlayingNewAudioDiary(audioLogText, false);
                fadingDiaryAudio = false;
                audioDiaryPlayer.GetComponent<AudioSource>().clip = secondaryClip;
                audioDiaryPlayer.GetComponent<AudioSource>().volume = 1.0f;
                audioDiaryPlayer.GetComponent<AudioSource>().Play();

            }
            else
            {

                secondaryInteractionSoundPlayer.GetComponent<AudioSource>().clip = secondaryClip;
                secondaryInteractionSoundPlayer.GetComponent<AudioSource>().volume = 1.0f;
                secondaryInteractionSoundPlayer.GetComponent<AudioSource>().Play();

            }

        }

        public void stopSecondaryInteractionAudio()
        {
            secondaryInteractionSoundPlayer.GetComponent<AudioSource>().Stop();
        }

        //TODO: Rename this function in the future
        public void hideAudioDiaryTitle()
        {

            currentAudioDiary = null;
            playingAudioDiary = false;
            myHUD.FadeAudioDiaryLabel();

        }

        #region JOURNAL
        public void openJournal()
        {

            disableMovement();
            disableMouseLook();
            setCursorVisibility(true);
            journalSFXPlayer.GetComponent<AudioSource>().clip = journalOpen;
            journalSFXPlayer.GetComponent<AudioSource>().Play();
            myHUD.ShowJournalUI();
            currentJournalPages = currentJournal.GetComponent<FPEInteractableJournalScript>().journalPages;

            if (currentJournalPages.Length > 0)
            {
                myHUD.SetJournalPage(currentJournalPages[currentJournalPageIndex]);
            }
            else
            {
                Debug.LogError("Journal '" + currentJournal.name + "' opened, but was assigned no pages. Assign Sprites to journalPages array in the Inspector.");
            }

        }

        public void nextJournalPage()
        {

            if (currentJournal)
            {

                currentJournalPageIndex++;

                if (currentJournalPageIndex > currentJournalPages.Length - 1)
                {
                    currentJournalPageIndex = currentJournalPages.Length - 1;
                }
                else
                {
                    journalSFXPlayer.GetComponent<AudioSource>().clip = journalPageTurn;
                    journalSFXPlayer.GetComponent<AudioSource>().Play();
                }

                myHUD.SetJournalPage(currentJournalPages[currentJournalPageIndex]);

            }

        }

        public void previousJournalPage()
        {

            if (currentJournal)
            {

                currentJournalPageIndex--;

                if (currentJournalPageIndex < 0)
                {
                    currentJournalPageIndex = 0;
                }
                else
                {
                    journalSFXPlayer.GetComponent<AudioSource>().clip = journalPageTurn;
                    journalSFXPlayer.GetComponent<AudioSource>().Play();
                }

                myHUD.SetJournalPage(currentJournalPages[currentJournalPageIndex]);

            }

        }

        public void closeJournal(bool playSound = true)
        {

            myHUD.HideJournalUI();

            if (currentJournal)
            {

                if (playSound)
                {
                    journalSFXPlayer.GetComponent<AudioSource>().clip = journalClose;
                    journalSFXPlayer.GetComponent<AudioSource>().Play();
                }

                currentJournal.GetComponent<FPEInteractableJournalScript>().deactivateJournal();

            }

            currentJournal = null;
            currentJournalPageIndex = 0;
            currentJournalPages = null;
            setCursorVisibility(false);
            enableMouseLook();
            enableMovement();

        }
        #endregion

        #region MENU
        public void openMenu()
        {

            if (currentInteractionState == eInteractionState.FREE && FPEMenu.Instance != null)
            {

                restorePreviousMouseSensitivity(false);
                FPEMenu.Instance.activateMenu();
                setInteractionState(eInteractionState.IN_MENU);
                Time.timeScale = 0.0f;
                disableMovement();
                disableMouseLook();
                setCursorVisibility(true);

            }

        }

        public void closeMenu()
        {

            if (currentInteractionState == eInteractionState.IN_MENU && FPEMenu.Instance != null)
            {

                FPEMenu.Instance.deactivateMenu();
                setCursorVisibility(false);
                enableMouseLook();
                enableMovement();
                Time.timeScale = 1.0f;
                setInteractionState(eInteractionState.FREE);

                // Try to get cursor lock back when we close the UI
                //OnApplicationFocus(true);


            }

        }
        #endregion

        private void moveObjectToPlayersHand(GameObject go)
        {

            go.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
            go.GetComponent<Rigidbody>().isKinematic = true;

            Collider[] objectColliders = go.GetComponentsInChildren<Collider>();

            foreach (Collider c in objectColliders)
            {
                c.isTrigger = true;
            }

            go.transform.position = interactionObjectPickupLocation.transform.position;

            // We put the pickup into the player's hands so that it is very safely and easily not destroyed when we change levels
            go.transform.parent = interactionObjectPickupLocation.transform;

            currentHeldObject = go;

            // Move to examination layer so object being held/examined doesn't clip through other objects.
            Transform[] objectTransforms = go.GetComponentsInChildren<Transform>();

            foreach (Transform t in objectTransforms)
            {
                t.gameObject.layer = LayerMask.NameToLayer("FPEObjectExamination");
            }

        }

        /// <summary>
        /// Puts specified FPEInteractableInventoryItemScript into inventory. Both updates the internal 
        /// inventory data and physically moves and modifies the object as required.
        /// </summary>
        /// <param name="item">The FPEInteractableInventoryItemScript to add to inventory.</param>
        public void putObjectIntoInventory(FPEInteractableInventoryItemScript item, bool playSound = true)
        {

            // Check for notes about the object
            retrieveNote(item.gameObject);

            // Put object into the actual inventory //
            bool keepOriginal = inventoryManager.addInventoryItem(item);

            if (playSound)
            {

                if (item.InventoryGetSound != null)
                {
                    genericSFXPlayer.GetComponent<AudioSource>().PlayOneShot(item.InventoryGetSound);
                }
                else
                {
                    genericSFXPlayer.GetComponent<AudioSource>().PlayOneShot(inventoryPickup);
                }

            }

            if (keepOriginal)
            {

                // Then move it physically "out of the world" (just move and hide it really) //
                GameObject go = item.gameObject;
                go.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
                go.GetComponent<Rigidbody>().isKinematic = true;

                Collider[] objectColliders = go.GetComponentsInChildren<Collider>();

                foreach (Collider c in objectColliders)
                {
                    c.isTrigger = true;
                }

                go.transform.position = interactionInventoryLocation.transform.position;
                go.transform.parent = interactionInventoryLocation.transform;

                // Move to examination layer so object being held/examined doesn't clip through other objects.
                Transform[] objectTransforms = go.GetComponentsInChildren<Transform>();

                foreach (Transform t in objectTransforms)
                {
                    t.gameObject.layer = LayerMask.NameToLayer("FPEObjectExamination");
                }

                // And disable it
                go.SetActive(false);

            }
            else
            {
                Destroy(item.gameObject);
            }

        }

        /// <summary>
        /// Drops specified inventory item from inventory. Assumes quantity and presence checks have been
        /// made and the object is valid and non-null. You should probably be calling 
        /// FPEInventoryManager.dropItemFromInventory() instead.
        /// </summary>
        /// <param name="itemToDrop">The item to drop</param>
        public void dropObjectFromInventory(FPEInteractableInventoryItemScript itemToDrop)
        {

            if(itemToDrop != null)
            {

                itemToDrop.gameObject.SetActive(true);
                tossObject(itemToDrop);

            }
            else
            {
                Debug.LogError("FPEInteractionManagerScript.dropObjectFromInventory() was given a null GameObject!");
            }

        }

        /// <summary>
        /// Call this function when loading a saved game and you need to place a previously saved object in the player's hand
        /// </summary>
        /// <param name="itemToHold"></param>
        public void holdObjectFromGameLoad(FPEInteractablePickupScript itemToHold)
        {

            itemToHold.SetAsPickedUpForGameLoad();
            moveObjectToPlayersHand(itemToHold.gameObject);

        }

        /// <summary>
        /// This function puts an inventory item into the player's hand
        /// </summary>
        /// <param name="itemToHold"></param>
        public void holdObjectFromInventory(FPEInteractableInventoryItemScript itemToHold)
        {

            FPEInteractableBaseScript.eInteractionType currentHeldType = FPEInteractionManagerScript.Instance.getHeldObjectType();

            // Case 1: Holding nothing
            // Put selected item into player hand
            if (currentHeldType == FPEInteractableBaseScript.eInteractionType.NULL_TYPE)
            {

                itemToHold.gameObject.SetActive(true);
                moveObjectToPlayersHand(itemToHold.gameObject);

            }

            // Case 2: Holding InventoryItem
            // Put held item back into inventory, and put selected item into player hand
            else if (currentHeldType == FPEInteractableBaseScript.eInteractionType.INVENTORY)
            {

                putObjectIntoInventory(currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>());
                itemToHold.gameObject.SetActive(true);
                moveObjectToPlayersHand(itemToHold.gameObject);

            }

            // Case 3: Holding Pickup object
            // Drop held Pickup item, and put selected item into player hand
            else if (currentHeldType == FPEInteractableBaseScript.eInteractionType.PICKUP)
            {

                tossObject(currentHeldObject.GetComponent<FPEInteractablePickupScript>());
                itemToHold.gameObject.SetActive(true);
                moveObjectToPlayersHand(itemToHold.gameObject);

            }

            // Case 4: Not supported
            else
            {
                Debug.Log("FPEInteractionManagerScript.holdObjectFromInventory():: Asked to hold unsupported type '" + currentHeldType + "' object. Nothing will be held.");
            }

        }

        /// <summary>
        /// Consumes an inventory item and fires its consumer script
        /// </summary>
        /// <param name="itemToConsume">The inventory item to consume</param>
        public void consumeObjectFromInventory(FPEInteractableInventoryItemScript itemToConsume)
        {

            FPEInventoryConsumer consumer = itemToConsume.gameObject.GetComponent<FPEInventoryConsumer>();
            if (consumer)
            {
                itemToConsume.gameObject.SetActive(true);
                consumer.consumeItem();
            }
            else
            {
                Debug.LogError("FPEInteractionManagerScript.consumeObjectFromInventory() asked to consume item '"+ itemToConsume.gameObject.name + "' but this item has no FPEInventoryConsumer attached. Destroying item instead!", itemToConsume.gameObject);
            }

        }


        /// <summary>
        /// Tosses any Pickup type Interactable object from the designated toss location. Note that 
        /// calling this a lot in short succession will likely result in physics weirdness if the 
        /// objects all overlap.
        /// </summary>
        /// <param name="pickup">The Pickup object you want to toss</param>
        public void tossObject(FPEInteractablePickupScript pickup)
        {

            GameObject go = pickup.gameObject;
            go.transform.parent = null;

            Collider[] objectColliders = go.GetComponentsInChildren<Collider>();

            foreach (Collider c in objectColliders)
            {
                c.isTrigger = false;
            }

            go.GetComponent<Rigidbody>().isKinematic = false;
            go.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Note: We move objects to a special toss location to prevent clipping into the player if the player tosses the object while walking forward
            float tossStrength = go.GetComponent<FPEInteractablePickupScript>().tossStrength;
            float tossOffsetUp = go.GetComponent<FPEInteractablePickupScript>().tossOffsetUp;
            float tossOffsetForward = go.GetComponent<FPEInteractablePickupScript>().tossOffsetForward;
            go.transform.position = interactionObjectTossLocation.transform.position + Vector3.up * tossOffsetUp + Camera.main.transform.forward * tossOffsetForward;
            go.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * tossImpulseFactor * tossStrength, ForceMode.Impulse);

            Transform[] objectTransforms = go.GetComponentsInChildren<Transform>();

            foreach (Transform t in objectTransforms)
            {
                t.gameObject.layer = LayerMask.NameToLayer("FPEPickupObjects");
            }

            go.GetComponent<FPEInteractablePickupScript>().drop();

        }

        public void PutBurger()
        {
            currentHeldObject.GetComponent<FPEInteractablePickupScript>().doPickupPutdown(true);
            currentHeldObject.transform.position = new Vector3(currentInteractableObject.transform.position.x, currentInteractableObject.transform.position.y + 0.01f, currentInteractableObject.transform.position.z);
            currentHeldObject.transform.rotation = currentInteractableObject.transform.rotation;
            currentHeldObject.transform.parent = emptyObj.transform;
            //currentHeldObject.GetComponent<Collider>().isTrigger = false;
            //currentHeldObject.GetComponent<Rigidbody>().isKinematic = false;
            //currentHeldObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
            //currentHeldObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //currentHeldObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            Transform[] objectTransforms = currentHeldObject.GetComponentsInChildren<Transform>();

            foreach (Transform t in objectTransforms)
            {
                t.gameObject.layer = LayerMask.NameToLayer("FPEPickupObjects");
            }
            //Destroy(currentInteractableObject.GetComponent<FPEInteractablePickupScript>());
            //给currentHeldObject创建一个空的父物体
            currentHeldObject = null;

            updateObjectHighlights();
        }



        /// <summary>
        /// Checks current held object and returns its interaction type
        /// </summary>
        /// <returns>interaction type of current held object, or NULL_TYPE if nothing is being held</returns>
        public FPEInteractableBaseScript.eInteractionType getHeldObjectType()
        {
            return ((currentHeldObject == null) ? FPEInteractableBaseScript.eInteractionType.NULL_TYPE : currentHeldObject.GetComponent<FPEInteractableBaseScript>().getInteractionType());
        }


        /// <summary>
        /// Get current held object. Should only really be called by save game logic handler, and object should 
        /// not be modified outside this class and expected to work correctly.
        /// </summary>
        /// <returns>currently held Game Object, or null of no object is being held</returns>
        public GameObject getHeldObject()
        {
            return currentHeldObject;
        }

        /// <summary>
        /// Will destroy the currently held object, if it exists
        /// </summary>
        public void DestroyHeldObject()
        {

            if (currentHeldObject)
            {

                Destroy(currentHeldObject.gameObject);
                currentHeldObject = null;

            }

        }

        private void retrieveNote(GameObject interactableObject)
        {

            if (interactableObject.gameObject.GetComponent<FPEAttachedNote>() && !interactableObject.gameObject.GetComponent<FPEAttachedNote>().Collected)
            {
                showNotification("新笔记'" + interactableObject.gameObject.GetComponent<FPEAttachedNote>().NoteTitle + "'被添加");
                genericSFXPlayer.GetComponent<AudioSource>().PlayOneShot(noteAdded);
                inventoryManager.addNoteEntry(interactableObject.gameObject.GetComponent<FPEAttachedNote>().collectNote());
            }

        }


        private void retrievePassiveAudioDiary(GameObject interactableObject, bool wasInteractedWith)
        {

            if (interactableObject.gameObject.GetComponent<FPEPassiveAudioDiary>())
            {

                // Case 1: Player interacted with parent object, in which case we play in both autoplay and manual play scenarios
                // Case 2: Player just looked at parent object, so only play if autoplay is supported
                if ((wasInteractedWith == true) || ((wasInteractedWith == false) && (interactableObject.gameObject.GetComponent<FPEPassiveAudioDiary>().AutomaticPlayback == true)))
                {

                    if (interactableObject.gameObject.GetComponent<FPEPassiveAudioDiary>().HasBeenPlayed == false)
                    {

                        interactableObject.gameObject.GetComponent<FPEPassiveAudioDiary>().collectAudioDiary();
                        FPEAudioDiaryEntry tempEntry = new FPEAudioDiaryEntry(interactableObject.gameObject.GetComponent<FPEPassiveAudioDiary>().DiaryTitle, interactableObject.gameObject.GetComponent<FPEPassiveAudioDiary>().DiaryAudio, interactableObject.gameObject.GetComponent<FPEPassiveAudioDiary>().ShowDiaryTitle);
                        playAudioDiaryEntry(tempEntry, false);

                        if (interactableObject.gameObject.GetComponent<FPEPassiveAudioDiary>().AddEntryToInventory == true)
                        {
                            inventoryManager.addAudioDiaryEntry(tempEntry);
                        }

                    }

                }

            }

        }

        private void attemptActivation(GameObject objectToActivate)
        {

            FPEInteractableActivateScript activatee = objectToActivate.GetComponent<FPEInteractableActivateScript>();
            if (!activatee)
            {
                Debug.Log("FPEInteractionManagerScript.attemptActivation():: Given non-Activate type object called '"+objectToActivate.name+"'. No activation will take place.");
            }
            else
            {

                // Case 1: It requires inventory
                // OR
                // Case 2: It's a toggle, and it's toggled ON right now, AND toggling off requires inventory
                if ((activatee.RequireInventoryItem) || (activatee.EventFireType == FPEGenericEvent.eEventFireType.TOGGLE && activatee.IsToggledOn && activatee.ToggleOffRequiresInventory) )
                {

                    // Required in hand
                    if (activatee.RequiredToBeInHand == FPEInteractableActivateScript.eInventoryRequirementMode.IN_HAND)
                    {

                        if ((currentHeldObject != null && currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>() && currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>().InventoryItemType == activatee.RequiredItemType))
                        {

                            if (activatee.RemoveOnUse)
                            {
                                Destroy(currentHeldObject);
                                currentHeldObject = null;
                            }

                            currentInteractableObject.GetComponent<FPEInteractableActivateScript>().activate();

                        }
                        else
                        {
                            currentInteractableObject.GetComponent<FPEInteractableActivateScript>().failToActivate();
                        }

                    }
                    // Required in inventory
                    else if(activatee.RequiredToBeInHand == FPEInteractableActivateScript.eInventoryRequirementMode.IN_INVENTORY)
                    {

                        if (inventoryManager.inventoryQuantity(activatee.RequiredItemType) >= activatee.RequiredInventoryQuantity)
                        {

                            if (activatee.RemoveOnUse)
                            {
                                inventoryManager.destroyInventoryItemsOfType(activatee.RequiredItemType, activatee.RequiredInventoryQuantity);
                            }

                            currentInteractableObject.GetComponent<FPEInteractableActivateScript>().activate();

                        }
                        // Otherwise, we didn't have it, so react accordingly
                        else
                        {
                            currentInteractableObject.GetComponent<FPEInteractableActivateScript>().failToActivate();
                        }

                    }
                    // Can either be in hand or in inventory
                    else
                    {

                        // If it's in hand, do that first
                        if ((currentHeldObject != null && currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>() && currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>().InventoryItemType == activatee.RequiredItemType))
                        {

                            if (activatee.RemoveOnUse)
                            {
                                Destroy(currentHeldObject);
                                currentHeldObject = null;
                            }

                            currentInteractableObject.GetComponent<FPEInteractableActivateScript>().activate();

                        }
                        // Otherwise if it's in inventory, do that
                        else if (inventoryManager.inventoryQuantity(activatee.RequiredItemType) >= activatee.RequiredInventoryQuantity)
                        {

                            if (activatee.RemoveOnUse)
                            {
                                inventoryManager.destroyInventoryItemsOfType(activatee.RequiredItemType, activatee.RequiredInventoryQuantity);
                            }

                            currentInteractableObject.GetComponent<FPEInteractableActivateScript>().activate();

                        }
                        // Otherwise, cannot activate, do activation fail
                        else
                        {
                            currentInteractableObject.GetComponent<FPEInteractableActivateScript>().failToActivate();
                        }

                    }

                }
                else
                {
                    currentInteractableObject.GetComponent<FPEInteractableActivateScript>().activate();
                }

            }

        }


        private void showNotification(string message)
        {
            myHUD.ShowNotification(message);
        }

        #region CURSOR_AND_MOUSE

        /// <summary>
        /// This function updates some special case object highlighting and unhighlighting based on what the player is looking at.
        /// </summary>
        private void updateObjectHighlights()
        {

            FPEInteractableBaseScript.eInteractionType heldType = (currentHeldObject != null) ? currentHeldObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() : FPEInteractableBaseScript.eInteractionType.NULL_TYPE;
            FPEInteractableBaseScript.eInteractionType lookedAtType = (currentInteractableObject != null) ? currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() : FPEInteractableBaseScript.eInteractionType.NULL_TYPE;

            if (currentPutbackObject != null)
            {
                lookedAtType = FPEInteractableBaseScript.eInteractionType.PUT_BACK;
            }

            #region HINT_HOLDING_NOTHING
            if (heldType == FPEInteractableBaseScript.eInteractionType.NULL_TYPE)
            {

                switch (lookedAtType)
                {

                    case FPEInteractableBaseScript.eInteractionType.STATIC:
                        currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PICKUP:
                        currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        break;

                    case FPEInteractableBaseScript.eInteractionType.ACTIVATE:
                        currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        break;

                    case FPEInteractableBaseScript.eInteractionType.JOURNAL:
                        currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        break;

                    case FPEInteractableBaseScript.eInteractionType.AUDIODIARY:

                        if (currentInteractableObject.GetComponent<FPEInteractableAudioDiaryScript>().AutomaticPlayback == true)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }
                        else
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }

                        break;

                    case FPEInteractableBaseScript.eInteractionType.INVENTORY:
                        currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        break;

                    case FPEInteractableBaseScript.eInteractionType.DOCK:
                        if (currentInteractableObject.GetComponent<FPEInteractableDockScript>().isOccupied() == false)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }
                        break;

                    // Holding Nothing, Looking at Nothing //
                    case FPEInteractableBaseScript.eInteractionType.PUT_BACK:
                    case FPEInteractableBaseScript.eInteractionType.NULL_TYPE:
                        if (currentInteractableObject)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().unHighlightObject();
                        }
                        break;

                    default:
                        Debug.LogError("FPEInteractionManagerScript:: Given bad eInteractionType '" + lookedAtType + "'.");
                        break;

                }

            }
            #endregion

            #region HINT_HOLDING_PICKUP
            else if (heldType == FPEInteractableBaseScript.eInteractionType.PICKUP)
            {

                switch (lookedAtType)
                {

                    case FPEInteractableBaseScript.eInteractionType.STATIC:
                    case FPEInteractableBaseScript.eInteractionType.PICKUP:
                    case FPEInteractableBaseScript.eInteractionType.PUT_BACK:
                        break;

                    case FPEInteractableBaseScript.eInteractionType.ACTIVATE:
                        if (currentInteractableObject.GetComponent<FPEInteractableActivateScript>().interactionsAllowedWhenHoldingObject())
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.JOURNAL:
                    case FPEInteractableBaseScript.eInteractionType.INVENTORY:
                        currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        break;

                    case FPEInteractableBaseScript.eInteractionType.AUDIODIARY:
                        if (currentInteractableObject.GetComponent<FPEInteractableAudioDiaryScript>().AutomaticPlayback == true)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }
                        else
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.DOCK:
                        if (currentInteractableObject.GetComponent<FPEInteractableDockScript>().isOccupied() == false)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }
                        break;

                    // Holding PICKUP, looking at nothing //
                    case FPEInteractableBaseScript.eInteractionType.NULL_TYPE:
                        if (currentInteractableObject)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().unHighlightObject();
                        }
                        break;

                    default:
                        Debug.LogError("FPEInteractionManagerScript:: Given bad eInteractionType '" + lookedAtType + "'.");
                        break;

                }

            }
            #endregion

            #region HINT_HOLDING_INVENTORY
            else if (heldType == FPEInteractableBaseScript.eInteractionType.INVENTORY)
            {

                switch (lookedAtType)
                {

                    case FPEInteractableBaseScript.eInteractionType.STATIC:
                    case FPEInteractableBaseScript.eInteractionType.PICKUP:
                    case FPEInteractableBaseScript.eInteractionType.PUT_BACK:
                        break;

                    case FPEInteractableBaseScript.eInteractionType.ACTIVATE:
                        if (currentInteractableObject.GetComponent<FPEInteractableActivateScript>().interactionsAllowedWhenHoldingObject())
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.JOURNAL:
                    case FPEInteractableBaseScript.eInteractionType.INVENTORY:
                        currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        break;

                    case FPEInteractableBaseScript.eInteractionType.AUDIODIARY:
                        if (currentInteractableObject.GetComponent<FPEInteractableAudioDiaryScript>().AutomaticPlayback == true)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }
                        else
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.DOCK:
                        if (currentInteractableObject.GetComponent<FPEInteractableDockScript>().isOccupied() == false)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().highlightObject();
                        }
                        break;

                    // Holding INVENTORY, looking at nothing //
                    case FPEInteractableBaseScript.eInteractionType.NULL_TYPE:
                        if (currentInteractableObject)
                        {
                            currentInteractableObject.GetComponent<FPEInteractableBaseScript>().unHighlightObject();
                        }
                        break;

                    default:
                        Debug.LogError("FPEInteractionManagerScript:: Given bad eInteractionType '" + lookedAtType + "'.");
                        break;

                }

            }
            #endregion

            else
            {
                Debug.LogError("FPEInteractionManagerScript.updateControlHints():: Player is holding object of type '" + heldType + "' which is not implemented.");
            }

        }

        private void refreshHUDData()
        {

            // General
            myHUDData.examiningObject = examiningObject;
            myHUDData.zoomedIn = cameraZoomedIn;
            
            // Dock
            if (currentDock != null)
            {
                myHUDData.dockedRightNow = true;
                myHUDData.currentDockHint = currentDock.GetComponent<FPEInteractableDockScript>().DockHintText;
                myHUDData.currentUndockHint = currentDock.GetComponent<FPEInteractableDockScript>().UnDockHintText;
            }
            else
            {
                myHUDData.dockedRightNow = false;
                myHUDData.currentDockHint = "";
                myHUDData.currentUndockHint = "";
            }

            myHUDData.dockTransitionHappeningRightNow = dockingInProgress;

            // Held/LookedAt Types
            myHUDData.heldType = (currentHeldObject != null) ? currentHeldObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() : FPEInteractableBaseScript.eInteractionType.NULL_TYPE;
            myHUDData.lookedAtType = (currentInteractableObject != null) ? currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() : FPEInteractableBaseScript.eInteractionType.NULL_TYPE;

            if (currentPutbackObject != null)
            {
                myHUDData.lookedAtType = FPEInteractableBaseScript.eInteractionType.PUT_BACK;
            }

            // Held Object Info
            if (currentHeldObject != null)
            {

                myHUDData.heldObjectInteractionString = currentHeldObject.GetComponent<FPEInteractablePickupScript>().interactionString;
                myHUDData.heldObjectinteractionsAllowedWhenHoldingObject = currentHeldObject.GetComponent<FPEInteractablePickupScript>().interactionsAllowedWhenHoldingObject();

                if (currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>())
                {
                    myHUDData.heldObjectInventoryItemName = currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>().ItemName;
                }

            }
            else
            {

                myHUDData.heldObjectInteractionString = "";
                myHUDData.heldObjectinteractionsAllowedWhenHoldingObject = false;
                myHUDData.heldObjectInventoryItemName = "";

            }

            // Looked At Object Info
            if(currentInteractableObject != null)
            {

                myHUDData.lookedAtInteractionString = currentInteractableObject.GetComponent<FPEInteractableBaseScript>().interactionString;

                if (currentInteractableObject.GetComponent<FPEInteractableInventoryItemScript>())
                {
                    myHUDData.lookedAtInventoryPickupPermitted = currentInteractableObject.GetComponent<FPEInteractableInventoryItemScript>().PickupPermitted;
                    myHUDData.lookedAtInventoryItemName = currentInteractableObject.GetComponent<FPEInteractableInventoryItemScript>().ItemName;
                }

                if (currentInteractableObject.GetComponent<FPEInteractableDockScript>())
                {
                    myHUDData.lookedAtDockHint = currentInteractableObject.GetComponent<FPEInteractableDockScript>().DockHintText;
                    myHUDData.lookedAtDockOccupied = currentInteractableObject.GetComponent<FPEInteractableDockScript>().Occupied;
                }

                if (currentInteractableObject.GetComponent<FPEInteractablePickupScript>())
                {
                    myHUDData.lookedAtPickupInteractionsAllowedWhenHoldingObject = currentInteractableObject.GetComponent<FPEInteractablePickupScript>().interactionsAllowedWhenHoldingObject();
                }

                if (currentInteractableObject.GetComponent<FPEInteractableActivateScript>())
                {
                    myHUDData.lookedAtActivateAllowedWhenHoldingObject = currentInteractableObject.GetComponent<FPEInteractableActivateScript>().interactionsAllowedWhenHoldingObject();
                }

                if (currentInteractableObject.GetComponent<FPEInteractableAudioDiaryScript>())
                {
                    myHUDData.lookedAtAudioDiaryAutoPlay = currentInteractableObject.GetComponent<FPEInteractableAudioDiaryScript>().AutomaticPlayback;
                }

            }
            else
            {

                myHUDData.lookedAtInteractionString = "";
                myHUDData.lookedAtInventoryPickupPermitted = false;
                myHUDData.lookedAtInventoryItemName = "";
                myHUDData.lookedAtDockHint = "";
                myHUDData.lookedAtDockOccupied = false;
                myHUDData.lookedAtPickupInteractionsAllowedWhenHoldingObject = false;
                myHUDData.lookedAtAudioDiaryAutoPlay = false;

            }

            // Special Case: If holding something that can be put back, and looking at a put back location that matches, we need a put back string
            if (currentPutbackObject != null)
            {

                if (currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>())
                {

                    string myCustomPutbackString = currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>().putBackString;

                    // Sort of inefficient, but we want to make sure the custom string is in fact custom. To optimize, the pickup default
                    // put back string could just be set to blank inside FPEInteractablePickupScript.cs, (~line 24)
                    if (myCustomPutbackString == "" || myCustomPutbackString == "<DEFAULT PUT BACK STRING>")
                    {

                        myHUDData.usingCustomLookedAtInteractionString = false;
                        myHUDData.lookedAtPickupPutbackString = currentHeldObject.GetComponent<FPEInteractableInventoryItemScript>().ItemName;

                    }
                    else
                    {

                        myHUDData.usingCustomLookedAtInteractionString = true;
                        myHUDData.lookedAtPickupPutbackString = myCustomPutbackString;

                    }


                }
                else
                {
                    myHUDData.lookedAtPickupPutbackString = currentHeldObject.GetComponent<FPEInteractablePickupScript>().putBackString;
                }

            }
            else
            {
                myHUDData.lookedAtPickupPutbackString = "";
            }

            // Audio Diary Playback
            myHUDData.audioDiaryPlayingRightNow = playingAudioDiary;
            myHUDData.audioDiaryIsReplay = audioDiaryPlaybackIsReplay;
            myHUDData.audioDiaryTitle = currentDiaryTitle;

        }

        /// <summary>
        /// Returns the latest HUD Data.
        /// </summary>
        /// <returns></returns>
        public FPEHUDData GetHUDData()
        {
            return myHUDData;
        }

        /// <summary>
        /// Creates and returns an array of strings based on current interactions. Can be used for debugging or other 
        /// UI outputs as required. Order is [currentInteractable.name, currentInteractble.type, currentPutBack.name, 
        /// PUT_BACK type, currentHeld.name, currentHeld.type]
        /// </summary>
        /// <returns>Array of strings containing alternating Name and Type for each of Current Interactable, Current Put Back, and Current Held Objects. If name strings are blank, the object was null.</returns>
        public string[] FetchCurrentInteractionDebugData()
        {

            string[] debugData = new string[6];

            debugData[0] = (currentInteractableObject != null) ? currentInteractableObject.name : "";
            debugData[1] = (currentInteractableObject != null) ? ""+currentInteractableObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() : ""+FPEInteractableBaseScript.eInteractionType.NULL_TYPE;
            debugData[2] = (currentPutbackObject != null) ? currentPutbackObject.name : "";
            debugData[3] = "" + FPEInteractableBaseScript.eInteractionType.PUT_BACK;
            debugData[4] = (currentHeldObject != null) ? currentHeldObject.name : "";
            debugData[5] = (currentHeldObject != null) ? "" + currentHeldObject.GetComponent<FPEInteractableBaseScript>().getInteractionType() : "" + FPEInteractableBaseScript.eInteractionType.NULL_TYPE;

            return debugData;

        }


        private void setCursorVisibility(bool visible)
        {

            Cursor.visible = visible;

            if (visible)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

        }

        // Note: This feature is only supported when using First Person Exploratino Kit with Unity 5 and above.
        // This function assumes that the top left pixel is the hotspot. To change that, simply pass in the hotspot
        // location rather than Vector2.zero.
        private void setCursorTexture(Texture2D cursorTex)
        {
            Cursor.SetCursor(cursorTex, Vector2.zero, CursorMode.ForceSoftware);
        }
        #endregion

        // TODO: Rename this function to something more appropriate
        public void unhideAllUI()
        {

            updateObjectHighlights();
            refreshHUDData();

        }

        private void OnApplicationFocus(bool hasFocus)
        {

            // We have focus and the menu is not open and we don't have cursor lock, so ask for cursor lock
            if (hasFocus && (currentInteractionState != eInteractionState.IN_MENU && currentInteractionState != eInteractionState.SUSPENDED) && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

        }

        /// <summary>
        /// Suspends the player and yields to the calling entity. The player will remain suspended until EndCutScene() is called
        /// </summary>
        /// <param name="showCursor">If true, cursor will be shown. Good for cases where you want to remove cursor lock so player can select UI elements, etc. Defaults to false.</param>
        public void BeginCutscene(bool showCursor = false)
        {

            setInteractionState(eInteractionState.SUSPENDED);

            disableMouseLook();
            disableMovement();
            thePlayer.GetComponent<FPEFirstPersonController>().playerFrozen = true;

            if (showCursor)
            {
                setCursorVisibility(true);
            }

        }

        /// <summary>
        /// Frees the player from previous Suspended state initiated by BeginCutscene(). 
        /// </summary>
        /// <param name="hideCursor">If true, cursor will be locked/hidden once player is freed. Default is false.</param>
        public void EndCutscene(bool hideCursor = false)
        {

            setInteractionState(eInteractionState.FREE);

            enableMouseLook();
            enableMovement();
            thePlayer.GetComponent<FPEFirstPersonController>().playerFrozen = false;

            if (hideCursor)
            {
                setCursorVisibility(false);
            }

            // Handle some possible edge cases
            examiningObject = false;

        }

        private void setInteractionState(eInteractionState newState)
        {

            currentInteractionState = newState;
            myHUD.InteractionStateChangeTo(currentInteractionState);

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Customize the body of these functions as required for your Character Controller code of choice. If using something //
        // like UFPS, you may want to overhaul these completely. If you need help, please email support@whilefun.com          //
        //                                                                                                                    //
        // Note: The Dock-related functions may require non-trivial changes to your player controller scripts. To see how you //
        //       can implement this fucntionality in your own scripts, please refer to FPEFirstPersonController.cs            //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region PLAYER_CONTROLLER_SPECIFIC

        // This function records our starting sensitivity.
        // The Standard Asset version of the MouseLook script uses X on Character Controller and Y on Camera.
        private void rememberStartingMouseSensitivity()
        {
            startingMouseSensitivity = FPEInputManager.Instance.LookSensitivity;
        }
        public void refreshAlternateMouseSensitivities()
        {
            zoomedMouseSensitivity = startingMouseSensitivity * zoomedMouseSensitivityMultiplier;
            highlightedMouseSensitivity = startingMouseSensitivity * highlightedMouseSensitivityMultiplier;
        }
        // Set sensitivity directly, and ensure smoothMouseChange is off.
        private void setMouseSensitivity(Vector2 sensitivity)
        {
            FPEInputManager.Instance.LookSensitivity = sensitivity;
            smoothMouseChange = false;
        }
        // Restores mouse sensitivity to starting Mouse sensitivity
        // Vector2 is desired sensitivity. If smoothTransition is true, sensitivity 
        // change is gradual. Otherwise, it is changed immediately.
        private void restorePreviousMouseSensitivity(bool smoothTransition)
        {

            if (smoothTransition)
            {
                targetMouseSensitivity.x = startingMouseSensitivity.x;
                targetMouseSensitivity.y = startingMouseSensitivity.y;
                smoothMouseChange = true;
            }
            else
            {
                FPEInputManager.Instance.LookSensitivity = startingMouseSensitivity;
                smoothMouseChange = false;
            }

        }
        // A hook for a menu or UI to set the mouse sensitivity during the game. Note in this case, both X and Y are set to the
        // same value to simplify the UI. This can be done a different way, respecting X and Y as separate values if desired.
        public void changeMouseSensitivityFromMenu(float sensitivity)
        {
            FPEInputManager.Instance.LookSensitivity = new Vector2(sensitivity, sensitivity);
            rememberStartingMouseSensitivity();
            refreshAlternateMouseSensitivities();
            smoothMouseChange = false;
        }
        // Locks mouse look, so we can move mouse to rotate objects when examining them.
        // If using another Character Controller (UFPS, etc.) substitute mouselook disable functionality
        private void disableMouseLook()
        {
            thePlayer.GetComponent<FPEMouseLook>().enableMouseLook = false;
            mouseLookEnabled = false;
        }
        // Unlocks mouse look so we can move mouse to look when walking/moving normally.
        // If using another Character Controller (UFPS, etc.) substitute mouselook enable functionality
        private void enableMouseLook()
        {
            thePlayer.GetComponent<FPEMouseLook>().enableMouseLook = true;
            mouseLookEnabled = true;
        }
        // Locks movement of Character Controller. 
        // If using another Character Controller (UFPS, etc.) substitute disable functionality
        private void disableMovement()
        {
            thePlayer.GetComponent<FPEFirstPersonController>().disableMovement();
        }
        // Unlocks movement of Character Controller. 
        // If using another Character Controller (UFPS, etc.) substitute enable functionality
        private void enableMovement()
        {
            thePlayer.GetComponent<FPEFirstPersonController>().enableMovement();
        }

        public bool isMouseLookEnabled()
        {
            return mouseLookEnabled;
        }

        /// <summary>
        /// Starts docking the player to the specified dock
        /// </summary>
        /// <param name="dock"></param>
        private void DockPlayer(FPEInteractableDockScript dock)
        {

            currentDock = dock.gameObject;
            dock.dock();
            thePlayer.GetComponent<FPEFirstPersonController>().dockThePlayer(dock.DockTransform, dock.DockedViewLimits, dock.FocusTransform.position, dock.SmoothDock);
            currentDockActionType = FPEFirstPersonController.ePlayerDockingState.DOCKING;
            dockingInProgress = true;

        }

        /// <summary>
        /// Starts to Un-Dock the player from their current Dock. 
        /// Note: converting this function to work with assets such as UFPS may be a non-trivial exercise. See FPEFirstPersonController.cs for details on existing implementation.
        /// </summary>
        /// <param name="smoothDock">If true, player view and position will smoothly lerp based on FPEFirstPersonController's Docking Lerp Factor value. Defaults to false.</param>
        private void UnDockPlayer(bool smoothDock = false)
        {

            thePlayer.GetComponent<FPEFirstPersonController>().unDockThePlayer(smoothDock);
            currentDock.GetComponent<FPEInteractableDockScript>().unDock();
            currentDockActionType = FPEFirstPersonController.ePlayerDockingState.UNDOCKING;
            dockingInProgress = true;

        }

        /// <summary>
        /// Checks with player controller to see if dock is currently in progress
        /// </summary>
        /// <returns>True if docking is in progress, false if it is not</returns>
        private bool dockingCompleted()
        {
            return !thePlayer.GetComponent<FPEFirstPersonController>().dockInProgress();
        }

        /// <summary>
        /// For use by Save Load Manager.
        /// </summary>
        /// <returns>the currentDock (can be null)</returns>
        public GameObject getCurrentDockForSaveGame()
        {
            return currentDock;
        }

        /// <summary>
        /// For use by Save Load Manager only.
        /// </summary>
        /// <param name="cd">Gameobject to assign to current dock (can be null)</param>
        public void restoreCurrentDockFromSavedGame(GameObject cd)
        {

            currentDock = cd;
            dockingInProgress = false;

        }

        /// <summary>
        /// Restricts player's look ability to be +/- specified x and y angles, relative to current reticle position.
        /// </summary>
        /// <param name="angles">The angles or bounds that will limit player view, +/-</param>
        public void RestrictPlayerLookFromCurrentView(Vector2 angles)
        {
            thePlayer.GetComponent<FPEMouseLook>().enableLookRestriction(angles);
        }

        /// <summary>
        /// Removes any existing view restriction on player's view.
        /// </summary>
        public void FreePlayerLookFromCurrentViewRestrictions()
        {
            thePlayer.GetComponent<FPEMouseLook>().disableLookRestriction();
        }

        /// <summary>
        /// This function disables player movement and look, for use by SaveLoadManager or other operation that requires player be 'locked'
        /// </summary>
        public void suspendPlayerAndInteraction()
        {

            playerSuspendedForSaveLoad = true;
            disableMouseLook();
            disableMovement();

            thePlayer.GetComponent<FPEFirstPersonController>().playerFrozen = true;

            setInteractionState(eInteractionState.SUSPENDED);
            setCursorVisibility(true);

        }

        /// <summary>
        /// This function enables player movement and look, for use by SaveLoadManager or other operation when it no longer requires player be 'locked'
        /// </summary>
        public void resumePlayerAndInteraction(bool resetLook)
        {

            if (resetLook)
            {
                FPEPlayer.Instance.GetComponent<FPEFirstPersonController>().setPlayerLookToNeutralLevelLoadedPosition();
            }

            playerSuspendedForSaveLoad = false;

            enableMouseLook();
            enableMovement();

            thePlayer.GetComponent<FPEFirstPersonController>().playerFrozen = false;

            setInteractionState(eInteractionState.FREE);
            setCursorVisibility(false);

        }

        /// <summary>
        /// A special function used by FPESaveLoadManager to ensure player states are reset when returning 
        /// to the main menu. Made for special New Game -> Dock -> Exit to Menu -> New Game edge case.
        /// </summary>
        public void resetPlayerOnReturnToMainMenu()
        {
            FPEPlayer.Instance.GetComponent<FPEFirstPersonController>().resetPlayerForMainMenu();
                    }

        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {

            if (drawDebugGizmos)
            {

                Vector3 iconPosition = FPEPlayer.Instance.transform.position + Vector3.up * 1.0f;

                switch (currentInteractionState)
                {

                    case eInteractionState.FREE:
                        Gizmos.DrawIcon(iconPosition, "Whilefun/playerFree.png", false);
                        break;
                    case eInteractionState.IN_MENU:
                        Gizmos.DrawIcon(iconPosition, "Whilefun/playerInMenu.png", false);
                        break;
                    case eInteractionState.SUSPENDED:
                        Gizmos.DrawIcon(iconPosition, "Whilefun/playerSuspended.png", false);
                        break;
                }

            }

        }

#endif

    }

}