using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Whilefun.FPEKit
{

    //
    // FPESlidingDoor
    // This script is used for simple manually operated sliding doors such as pocket sliders, cabinets, etc.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [ExecuteInEditMode]
    public class FPESlidingDoor : FPEDoor
    {

        [Header("Door-Specific Behaviour")]
        [SerializeField, Tooltip("The distance (in Unity units) and direction (positive or negative) the door will slide open along its local x-axis")]
        private float slideDistance = 2.0f;
        [SerializeField, Tooltip("The time (in seconds) it takes for the door to fully open or close.")]
        private float slideOpenTimeInSeconds = 1.0f;
        [SerializeField, Tooltip("The distance (in units) at which the door snaps open and closed. Default is 0.05.")]
        private float doorSlideSnapDistance = 0.05f;
        [SerializeField, Tooltip("If true, doors will be closed after number of seconds set in Automatic Close Time field.")]
        private bool closeAutomatically = false;
        [SerializeField, Tooltip("Time (in seconds) before doors will close after being remotely opened. Ignored if Close Automatically is set to false")]
        private float automaticCloseTime = 1.0f;

        private float automaticCloseCounter = 0f;
        private Vector3 closedPosition = Vector3.zero;
        private Vector3 openedPosition = Vector3.zero;
        private float doorSlideDistancePerSecond = 0f;
        private Transform slidingPart = null;


#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField]
        private bool drawDebugGizmos = true;
        [SerializeField, Tooltip("The scale of the blue 'ghost door'. Default is 1, but you might want to make it smaller for small doors like desks and cupboards.")]
        private Vector3 ghostDoorSize = new Vector3(2.0f, 3.0f, 0.1f);
#endif

        protected override void Awake()
        {

            base.Awake();

            Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>();
            doorHandles = new List<Transform>();

            foreach (Transform t in childTransforms)
            {

                if (t.name == "SlidingPart")
                {
                    slidingPart = t;
                }
                else if (t.name.Contains("DoorHandle") && t.gameObject.GetComponent<FPEInteractableActivateScript>())
                {
                    doorHandles.Add(t);
                }

            }

            if (!slidingPart || doorHandles.Count == 0)
            {

                Debug.Log("FPESlidingDoor:: Door '" + gameObject.name + "' is missing a required part. Attempting to fix.", gameObject);

                // Attempt some automatic fixes
#if UNITY_EDITOR

                if (slidingPart == null)
                {
                    GameObject slidingPartFix = new GameObject("SlidingPart");
                    slidingPartFix.transform.parent = gameObject.transform;
                    slidingPartFix.transform.localPosition = Vector3.zero;
                    slidingPart = slidingPartFix.transform;
                }

                if (doorHandles.Count == 0)
                {
                    GameObject doorHandleFix = new GameObject("DoorHandle");
                    doorHandleFix.AddComponent<BoxCollider>();
                    doorHandleFix.AddComponent<FPEInteractableActivateScript>();
                    doorHandleFix.transform.parent = slidingPart;
                    doorHandleFix.transform.localPosition = Vector3.zero;
                    doorHandles.Add(doorHandleFix.transform);
                    Debug.LogWarning("FPEDoor:: Door '" + gameObject.name + "' DoorHandle child has no Activation Event assigned. Door Handle interaction will do nothing until it is assigned. Also remember to make the Event Fire Type be 'EVERYTIME'.", gameObject);
                }

#endif

            }

            doorSlideDistancePerSecond = Mathf.Abs(slideDistance / slideOpenTimeInSeconds);
            closedPosition = Vector3.zero;
            closedPosition.y = slidingPart.localPosition.y;
            openedPosition = closedPosition + new Vector3(slideDistance, 0f, 0f);
            myHitHelpers = gameObject.GetComponentsInChildren<FPEDoorAndDrawerHitHelper>();

            if (startOpened)
            {
                currentDoorState = eDoorState.OPEN;
                setDoorHandleInteractionStrings(closeDoorString);
            }
            else
            {
                setDoorHandleInteractionStrings(openDoorString);
            }

            doorSpeaker = gameObject.GetComponent<AudioSource>();

            // Error Checking //
            if (stopIfDoorHitsSomething && myHitHelpers.Length == 0)
            {
                Debug.LogWarning("FPESlidingDoor:: Door '" + gameObject.name + "' is configured to stop if door hits something, but it has no child FPEDoorAndDrawerHitHelper objects. Door will not stop when it hits something.", gameObject);
            }

            if (playSounds && doorSpeaker == null)
            {

                Debug.Log("FPESlidingDoor:: Door '" + gameObject.name + "' is configured to play sounds but has no AudioSource. Attempting Fix.", gameObject);

                // Attempt some automatic fixes
#if UNITY_EDITOR
                gameObject.AddComponent<AudioSource>();
                doorSpeaker = gameObject.GetComponent<AudioSource>();
#endif

            }

            if (closeAutomatically && automaticCloseTime <= 0f)
            {
                Debug.LogError("FPESlidingDoor:: Door '" + gameObject.name + "' is configured to close automatically, but Automatic Close Time is set to a value less than or equal to 0.", gameObject);
            }

            if (startOpened && slidingPart.transform.localPosition.x == 0f)
            {
                Debug.LogWarning("FPESlidingDoor:: Door '" + gameObject.name + "' is configured to start OPEN, but SlidingPart transform is not in an open position.", gameObject);
            }

        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {

            base.Update();

            if (currentDoorState == eDoorState.CLOSED)
            {
                // Do nothing
            }
            else if (currentDoorState == eDoorState.CLOSING)
            {

                if ((stopIfDoorHitsSomething == true && doorHitSomething() == false) || stopIfDoorHitsSomething == false)
                {

                    slidingPart.localPosition = Vector3.MoveTowards(slidingPart.localPosition, closedPosition, doorSlideDistancePerSecond * Time.deltaTime);

                    if (Mathf.Abs(Vector3.Distance(slidingPart.localPosition, closedPosition)) <= doorSlideSnapDistance)
                    {

                        slidingPart.localPosition = closedPosition;

                        if (playSounds && doorLatchSounds)
                        {
                            doorLatchSounds.Play(doorSpeaker);
                        }

                        setDoorHandleInteractionStrings(openDoorString);
                        currentDoorState = eDoorState.CLOSED;

                    }

                }
                else
                {

                    if (playSounds && doorBlockedSounds)
                    {
                        doorBlockedSounds.Play(doorSpeaker);
                    }


                    currentDoorState = eDoorState.BLOCKED_PARTLY_CLOSED;
                    setDoorHandleInteractionStrings(openDoorString);
                    

                }

            }
            else if (currentDoorState == eDoorState.OPENING)
            {


                slidingPart.localPosition = Vector3.MoveTowards(slidingPart.localPosition, openedPosition, doorSlideDistancePerSecond * Time.deltaTime);

                if (Mathf.Abs(Vector3.Distance(slidingPart.localPosition, openedPosition)) <= doorSlideSnapDistance)
                {

                    slidingPart.localPosition = openedPosition;

                    if (playSounds && doorLatchSounds && !closeAutomatically)
                    {
                        doorLatchSounds.Play(doorSpeaker);
                    }

                    setDoorHandleInteractionStrings(closeDoorString);
                    currentDoorState = eDoorState.OPEN;

                }

            }
            else if (currentDoorState == eDoorState.OPEN)
            {

                if (closeAutomatically)
                {

                    automaticCloseCounter -= Time.deltaTime;

                    if(automaticCloseCounter <= 0f)
                    {
                        closeTheDoor();
                    }

                }

            }
            else if (currentDoorState == eDoorState.BLOCKED_PARTLY_OPEN)
            {
                // Do nothing
            }
            else if (currentDoorState == eDoorState.BLOCKED_PARTLY_CLOSED)
            {
                // Do nothing
            }

        }

        public override void activateDoor()
        {

            if (externallyLocked)
            {
                handleExternalLockActivation();
            }
            else if (internallyLocked)
            {

                GameObject objectInPlayersHand = FPEInteractionManagerScript.Instance.getHeldObject();

                if ((FPEInventoryManagerScript.Instance.inventoryQuantity(requiredKeyType) > 0) || (objectInPlayersHand != null && objectInPlayersHand.GetComponent<FPEInteractableInventoryItemScript>() && objectInPlayersHand.GetComponent<FPEInteractableInventoryItemScript>().InventoryItemType == requiredKeyType))
                {

                    if (playSounds && doorUnlockSounds)
                    {
                        doorUnlockSounds.Play(doorSpeaker);
                    }

                    internallyLocked = false;
                    setDoorHandleInteractionStrings(internalUnlockString);

                    if (consumeKeyOnInternalUnlock)
                    {

                        // Case 1: Player is holding the key in their hand
                        if (objectInPlayersHand != null && objectInPlayersHand.GetComponent<FPEInteractableInventoryItemScript>() && objectInPlayersHand.GetComponent<FPEInteractableInventoryItemScript>().InventoryItemType == requiredKeyType)
                        {
                            FPEInteractionManagerScript.Instance.DestroyHeldObject();
                        }
                        // Case 2: Player has the key in their inventory
                        // Note: To destroy all of this type of key, use FPEInventoryManagerScript.Instance.inventoryQuantity(requiredKeyType) instead of 1
                        else
                        {
                            FPEInventoryManagerScript.Instance.destroyInventoryItemsOfType(requiredKeyType, 1);
                        }

                    }

                }
                else
                {

                    if (playSounds && doorOpenFailureSounds)
                    {
                        doorOpenFailureSounds.Play(doorSpeaker);
                    }

                    setDoorHandleInteractionStrings(openDoorFailureString);

                }

            }
            else
            {

                if (currentDoorState == eDoorState.CLOSED || currentDoorState == eDoorState.CLOSING)
                {
                    openTheDoor();
                }
                else if (currentDoorState == eDoorState.OPEN || currentDoorState == eDoorState.OPENING)
                {
                    closeTheDoor();
                }
                else if (currentDoorState == eDoorState.BLOCKED_PARTLY_OPEN)
                {
                    closeTheDoor();
                }
                else if (currentDoorState == eDoorState.BLOCKED_PARTLY_CLOSED)
                {
                    openTheDoor();
                }

            }

        }

        protected override void closeTheDoor()
        {

            if (playSounds && doorCloseSounds)
            {
                doorCloseSounds.Play(doorSpeaker);
            }

            resetHitHelpers();
            currentDoorState = eDoorState.CLOSING;

        }

        protected override void openTheDoor(bool makeSound = true)
        {

            if (makeSound && playSounds && doorOpenSounds)
            {
                doorOpenSounds.Play(doorSpeaker);
            }

            resetHitHelpers();
            currentDoorState = eDoorState.OPENING;

        }

        /// <summary>
        /// To be called by automatic door sensor (trigger volumes, switches, etc.) to open the door or keep it from automatically closing. When called the first time, sounds are played. When called subsequent times, the doors auto close counter is sustained.
        /// </summary>
        public void RemotelyOpenDoor(bool makeSound = true)
        {

            if (currentDoorState == eDoorState.CLOSED || currentDoorState == eDoorState.CLOSING || currentDoorState == eDoorState.BLOCKED_PARTLY_CLOSED || currentDoorState == eDoorState.BLOCKED_PARTLY_OPEN)
            {
                openTheDoor(makeSound);
            }

            if (closeAutomatically)
            {
                automaticCloseCounter = automaticCloseTime;
            }

        }

        /// <summary>
        /// To be called by automatic door sensor (trigger volumes, switches, etc.)
        /// </summary>
        public void RemotelyCloseDoor()
        {
            closeTheDoor();
        }

        protected override void handleExternalLockActivation()
        {

            if (playSounds && doorBlockedSounds)
            {
                doorBlockedSounds.Play(doorSpeaker);
            }

            setDoorHandleInteractionStrings(externalLockString);

        }

        public override FPEDoorSaveData getSaveGameData()
        {

            Vector3 customPosition = slidingPart.localPosition;
            return new FPEDoorSaveData(gameObject.name, currentDoorState, doorHandles[0].GetComponent<FPEInteractableActivateScript>().interactionString, customPosition, internallyLocked, externallyLocked);

        }

        public override void restoreSaveGameData(FPEDoorSaveData data)
        {

            currentDoorState = data.DoorState;
            internallyLocked = data.IsInternallyLocked;
            externallyLocked = data.IsExternallyLocked;
            setDoorHandleInteractionStrings(data.DoorHandleString);

            switch (currentDoorState)
            {

                case eDoorState.CLOSED:
                    slidingPart.localPosition = closedPosition;
                    break;

                case eDoorState.CLOSING:
                case eDoorState.BLOCKED_PARTLY_CLOSED:
                    slidingPart.localPosition = data.CustomDoorVector;
                    break;

                case eDoorState.OPEN:
                    slidingPart.localPosition = openedPosition;
                    break;

                case eDoorState.OPENING:
                case eDoorState.BLOCKED_PARTLY_OPEN:
                    slidingPart.localPosition = data.CustomDoorVector;
                    break;

                default:
                    Debug.LogError("FPESlidingDoor.restoreSaveGameData():: Given bad door state '" + currentDoorState + "'");
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
                    Gizmos.DrawLine(slidingPart.transform.position - slidingPart.transform.up, slidingPart.transform.position + slidingPart.transform.up);
                    Gizmos.DrawWireSphere(slidingPart.transform.position, 0.1f);
                    Gizmos.DrawIcon(slidingPart.transform.position, "Whilefun/doorHinge.png", false);

                    // Represent the sliding limits of our door in both left and right hand inswing
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Matrix4x4 tempMatrixPos = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    Gizmos.matrix = tempMatrixPos;

                    c = Color.red;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(openedPosition, 0.25f);

                    c = Color.green;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(closedPosition, 0.25f);

                    c = Color.blue;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawLine(openedPosition, closedPosition);
                    Gizmos.DrawCube(openedPosition, ghostDoorSize);

                    Gizmos.matrix = oldMatrix;

                }

                if (doorHandles != null && doorHandles.Count != 0)
                {

                    foreach (Transform t in doorHandles)
                    {

                        c = Color.green;
                        c.a = 0.5f;
                        Gizmos.color = c;
                        Gizmos.DrawSphere(t.position, 0.15f);
                        Gizmos.DrawIcon(t.position, "Whilefun/doorHandle.png", false);

                    }

                }

                Vector3 stateIconPosition = transform.position + transform.up * 3f;
                switch (currentDoorState)
                {

                    case eDoorState.BLOCKED_PARTLY_OPEN:
                    case eDoorState.BLOCKED_PARTLY_CLOSED:
                        Gizmos.DrawIcon(stateIconPosition, "Whilefun/doorStateBlocked.png", false);
                        break;
                    case eDoorState.CLOSED:
                        Gizmos.DrawIcon(stateIconPosition, "Whilefun/doorStateClosed.png", false);
                        break;
                    case eDoorState.CLOSING:
                        Gizmos.DrawIcon(stateIconPosition, "Whilefun/doorStateClosing.png", false);
                        break;
                    case eDoorState.OPEN:
                        Gizmos.DrawIcon(stateIconPosition, "Whilefun/doorStateOpen.png", false);
                        break;
                    case eDoorState.OPENING:
                        Gizmos.DrawIcon(stateIconPosition, "Whilefun/doorStateOpening.png", false);
                        break;

                }

                Vector3 lockIconPosition = transform.position + transform.up * 3.5f;
                if (externallyLocked || (startExternallyLocked && externallyLocked))
                {
                    Gizmos.DrawIcon(lockIconPosition, "Whilefun/doorExternallyLocked.png", false);
                }
                else
                {
                    Gizmos.DrawIcon(lockIconPosition, "Whilefun/doorExternallyUnlocked.png", false);
                }

            }

        }

#endif

    }

}