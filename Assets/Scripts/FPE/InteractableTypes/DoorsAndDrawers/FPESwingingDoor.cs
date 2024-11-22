using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPESwingingDoor
    // This script is used for more complex and realistic "one way swing out" doors
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [ExecuteInEditMode]
    public class FPESwingingDoor : FPEDoor
    {

        [Header("Door-Specific Behaviour")]
        [SerializeField, Tooltip("The angle from closed the door will swing open. 135 is a good default for most realistic doors.")]
        private float swingAngle = 135.0f;
        [SerializeField, Tooltip("The time (in seconds) it takes for the door to fully open or close.")]
        private float swingOpenTimeInSeconds = 1.0f;
        [SerializeField, Tooltip("The angle (in degrees) at which the door snaps closed. Default is 1.")]
        private float doorSwingSnapAngle = 1.0f;
        [SerializeField, Tooltip("If true, Player will be automatically moved into safe zones and pan their view so the door will be less likely to hit them.")]
        private bool autoMovePlayerToSafeZones = true;

        private Quaternion closedRotation = Quaternion.identity;
        private Quaternion openRotation = Quaternion.identity;
        private Transform swingingPart = null;
        private float maxSwingDegreesPerSecond = 0f;
        // How quickly the player is auto-moved to the nearest safe zone (default 25)
        private float playerAutoMoveUnitsPerSecond = 25.0f;
        // How quickly the player view pans to the door's look target while moving to safe zone (default 6)
        private float playerAutoMoveLookPanSpeed = 6.0f;
        private bool needToMovePlayerToSafeZone = false;
        private Transform swingOutSide = null;
        private Transform swingInSide = null;
        private Transform safeZoneSwingIn = null;
        private Transform safeZoneSwingOut = null;
        private Transform doorActionLookTarget = null;
        private Vector3 currentSafeZoneTargetPosition = Vector3.zero;
        private Vector3 currentSafeLookTargetPosition = Vector3.zero;


#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField]
        private bool drawDebugGizmos = true;
        [SerializeField, Tooltip("The scale of the blue 'ghost door'. Default is 1, but you might want to make it smaller for small doors like desks and cupboards.")]
        private float doorGhostScale = 1.0f;
#endif

        protected override void Awake()
        {

            base.Awake();

            Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>();
            doorHandles = new List<Transform>();
            myHitHelpers = new FPEDoorAndDrawerHitHelper[2];

            foreach (Transform t in childTransforms)
            {

                if(t.name == "SwingingPart")
                {
                    swingingPart = t;
                }
                else if (t.name.Contains("DoorHandle") && t.gameObject.GetComponent<FPEInteractableActivateScript>())
                {
                    doorHandles.Add(t);
                }
                else if(t.name == "SwingInSide")
                {
                    swingInSide = t;
                }
                else if (t.name == "SwingOutSide")
                {
                    swingOutSide = t;
                }
                else if (t.name == "SafeZoneSwingIn")
                {
                    safeZoneSwingIn = t;
                }
                else if (t.name == "SafeZoneSwingOut")
                {
                    safeZoneSwingOut = t;
                }
                else if(t.name == "DoorActionLookTarget")
                {
                    doorActionLookTarget = t;
                }
                else if(t.name == "HitHelperOpen" && t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>())
                {
                    myHitHelpers[(int)eDoorActionType.OPEN] = t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>();
                }
                else if (t.name == "HitHelperClose" && t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>())
                {
                    myHitHelpers[(int)eDoorActionType.CLOSE] = t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>();
                }

            }

            maxSwingDegreesPerSecond = Mathf.Abs(swingAngle / swingOpenTimeInSeconds);
            closedRotation = Quaternion.identity;
            openRotation = closedRotation * Quaternion.Euler(new Vector3(0f, swingAngle, 0f));

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

            // Doors must have swinging parts and at least one door handle
            if (!swingingPart || doorHandles.Count == 0)
            {

                Debug.Log("FPESwingingDoor:: Door '" + gameObject.name + "' is missing a required part. Attempting to fix.", gameObject);

                // Attempt some automatic fixes
#if UNITY_EDITOR

                if (swingingPart == null)
                {

                    GameObject swingingPartFix = new GameObject("SwingingPart");
                    swingingPartFix.transform.parent = gameObject.transform;
                    swingingPartFix.transform.localPosition = new Vector3(-1.0f, 1.5f, -0.251f);
                    swingingPart = swingingPartFix.transform;

                }

                if (doorHandles.Count == 0)
                {

                    GameObject doorHandleFix = new GameObject("DoorHandle");
                    doorHandleFix.AddComponent<BoxCollider>();
                    doorHandleFix.AddComponent<FPEInteractableActivateScript>();
                    doorHandleFix.transform.parent = swingingPart;
                    doorHandleFix.transform.localPosition = Vector3.zero;
                    doorHandles.Add(doorHandleFix.transform);
                    Debug.LogWarning("FPEDoor:: Door '" + gameObject.name + "' DoorHandle child has no Activation Event assigned. Door Handle interaction will do nothing until it is assigned. Also remember to make the Event Fire Type be 'EVERYTIME'.", gameObject);

                }

#endif

            }

            // Some angles are not valid and will result in weird first time swings
            if (swingAngle == 0f)
            {
                Debug.LogWarning("FPESwingingDoor:: Door '" + gameObject.name + "' has a swingAngle of zero. This door will not work.", gameObject);
            }
            else
            {

                float deltaA = Quaternion.Angle(swingingPart.localRotation, openRotation);
                float deltaB = Quaternion.Angle(swingingPart.localRotation, closedRotation);
                
                if(Mathf.Abs(deltaA) > Mathf.Abs(swingAngle) || Mathf.Abs(deltaB) > Mathf.Abs(swingAngle))
                {
                    Debug.LogWarning("FPESwingingDoor:: Door '" + gameObject.name + "''s 'SwingingPart' starting rotation is outside defined swingAngle's range. First open/close actions will not behave as expected.", gameObject);
                }

            }

            if (stopIfDoorHitsSomething && (myHitHelpers[(int)eDoorActionType.OPEN] == null || myHitHelpers[(int)eDoorActionType.CLOSE] == null))
            {
                Debug.LogWarning("FPESwingingDoor:: Door '" + gameObject.name + "' is configured to stop if door hits something, but it is missing child FPEDoorAndDrawerHitHelper objects 'HitHelperOpen' and/or 'HitHelperClose'. Door will not stop when it hits something.", gameObject);
            }

            // Safe zones are optional, but are required if you've set the flag to auto move the player
            if (autoMovePlayerToSafeZones && (!swingInSide || !swingOutSide || !safeZoneSwingIn || !safeZoneSwingOut || !doorActionLookTarget))
            {

                Debug.Log("FPESwingingDoor:: Door '" + gameObject.name + "' is configured to automatically move the player into safe zones, but has no safe zones defined. Attempting to fix");

                // Attempt some automatic fixes (these assume swingAngle is 135, which is the default, which is likely 
                // the value since these fixes really only happen if you just add this script to an empty game object)
#if UNITY_EDITOR

                GameObject interactionZoneFix = new GameObject("PlayerInteractionZones");
                interactionZoneFix.transform.parent = gameObject.transform;
                interactionZoneFix.transform.localPosition = Vector3.zero;

                if (swingInSide == null)
                {
                    GameObject swingInsideFix = new GameObject("SwingInSide");
                    swingInsideFix.transform.parent = interactionZoneFix.transform;
                    swingInsideFix.transform.localPosition = new Vector3(0.345f, 0.474f, 0.054f);
                    swingInSide = swingInsideFix.transform;
                }

                if (swingOutSide == null)
                {
                    GameObject swingOutSideFix = new GameObject("SwingOutSide");
                    swingOutSideFix.transform.parent = interactionZoneFix.transform;
                    swingOutSideFix.transform.localPosition = new Vector3(0.345f, 0.474f, -1.219f);
                    swingOutSide = swingOutSideFix.transform;
                }

                if (safeZoneSwingIn == null)
                {
                    GameObject safeZoneSwingInFix = new GameObject("SafeZoneSwingIn");
                    safeZoneSwingInFix.transform.parent = interactionZoneFix.transform;
                    safeZoneSwingInFix.transform.localPosition = new Vector3(-0.07f, 0.98f, 0.92f);
                    safeZoneSwingIn = safeZoneSwingInFix.transform;
                }

                if (safeZoneSwingOut == null)
                {
                    GameObject safeZoneSwingOutFix = new GameObject("SafeZoneSwingOut");
                    safeZoneSwingOutFix.transform.parent = interactionZoneFix.transform;
                    safeZoneSwingOutFix.transform.localPosition = new Vector3(1.0f, 0.98f, -1.66f);
                    safeZoneSwingOut = safeZoneSwingOutFix.transform;
                }

                if (doorActionLookTarget == null)
                {
                    GameObject lookTargetFix = new GameObject("DoorActionLookTarget");
                    lookTargetFix.transform.parent = interactionZoneFix.transform;
                    lookTargetFix.transform.localPosition = new Vector3(0.741f, 1.568f, -0.161f);
                    doorActionLookTarget = lookTargetFix.transform;
                }

#endif

            }

            if (playSounds && doorSpeaker == null)
            {

                Debug.Log("FPESwingingDoor:: Door '" + gameObject.name + "' is configured to play sounds but has no AudioSource. Attempting to fix.", gameObject);

                // Attempt some automatic fixes
#if UNITY_EDITOR
                gameObject.AddComponent<AudioSource>();
                doorSpeaker = gameObject.GetComponent<AudioSource>();
#endif

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

                    swingingPart.localRotation = Quaternion.RotateTowards(swingingPart.localRotation, closedRotation, maxSwingDegreesPerSecond * Time.deltaTime);
                    movePlayerCloserToSafeZone();

                    if (Mathf.Abs(Quaternion.Angle(swingingPart.localRotation, closedRotation)) <= doorSwingSnapAngle)
                    {

                        swingingPart.localRotation = closedRotation;

                        if (playSounds && doorLatchSounds)
                        {
                            doorLatchSounds.Play(doorSpeaker);
                        }

                        setDoorHandleInteractionStrings(openDoorString);
                        currentDoorState = eDoorState.CLOSED;
                        releasePlayerFromSafeZone();

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
                    releasePlayerFromSafeZone();

                }

            }
            else if (currentDoorState == eDoorState.OPENING)
            {

                if((stopIfDoorHitsSomething == true && doorHitSomething() == false) || stopIfDoorHitsSomething == false)
                {

                    swingingPart.localRotation = Quaternion.RotateTowards(swingingPart.localRotation, openRotation, maxSwingDegreesPerSecond * Time.deltaTime);
                    movePlayerCloserToSafeZone();

                    if (Mathf.Abs(Quaternion.Angle(swingingPart.localRotation, openRotation)) <= doorSwingSnapAngle)
                    {

                        swingingPart.localRotation = openRotation;
                        setDoorHandleInteractionStrings(closeDoorString);
                        currentDoorState = eDoorState.OPEN;
                        releasePlayerFromSafeZone();

                    }

                }
                else
                {

                    if (playSounds && doorBlockedSounds)
                    {
                        doorBlockedSounds.Play(doorSpeaker);
                    }

                    currentDoorState = eDoorState.BLOCKED_PARTLY_OPEN;
                    setDoorHandleInteractionStrings(closeDoorString);
                    releasePlayerFromSafeZone();

                }

            }
            else if (currentDoorState == eDoorState.OPEN)
            {
                // Do nothing
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

        /// <summary>
        /// Checks if door hit something based on its current state.
        /// </summary>
        /// <returns>True if the relevent child hit helper has hit something.</returns>
        protected override bool doorHitSomething()
        {

            bool result = false;

            if(currentDoorState == eDoorState.OPENING)
            {

                if (myHitHelpers[(int)eDoorActionType.OPEN] != null && myHitHelpers[(int)eDoorActionType.OPEN].HasHitSomething)
                {
                    result = true;
                }

            }
            else if (currentDoorState == eDoorState.CLOSING)
            {

                if (myHitHelpers[(int)eDoorActionType.CLOSE] != null && myHitHelpers[(int)eDoorActionType.CLOSE].HasHitSomething)
                {
                    result = true;
                }

            }

            return result;

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

        protected override void openTheDoor(bool makeSound = true)
        {

            // Check where the player is relative to the door and move them as required
            if (playerCloserToSwingOutSide())
            {

                needToMovePlayerToSafeZone = true;
                putPlayerInSafeZone(safeZoneSwingOut);

            }
            else
            {

                // We only need to move the player if they are in danger of being hit by the door. If it's closed all the way, they are not in danger because the door
                // cannot open toward them when they are closer to the inside
                if(currentDoorState != eDoorState.CLOSED)
                {

                    needToMovePlayerToSafeZone = true;
                    putPlayerInSafeZone(safeZoneSwingIn);

                }
                else
                {

                    needToMovePlayerToSafeZone = false;

                }

            }

            if (playSounds && doorOpenSounds)
            {
                doorOpenSounds.Play(doorSpeaker);
            }

            resetHitHelpers();
            currentDoorState = eDoorState.OPENING;

        }

        protected override void closeTheDoor()
        {

            // Check where the player is relative to the door and move them as required
            if (playerCloserToSwingOutSide())
            {

                needToMovePlayerToSafeZone = true;
                putPlayerInSafeZone(safeZoneSwingOut);

            }
            else
            {

                needToMovePlayerToSafeZone = true;
                putPlayerInSafeZone(safeZoneSwingIn);

            }

            if (playSounds && doorCloseSounds)
            {
                doorCloseSounds.Play(doorSpeaker);
            }

            resetHitHelpers();
            currentDoorState = eDoorState.CLOSING;

        }

        protected override void handleExternalLockActivation()
        {

            if (playSounds && doorBlockedSounds)
            {
                doorBlockedSounds.Play(doorSpeaker);
            }

            setDoorHandleInteractionStrings(externalLockString);

        }

        private bool playerCloserToSwingOutSide()
        {

            bool closerToOut = false;

            if (autoMovePlayerToSafeZones)
            {

                float distanceToSwingOut = Vector3.Distance(FPEPlayer.Instance.gameObject.transform.position, swingOutSide.position);
                float distanceToSwingIn = Vector3.Distance(FPEPlayer.Instance.gameObject.transform.position, swingInSide.position);

                if (distanceToSwingOut < distanceToSwingIn)
                {
                    closerToOut = true;
                }

            }

            return closerToOut;

        }

        private void putPlayerInSafeZone(Transform zone)
        {

            if (autoMovePlayerToSafeZones)
            {

                FPEInteractionManagerScript.Instance.BeginCutscene();
                currentSafeZoneTargetPosition = zone.position;
                currentSafeZoneTargetPosition.y = FPEPlayer.Instance.transform.position.y;
                needToMovePlayerToSafeZone = true;
                currentSafeLookTargetPosition = Camera.main.transform.position + Camera.main.transform.forward * 2f;
                currentSafeLookTargetPosition.y = doorActionLookTarget.position.y;

            }

        }

        private void releasePlayerFromSafeZone()
        {

            if (autoMovePlayerToSafeZones)
            {
                needToMovePlayerToSafeZone = false;
                FPEInteractionManagerScript.Instance.EndCutscene();
            }

        }

        private void movePlayerCloserToSafeZone()
        {

            if (autoMovePlayerToSafeZones && needToMovePlayerToSafeZone)
            {
                
                FPEPlayer.Instance.gameObject.transform.position = Vector3.Lerp(FPEPlayer.Instance.gameObject.transform.position, currentSafeZoneTargetPosition, playerAutoMoveUnitsPerSecond * Time.deltaTime);

                currentSafeLookTargetPosition = Vector3.Lerp(currentSafeLookTargetPosition, doorActionLookTarget.position, playerAutoMoveLookPanSpeed * Time.deltaTime);
                FPEPlayer.Instance.GetComponent<FPEFirstPersonController>().forcePlayerLookToPosition(currentSafeLookTargetPosition);

            }

        }

        public override FPEDoorSaveData getSaveGameData()
        {

            Vector3 customAngles = swingingPart.localRotation.eulerAngles;
            return new FPEDoorSaveData(gameObject.name, currentDoorState, doorHandles[0].GetComponent<FPEInteractableActivateScript>().interactionString, customAngles, internallyLocked, externallyLocked);

        }

        public override void restoreSaveGameData(FPEDoorSaveData data)
        {

            currentDoorState = data.DoorState;
            internallyLocked = data.IsInternallyLocked;
            externallyLocked = data.IsExternallyLocked;
            setDoorHandleInteractionStrings(data.DoorHandleString);

            switch (currentDoorState)
            {

                case eDoorState.CLOSING:
                case eDoorState.BLOCKED_PARTLY_CLOSED:
                    swingingPart.localRotation = closedRotation * Quaternion.Euler(data.CustomDoorVector);
                    break;

                case eDoorState.CLOSED:
                    swingingPart.localRotation = closedRotation;
                    break;

                case eDoorState.OPENING:
                case eDoorState.BLOCKED_PARTLY_OPEN:
                    swingingPart.localRotation = closedRotation * Quaternion.Euler(data.CustomDoorVector);
                    break;

                case eDoorState.OPEN:
                    swingingPart.localRotation = openRotation;
                    break;

                default:
                    Debug.LogError("FPESwingingDoor.restoreSaveGameData():: Given bad door state '" + currentDoorState + "'");
                    break;

            }

        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {

            if (drawDebugGizmos)
            {

                Color c = Color.red;

                if (swingingPart != null)
                {

                    Gizmos.color = c;
                    Gizmos.DrawLine(swingingPart.transform.position - swingingPart.transform.up, swingingPart.transform.position + swingingPart.transform.up);
                    Gizmos.DrawWireSphere(swingingPart.transform.position, 0.1f);
                    Gizmos.DrawIcon(swingingPart.transform.position, "Whilefun/doorHinge.png", false);

                    // Represent the swinging limits of our door (ish)
                    c = Color.blue;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Matrix4x4 oldMatrix = Gizmos.matrix;

                    // Fake door should be about the size of a 2wx3h door
                    Vector3 fakeDoorScale = new Vector3(2f, 3f, 0.1f);
                    // Matrix is transformed about our hinge position, and combined rotation of swingAngle AND absolute parent object world rotation
                    Matrix4x4 tempMatrix = Matrix4x4.TRS(swingingPart.position, Quaternion.Euler(new Vector3(0f, swingAngle, 0f)) * transform.rotation, fakeDoorScale*doorGhostScale);
                    Gizmos.matrix = tempMatrix;

                    // If we have left hand inswing doors, we need to flip the orientation of the ghost door
                    if (swingAngle > 0)
                    {
                        Gizmos.DrawCube(new Vector3(0.5f, 0f, 0f), Vector3.one);
                    }
                    else
                    {
                        Gizmos.DrawCube(new Vector3(-0.5f, 0f, 0f), Vector3.one);
                    }

                    // Then switch back
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

                if (swingInSide && swingOutSide)
                {

                    c = Color.magenta;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(swingInSide.position, 0.25f);
                    Gizmos.DrawSphere(swingOutSide.position, 0.25f);

                }

                if (safeZoneSwingIn && safeZoneSwingOut)
                {

                    c = Color.cyan;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(safeZoneSwingIn.position, 0.5f);
                    Gizmos.DrawIcon(safeZoneSwingIn.position, "Whilefun/doorSafeZone.png", false);
                    Gizmos.DrawSphere(safeZoneSwingOut.position, 0.5f);
                    Gizmos.DrawIcon(safeZoneSwingOut.position, "Whilefun/doorSafeZone.png", false);

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

                if (doorActionLookTarget != null)
                {

                    c = Color.blue;
                    c.a = 0.2f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(doorActionLookTarget.position, 0.25f);

                }

                if (currentSafeLookTargetPosition != Vector3.zero)
                {

                    c = Color.red;
                    c.a = 0.2f;
                    Gizmos.color = c;
                    Gizmos.DrawWireSphere(currentSafeLookTargetPosition, 0.25f);

                }

            }

        }
       
#endif

    }

}