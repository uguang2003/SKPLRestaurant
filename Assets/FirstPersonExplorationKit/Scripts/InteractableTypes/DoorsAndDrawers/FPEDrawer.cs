using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEDrawer
    // This script is used for creating drawers in cabinets, dressers, desks, etc.
    //
    // To use, place onto empty game object, and add SlidingPart child. The SlidingPart houses the drawer mesh, colliders, FPEDoorAndDrawerHitHelpers, FPEDrawerContentsGrabber, etc.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [ExecuteInEditMode]
    public class FPEDrawer : MonoBehaviour
    {

        public enum eDrawerState
        {

            CLOSED = 0,
            CLOSING,
            OPENING,
            OPEN,
            BLOCKED_PARTLY_OPEN,
            BLOCKED_PARTLY_CLOSED

        }
        protected eDrawerState currentDrawerState = eDrawerState.CLOSED;

        [Header("Drawer Configuration")]
        [SerializeField, Tooltip("If true, the drawer will be unlocked the first time the player activates its drawer pull. Drawer Pull Activate components should be configured to require an inventory item. (e.g. Drawer starts locked, and unlocks when player activates with required inventory object.)")]
        protected bool startInternallyLocked = false;
        [SerializeField, Tooltip("The type of inventory item required to unlock the drawer")]
        protected FPEInventoryManagerScript.eInventoryItems requiredKeyType = FPEInventoryManagerScript.eInventoryItems.SIMPLEKEY;
        [SerializeField, Tooltip("If true, the drawer will never open until an external object unlocks it. (e.g. A hidden security button under a desk is pressed and releases the lock).")]
        protected bool startExternallyLocked = false;
        [SerializeField, Tooltip("If true, drawer's default state will be 'OPEN'. Used for drawers that are open a little when the game starts.")]
        private bool startOpened = false;
        
        [SerializeField, Tooltip("If true, will stop sliding if its FPEDoorAndDrawerHitHelpers hit something.")]
        private bool stopIfDrawerHitsSomething = false;

        private bool internallyLocked = false;
        private bool externallyLocked = false;

        private Transform slidingPart = null;
        // Note: There can be more than one drawer pull, but they should all do the same thing, and will all be given the same 
        // interaction string. If you need special pulls that do different things , consider writing a custom drawer class.
        private List<Transform> drawerPulls = null;
        private FPEDoorAndDrawerHitHelper openHitHelper = null;
        private FPEDoorAndDrawerHitHelper closeHitHelper = null;
        private Vector3 closedPosition = Vector3.zero;
        private Vector3 openedPosition = Vector3.zero;
        private float drawerSlideDistancePerSecond = 0f;

        [Header("Drawer Pull Interaction Text Overrides")]
        [SerializeField, Tooltip("String assigned to drawer pull when drawer can be opened")]
        private string openDrawerString = "Open";
        [SerializeField, Tooltip("String assigned to drawer pull when drawer can be closed")]
        private string closeDrawerString = "Close";
        [SerializeField, Tooltip("String assigned to drawer pull when drawer cannot open and player tries to open it. (E.g. 'It's locked.', 'I think it's jammed.', etc.)")]
        private string openDrawerFailureString = "It's locked.";
        [SerializeField, Tooltip("String assigned to drawer pull when drawer was internally locked and player tried to open it with the required key or item. (E.g. 'It's unlocked now.', etc.)")]
        protected string internalUnlockString = "It's unlocked now.";
        [SerializeField, Tooltip("String assigned to drawer pull when drawer is externally locked and player tries to open it. (E.g. 'I have to find the secret button to open this.', etc.)")]
        protected string externalLockString = "I have to find the secret button to open this.";

        [Header("Sounds")]
        [SerializeField, Tooltip("If true, door will play sounds for various states and actions")]
        private bool playSounds = true;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Drawer Opening sounds here. If this field is left unassigned, these sounds will not be played")]
        private FPESimpleSoundBank drawerOpenSounds = null;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Drawer Closing sounds here. If this field is left unassigned, these sounds will not be played")]
        private FPESimpleSoundBank drawerCloseSounds = null;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Drawer Blocked sounds here. If this field is left unassigned, these sounds will not be played")]
        private FPESimpleSoundBank drawerBlockedSounds = null;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Drawer Open Failure sounds here. If this field is left unassigned, these sounds will not be played")]
        private FPESimpleSoundBank drawerOpenFailureSounds = null;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Drawer Unlock sounds here. If this field is left unassigned, these sounds will not be played")]
        protected FPESimpleSoundBank drawerUnlockSounds = null;

        [Header("Drawer-Specific Behaviour")]
        [SerializeField, Tooltip("The distance (in Unity units) and direction (positive or negative) the drawer will slide open along its local z-axis")]
        private float slideDistance = 0.45f;
        [SerializeField, Tooltip("The time (in seconds) it takes for the drawer to fully open or close.")]
        private float slideOpenTimeInSeconds = 0.5f;
        [SerializeField, Tooltip("The distance (in units) at which the drawer snaps open and closed. Default is 0.01.")]
        private float drawerSlideSnapDistance = 0.01f;

        [SerializeField, Tooltip("If true, Player will be automatically moved into safe zone so the drawer will be less likely to hit them.")]
        private bool autoMovePlayerToSafeZone = true;
        private Transform safeZone = null;
        // How quickly the player is auto-moved to the nearest safe zone (default )
        private float playerAutoMoveUnitsPerSecond = 5.0f;
        private bool needToMovePlayerToSafeZone = false;
        private Vector3 currentSafeZoneTargetPosition = Vector3.zero;

        private AudioSource drawerSpeaker = null;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField]
        private bool drawDebugGizmos = true;
#endif

        private void Awake()
        {

            Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>();
            drawerPulls = new List<Transform>();

            foreach (Transform t in childTransforms)
            {

                if (t.name == "SlidingPart")
                {
                    slidingPart = t;
                }
                else if (t.name.Contains("DrawerPull") && t.gameObject.GetComponent<FPEInteractableActivateScript>())
                {
                    drawerPulls.Add(t);
                }
                else if (t.name.Contains("OpenHitHelper") && t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>())
                {
                    openHitHelper = t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>();
                }
                else if (t.name.Contains("CloseHitHelper") && t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>())
                {
                    closeHitHelper = t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>();
                }
                else if (t.name == "SafeZone")
                {
                    safeZone = t;
                }

            }

            if (!slidingPart || drawerPulls.Count == 0)
            {

                Debug.Log("FPEDrawer:: Drawer '" + gameObject.name + "' was missing key component(s). Attempting to fix.", gameObject);

                // Attempt some automatic fixes
#if UNITY_EDITOR

                if (slidingPart == null)
                {
                    GameObject slidingPartFix = new GameObject("SlidingPart");
                    slidingPartFix.transform.parent = gameObject.transform;
                    slidingPartFix.transform.localPosition = Vector3.zero;
                    slidingPart = slidingPartFix.transform;
                }

                if(drawerPulls.Count == 0)
                {
                    GameObject drawerPullFix = new GameObject("DrawerPull");
                    drawerPullFix.AddComponent<BoxCollider>();
                    drawerPullFix.AddComponent<FPEInteractableActivateScript>();
                    drawerPullFix.transform.parent = slidingPart;
                    drawerPullFix.transform.localPosition = Vector3.zero;
                    drawerPulls.Add(drawerPullFix.transform);
                    Debug.LogWarning("FPEDrawer:: Drawer '" + gameObject.name + "' DrawerPull has no Activation Event. Drawer Pull interaction will do nothing until assigned. Also remember to make the Event Fire Type be 'EVERYTIME'.", gameObject);
                }

#endif

            }

            drawerSlideDistancePerSecond = Mathf.Abs(slideDistance / slideOpenTimeInSeconds);
            closedPosition = slidingPart.localPosition;
            closedPosition.z = 0f;
            openedPosition = closedPosition + new Vector3(0f, 0f, slideDistance);

            if (startOpened)
            {
                currentDrawerState = eDrawerState.OPEN;
                setDrawerPullInteractionStrings(closeDrawerString);
            }
            else
            {
                setDrawerPullInteractionStrings(openDrawerString);
            }

            if (startExternallyLocked)
            {
                externallyLocked = true;
            }

            if (startInternallyLocked)
            {
                internallyLocked = true;
            }

            drawerSpeaker = gameObject.GetComponent<AudioSource>();

            // Error Checking //
            if (stopIfDrawerHitsSomething && (openHitHelper == null || closeHitHelper == null))
            {
                Debug.LogError("FPEDrawer:: Drawer '" + gameObject.name + "' is configured to stop if drawer hits something, but it is missing child 'OpenHitHelper' or 'CloseHitHelper' FPEDoorAndDrawerHitHelper objects. Drawer will not stop when it hits something.", gameObject);
            }

            // Safe zone is optional, but is required if you've set the flag to auto move the player
            if (autoMovePlayerToSafeZone && !safeZone)
            {

                Debug.Log("FPEDrawer:: Drawer '" + gameObject.name + "' is configured to automatically move the player into safe zone, but has no safe zone defined. Attempting to fix");

                // Attempt some automatic fixes (assumes slide distance is 0.45, which is the default, which is likely 
                // the value since these fixes really only happen if you just add this script to an empty game object)
#if UNITY_EDITOR

                GameObject safeZoneFix = new GameObject("SafeZone");
                safeZoneFix.transform.parent = gameObject.transform;
                safeZoneFix.transform.localPosition = new Vector3(0.0f,0.0f,1.3f);
                safeZone = safeZoneFix.transform;

#endif

            }

            if (playSounds && drawerSpeaker == null)
            {

                Debug.Log("FPEDrawer:: Drawer '" + gameObject.name + "' was configured to play sounds but had no AudioSource. Adding AudioSource.", gameObject);

                // Attempt some automatic fixes
#if UNITY_EDITOR
                gameObject.AddComponent<AudioSource>();
                drawerSpeaker = gameObject.GetComponent<AudioSource>();
#endif

            }
            
        }


        void Start()
        {

        }


        void Update()
        {

            if (currentDrawerState == eDrawerState.CLOSED)
            {
                // Do nothing
            }
            else if (currentDrawerState == eDrawerState.CLOSING)
            {

                if ((stopIfDrawerHitsSomething == true && drawerCloseHitHelperHitSomething() == false) || stopIfDrawerHitsSomething == false)
                {

                    slidingPart.localPosition = Vector3.MoveTowards(slidingPart.localPosition, closedPosition, drawerSlideDistancePerSecond * Time.deltaTime);

                    if (Mathf.Abs(Vector3.Distance(slidingPart.localPosition, closedPosition)) <= drawerSlideSnapDistance)
                    {

                        slidingPart.localPosition = closedPosition;
                        currentDrawerState = eDrawerState.CLOSED;
                        setDrawerPullInteractionStrings(openDrawerString);

                    }

                }
                else
                {

                    if (playSounds && drawerBlockedSounds)
                    {
                        drawerBlockedSounds.Play(drawerSpeaker);
                    }

                    // Yes. We want drawers that hit something on closing to have their next interaction open them away from the thing 
                    // they hit. This will give objects stuck in drawer a chance to fall back into the drawer
                    currentDrawerState = eDrawerState.BLOCKED_PARTLY_OPEN;
                    setDrawerPullInteractionStrings(openDrawerString);

                }

            }
            else if (currentDrawerState == eDrawerState.OPENING)
            {

                if ((stopIfDrawerHitsSomething == true && drawerOpenHitHelperHitSomething() == false) || stopIfDrawerHitsSomething == false)
                {

                    movePlayerCloserToSafeZone();
                    slidingPart.localPosition = Vector3.MoveTowards(slidingPart.localPosition, openedPosition, drawerSlideDistancePerSecond * Time.deltaTime);

                    if (Mathf.Abs(Vector3.Distance(slidingPart.localPosition, openedPosition)) <= drawerSlideSnapDistance)
                    {

                        slidingPart.localPosition = openedPosition;
                        currentDrawerState = eDrawerState.OPEN;
                        setDrawerPullInteractionStrings(closeDrawerString);
                        releasePlayerFromSafeZone();

                    }

                }
                else
                {

                    if (playSounds && drawerBlockedSounds)
                    {
                        drawerBlockedSounds.Play(drawerSpeaker);
                    }

                    // Yes. We want drawers that hit something on opening to have their next interaction close them away from the thing they hit
                    currentDrawerState = eDrawerState.BLOCKED_PARTLY_CLOSED;
                    setDrawerPullInteractionStrings(closeDrawerString);
                    releasePlayerFromSafeZone();

                }

            }
            else if (currentDrawerState == eDrawerState.OPEN)
            {
                // Do nothing
            }
            else if (currentDrawerState == eDrawerState.BLOCKED_PARTLY_OPEN)
            {
                // Do nothing
            }
            else if (currentDrawerState == eDrawerState.BLOCKED_PARTLY_CLOSED)
            {
                // Do nothing
            }

        }

        public void activateDrawer()
        {

            if (externallyLocked)
            {
                handleExternalLockActivation();
            }
            else if (internallyLocked)
            {

                GameObject objectInPlayersHand = FPEInteractionManagerScript.Instance.getHeldObject();

                // If player is holding something that is inventory and matches required key type
                if ((FPEInventoryManagerScript.Instance.inventoryQuantity(requiredKeyType) > 0) || (objectInPlayersHand != null && objectInPlayersHand.GetComponent<FPEInteractableInventoryItemScript>() && objectInPlayersHand.GetComponent<FPEInteractableInventoryItemScript>().InventoryItemType == requiredKeyType))
                {

                    if (playSounds && drawerUnlockSounds)
                    {
                        drawerUnlockSounds.Play(drawerSpeaker);
                    }

                    internallyLocked = false;
                    setDrawerPullInteractionStrings(internalUnlockString);

                }
                else
                {

                    if (playSounds && drawerOpenFailureSounds)
                    {
                        drawerOpenFailureSounds.Play(drawerSpeaker);
                    }

                    setDrawerPullInteractionStrings(openDrawerFailureString);

                }

            }
            else
            {

                if (currentDrawerState == eDrawerState.CLOSED || currentDrawerState == eDrawerState.CLOSING)
                {
                    openTheDrawer();
                }
                else if (currentDrawerState == eDrawerState.OPEN || currentDrawerState == eDrawerState.OPENING)
                {
                    closeTheDrawer();
                }
                else if (currentDrawerState == eDrawerState.BLOCKED_PARTLY_OPEN)
                {
                    openTheDrawer();
                }
                else if (currentDrawerState == eDrawerState.BLOCKED_PARTLY_CLOSED)
                {
                    closeTheDrawer();
                }

            }

        }

        private void closeTheDrawer()
        {

            needToMovePlayerToSafeZone = false;

            if (playSounds && drawerCloseSounds)
            {
                drawerCloseSounds.Play(drawerSpeaker);
            }

            resetHitHelpers();
            currentDrawerState = eDrawerState.CLOSING;

        }

        private void openTheDrawer()
        {

            needToMovePlayerToSafeZone = true;
            putPlayerInSafeZone(safeZone);

            if (playSounds && drawerOpenSounds)
            {
                drawerOpenSounds.Play(drawerSpeaker);
            }

            resetHitHelpers();
            currentDrawerState = eDrawerState.OPENING;

        }
     
        private void handleExternalLockActivation()
        {

            if (playSounds && drawerBlockedSounds)
            {
                drawerBlockedSounds.Play(drawerSpeaker);
            }

            setDrawerPullInteractionStrings(externalLockString);

        }

        private void putPlayerInSafeZone(Transform zone)
        {

            if (autoMovePlayerToSafeZone)
            {

                FPEInteractionManagerScript.Instance.BeginCutscene();
                currentSafeZoneTargetPosition = zone.position;
                currentSafeZoneTargetPosition.y = FPEPlayer.Instance.transform.position.y;
                needToMovePlayerToSafeZone = true;

            }

        }


        private void releasePlayerFromSafeZone()
        {

            if (autoMovePlayerToSafeZone)
            {
                needToMovePlayerToSafeZone = false;
                FPEInteractionManagerScript.Instance.EndCutscene();
            }

        }

        private void movePlayerCloserToSafeZone()
        {

            if (autoMovePlayerToSafeZone && needToMovePlayerToSafeZone)
            {

                FPEPlayer.Instance.gameObject.transform.position = Vector3.Lerp(FPEPlayer.Instance.gameObject.transform.position, currentSafeZoneTargetPosition, playerAutoMoveUnitsPerSecond * Time.deltaTime);

            }

        }


        /// <summary>
        /// Checks if the OpenHitHelper has hit anything
        /// </summary>
        /// <returns>True if OpenHitHelper has hit something.</returns>
        private bool drawerOpenHitHelperHitSomething()
        {

            bool result = false;
            
            if (openHitHelper != null && openHitHelper.HasHitSomething)
            {
                result = true;
            }

            return result;

        }

        /// <summary>
        /// Checks if the CloseHitHelper has hit anything
        /// </summary>
        /// <returns>True if CloseHitHelper has hit something.</returns>
        private bool drawerCloseHitHelperHitSomething()
        {

            bool result = false;

            if (closeHitHelper != null && closeHitHelper.HasHitSomething)
            {
                result = true;
            }

            return result;

        }

        /// <summary>
        /// To reset all hit helpers so we can try to open or close the drawer again after it became blocked
        /// </summary>
        private void resetHitHelpers()
        {

            if(openHitHelper != null)
            {
                openHitHelper.ResetHits();
            }
            if (closeHitHelper != null)
            {
                closeHitHelper.ResetHits();
            }

        }

        private void setDrawerPullInteractionStrings(string s)
        {

            foreach (Transform pull in drawerPulls)
            {
                pull.gameObject.GetComponent<FPEInteractableActivateScript>().setInteractionString(s);
            }

        }

        /// <summary>
        /// Used to disable external lock on the drawer from another object such (e.g. A secret button)
        /// Note: You may want to create your own version of this function in your own drawer class to 
        /// handle special cases where asking the drawer to be unlocked fails (and maybe return a bool)
        /// </summary>
        public void ExternallyUnlockDrawer()
        {

            if(playSounds && drawerUnlockSounds)
            {
                drawerUnlockSounds.Play(drawerSpeaker);
            }

            externallyLocked = false;

        }

        public FPEDrawerSaveData getSaveGameData()
        {

            Vector3 customPosition = slidingPart.localPosition;
            return new FPEDrawerSaveData(gameObject.name, currentDrawerState, drawerPulls[0].GetComponent<FPEInteractableActivateScript>().interactionString, customPosition, internallyLocked, externallyLocked);

        }

        public void restoreSaveGameData(FPEDrawerSaveData data)
        {

            currentDrawerState = data.DrawerState;
            internallyLocked = data.IsInternallyLocked;
            externallyLocked = data.IsExternallyLocked;
            setDrawerPullInteractionStrings(data.DrawerPullString);

            switch (currentDrawerState)
            {

                case eDrawerState.CLOSING:
                case eDrawerState.CLOSED:
                case eDrawerState.BLOCKED_PARTLY_CLOSED:
                case eDrawerState.OPENING:
                case eDrawerState.OPEN:
                case eDrawerState.BLOCKED_PARTLY_OPEN:
                    slidingPart.localPosition = data.CustomDrawerVector;
                    break;

                default:
                    Debug.LogError("FPEDrawer.restoreSaveGameData():: Given bad drawer state '" + currentDrawerState + "'");
                    break;

            }

        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {

            if (drawDebugGizmos)
            {

                Color c = Color.red;
                if (slidingPart != null)
                {

                    Gizmos.color = c;

                    // Represent the sliding limits of our door in both left and right hand inswing
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Matrix4x4 tempMatrixPos = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    Gizmos.matrix = tempMatrixPos;

                    c = Color.red;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(openedPosition, 0.1f);

                    c = Color.green;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(closedPosition, 0.1f);

                    c = Color.blue;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawLine(openedPosition, closedPosition);

                    Gizmos.matrix = oldMatrix;

                }


                if (drawerPulls != null && drawerPulls.Count != 0)
                {

                    foreach (Transform t in drawerPulls)
                    {

                        c = Color.green;
                        c.a = 0.5f;
                        Gizmos.color = c;
                        Gizmos.DrawSphere(t.position, 0.15f);
                        Gizmos.DrawIcon(t.position, "Whilefun/doorHandle.png", false);

                    }

                }

                if (safeZone)
                {

                    c = Color.cyan;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(safeZone.position, 0.25f);
                    Gizmos.DrawIcon(safeZone.position, "Whilefun/doorSafeZone.png", false);

                }

                Vector3 stateIconPosition = transform.position + transform.up * 0.5f;
                switch (currentDrawerState)
                {

                    case eDrawerState.BLOCKED_PARTLY_OPEN:
                    case eDrawerState.BLOCKED_PARTLY_CLOSED:
                        Gizmos.DrawIcon(stateIconPosition, "Whilefun/doorStateBlocked.png", false);
                        break;
                    case eDrawerState.CLOSED:
                        Gizmos.DrawIcon(stateIconPosition, "Whilefun/doorStateClosed.png", false);
                        break;
                    case eDrawerState.CLOSING:
                        Gizmos.DrawIcon(stateIconPosition, "Whilefun/doorStateClosing.png", false);
                        break;
                    case eDrawerState.OPEN:
                        Gizmos.DrawIcon(stateIconPosition, "Whilefun/doorStateOpen.png", false);
                        break;
                    case eDrawerState.OPENING:
                        Gizmos.DrawIcon(stateIconPosition, "Whilefun/doorStateOpening.png", false);
                        break;

                }

            }

        }

#endif

    }

}