using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEAlwaysSwingOutDoor
    // This script is used for very simple "always swing away from the player" doors
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [ExecuteInEditMode]
    public class FPEAlwaysSwingOutDoor : FPEDoor
    {

        [Header("Door-Specific Behaviour")]
        [SerializeField, Tooltip("The angle from closed the door will swing open in either direction. 135 is a good default for most realistic doors.")]
        private float swingAngle = 135.0f;
        [SerializeField, Tooltip("The time (in seconds) it takes for the door to fully open or close.")]
        private float swingOpenTimeInSeconds = 1.0f;
        [SerializeField, Tooltip("The angle (in degrees) at which the door snaps closed. Default is 1.")]
        private float doorSwingSnapAngle = 1.0f;

        private Quaternion closedRotation = Quaternion.identity;
        private Quaternion openRotationLeft = Quaternion.identity;
        private Quaternion openRotationRight = Quaternion.identity;
        private Transform swingingPart = null;
        private float maxSwingDegreesPerSecond = 0f;
        private Quaternion currentSwingOpenRotation = Quaternion.identity;
        private Transform leftHandSide = null;
        private Transform rightHandSide = null;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField]
        private bool drawDebugGizmos = true;
#endif

        protected override void Awake()
        {

            base.Awake();

            Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>();
            doorHandles = new List<Transform>();
            myHitHelpers = new FPEDoorAndDrawerHitHelper[2];

            foreach (Transform t in childTransforms)
            {

                if (t.name == "SwingingPart")
                {
                    swingingPart = t;
                }
                else if (t.name.Contains("DoorHandle") && t.gameObject.GetComponent<FPEInteractableActivateScript>())
                {
                    doorHandles.Add(t);
                }
                else if (t.name == "LeftHandSide")
                {
                    leftHandSide = t;
                }
                else if (t.name == "RightHandSide")
                {
                    rightHandSide = t;
                }
                else if(t.name == "HitHelperLeft" && t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>())
                {
                    myHitHelpers[(int)eDoorSide.LEFT] = t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>();
                }
                else if (t.name == "HitHelperRight" && t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>())
                {
                    myHitHelpers[(int)eDoorSide.RIGHT] = t.gameObject.GetComponent<FPEDoorAndDrawerHitHelper>();
                }


            }

            maxSwingDegreesPerSecond = Mathf.Abs(swingAngle / swingOpenTimeInSeconds);
            closedRotation = Quaternion.identity;
            openRotationLeft = closedRotation * Quaternion.Euler(new Vector3(0f, swingAngle, 0f));
            openRotationRight = closedRotation * Quaternion.Euler(new Vector3(0f, -swingAngle, 0f));
            currentSwingOpenRotation = openRotationLeft;

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

            if (stopIfDoorHitsSomething && (myHitHelpers[(int)eDoorSide.LEFT] == null || myHitHelpers[(int)eDoorSide.RIGHT] == null))
            {
                Debug.LogWarning("FPEAlwaysSwingOutDoor:: Door '" + gameObject.name + "' is configured to stop if door hits something, but it is missing its no child FPEDoorAndDrawerHitHelper objects 'HitHelperLeft' and/or 'HitHelperRight'. Door will not stop when it hits something.", gameObject);
            }

            // Doors must have swinging parts and at least one door handle
            if (!swingingPart || doorHandles.Count == 0 || !leftHandSide || !rightHandSide)
            {

                Debug.Log("FPEAlwaysSwingOutDoor:: Door '" + gameObject.name + "' is missing a required parts. Attempting to fix.", gameObject);

                // Attempt some automatic fixes
#if UNITY_EDITOR
                if (swingingPart == null)
                {

                    GameObject swingingPartFix = new GameObject("SwingingPart");
                    swingingPartFix.transform.parent = gameObject.transform;
                    swingingPartFix.transform.localPosition = Vector3.zero;
                    swingingPart = swingingPartFix.transform;

                }

                if(doorHandles.Count == 0)
                {

                    GameObject doorHandleFix = new GameObject("DoorHandle");
                    doorHandleFix.AddComponent<BoxCollider>();
                    doorHandleFix.AddComponent<FPEInteractableActivateScript>();
                    doorHandleFix.transform.parent = swingingPart;
                    doorHandleFix.transform.localPosition = Vector3.zero;
                    doorHandles.Add(doorHandleFix.transform);
                    Debug.LogWarning("FPEDoor:: Door '" + gameObject.name + "' DoorHandle child has no Activation Event assigned. Door Handle interaction will do nothing until it is assigned. Also remember to make the Event Fire Type be 'EVERYTIME'.", gameObject);

                }

                if(leftHandSide == null || rightHandSide == null)
                {

                    GameObject interactionZoneFix = new GameObject("PlayerInteractionZones");
                    interactionZoneFix.transform.parent = gameObject.transform;
                    interactionZoneFix.transform.localPosition = Vector3.zero;
                    
                    if(leftHandSide == null)
                    {
                        GameObject lhsFix = new GameObject("LeftHandSide");
                        lhsFix.transform.parent = interactionZoneFix.transform;
                        lhsFix.transform.localPosition = Vector3.zero - (transform.forward * 1f);
                        leftHandSide = lhsFix.transform;
                    }

                    if(rightHandSide == null)
                    {
                        GameObject rhsFix = new GameObject("RightHandSide");
                        rhsFix.transform.parent = interactionZoneFix.transform;
                        rhsFix.transform.localPosition = Vector3.zero + (transform.forward * 1f);
                        rightHandSide = rhsFix.transform;
                    }

                }
#endif
            }


            
            // Some angles are not valid and will result in weird first time swings
            if (swingAngle == 0f)
            {
                Debug.LogWarning("FPEAlwaysSwingOutDoor:: Door '" + gameObject.name + "' has a swingAngle of zero. This door will not work.", gameObject);
            }
            else
            {

                float deltaA1 = Quaternion.Angle(swingingPart.localRotation, openRotationLeft);
                float deltaA2 = Quaternion.Angle(swingingPart.localRotation, openRotationRight);
                float deltaB = Quaternion.Angle(swingingPart.localRotation, closedRotation);

                if ((Mathf.Abs(deltaA1) > Mathf.Abs(swingAngle) && Mathf.Abs(deltaA2) > Mathf.Abs(swingAngle)) || Mathf.Abs(deltaB) > Mathf.Abs(swingAngle))
                {
                    Debug.LogWarning("FPEAlwaysSwingOutDoor:: Door '" + gameObject.name + "''s 'SwingingPart' starting rotation is outside defined swingAngle's range. First open/close actions will not behave as expected.", gameObject);
                }

            }

            if (playSounds && doorSpeaker == null)
            {

                Debug.Log("FPEAlwaysSwingOutDoor:: Door '" + gameObject.name + "' is configured to play sounds but has no AudioSource. Attempting to fix.", gameObject);

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

                    if (Mathf.Abs(Quaternion.Angle(swingingPart.localRotation, closedRotation)) <= doorSwingSnapAngle)
                    {

                        swingingPart.localRotation = closedRotation;

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

                    setDoorHandleInteractionStrings(openDoorString);
                    currentDoorState = eDoorState.BLOCKED_PARTLY_CLOSED;

                }

            }
            else if (currentDoorState == eDoorState.OPENING)
            {


                if ((stopIfDoorHitsSomething == true && doorHitSomething() == false) || stopIfDoorHitsSomething == false)
                {

                    swingingPart.localRotation = Quaternion.RotateTowards(swingingPart.localRotation, currentSwingOpenRotation, maxSwingDegreesPerSecond * Time.deltaTime);

                    if (Mathf.Abs(Quaternion.Angle(swingingPart.localRotation, currentSwingOpenRotation)) <= doorSwingSnapAngle)
                    {

                        swingingPart.localRotation = currentSwingOpenRotation;
                        setDoorHandleInteractionStrings(closeDoorString);
                        currentDoorState = eDoorState.OPEN;

                    }

                }
                else
                {

                    if (playSounds && doorBlockedSounds)
                    {
                        doorBlockedSounds.Play(doorSpeaker);
                    }

                    setDoorHandleInteractionStrings(closeDoorString);
                    currentDoorState = eDoorState.BLOCKED_PARTLY_OPEN;

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
        /// Checks if door hit something based on its state and direction of travel.
        /// </summary>
        /// <returns>True if relevent left or right side hit helper has hit something.</returns>
        protected override bool doorHitSomething()
        {

            bool result = false;

            // If opening, check urrent target rotation and check that side's hit helper only
            if(currentDoorState == eDoorState.OPENING)
            {

                if(currentSwingOpenRotation == openRotationLeft)
                {

                    if (myHitHelpers[(int)eDoorSide.LEFT] != null && myHitHelpers[(int)eDoorSide.LEFT].HasHitSomething)
                    {
                        result = true;
                    }

                }
                else
                {

                    if (myHitHelpers[(int)eDoorSide.RIGHT] != null && myHitHelpers[(int)eDoorSide.RIGHT].HasHitSomething)
                    {
                        result = true;
                    }

                }

            }
            // If closing, check which way we're closing from and check opposite hit helper only
            else if (currentDoorState == eDoorState.CLOSING)
            {

                if(Quaternion.Angle(swingingPart.localRotation, openRotationLeft) < Quaternion.Angle(swingingPart.localRotation, openRotationRight))
                {

                    if (myHitHelpers[(int)eDoorSide.RIGHT].HasHitSomething)
                    {
                        result = true;
                    }

                }
                else
                {

                    if (myHitHelpers[(int)eDoorSide.LEFT].HasHitSomething)
                    {
                        result = true;
                    }

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

                // We need to check if player has the required key, and change state accordingly
                GameObject objectInPlayersHand = FPEInteractionManagerScript.Instance.getHeldObject();

                // If player is holding something that is inventory and matches required key type
                if((FPEInventoryManagerScript.Instance.inventoryQuantity(requiredKeyType) > 0) || (objectInPlayersHand != null && objectInPlayersHand.GetComponent<FPEInteractableInventoryItemScript>() && objectInPlayersHand.GetComponent<FPEInteractableInventoryItemScript>().InventoryItemType == requiredKeyType))
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

            // If blocked partially closed, we need to open to the nearest good rotation (either left or right).
            if (currentDoorState == eDoorState.BLOCKED_PARTLY_CLOSED)
            {

                if (Mathf.Abs(Quaternion.Angle(swingingPart.localRotation, openRotationLeft)) < Mathf.Abs(Quaternion.Angle(swingingPart.localRotation, openRotationRight)))
                {
                    currentSwingOpenRotation = openRotationLeft;
                }
                else
                {
                    currentSwingOpenRotation = openRotationRight;
                }

            }
            // Otherwise choose side player is not on.
            else
            {

                // If player is closer to left hand side, target open rotation is rightHandRotation, otherwise target is leftHandRotation
                if (playerCloserToLeftHandSide())
                {
                    currentSwingOpenRotation = openRotationRight;
                }
                else
                {
                    currentSwingOpenRotation = openRotationLeft;
                }

            }

            if (playSounds && doorOpenSounds)
            {
                doorOpenSounds.Play(doorSpeaker);
            }

            resetHitHelpers();
            currentDoorState = eDoorState.OPENING;

        }

        protected override void handleExternalLockActivation()
        {

            if(playSounds && doorBlockedSounds)
            {
                doorBlockedSounds.Play(doorSpeaker);
            }

            setDoorHandleInteractionStrings(externalLockString);

        }

        private bool playerCloserToLeftHandSide()
        {

            bool closerToLeft = false;

            float distanceToLeftHandSide = Vector3.Distance(FPEPlayer.Instance.gameObject.transform.position, leftHandSide.position);
            float distanceToRightHandSide = Vector3.Distance(FPEPlayer.Instance.gameObject.transform.position, rightHandSide.position);

            if (distanceToLeftHandSide < distanceToRightHandSide)
            {
                closerToLeft = true;
            }

            return closerToLeft;

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

                
                case eDoorState.CLOSED:
                    swingingPart.localRotation = closedRotation;
                    break;

                case eDoorState.CLOSING:
                case eDoorState.BLOCKED_PARTLY_CLOSED:
                    swingingPart.localRotation = closedRotation * Quaternion.Euler(data.CustomDoorVector);
                    break;

                case eDoorState.OPENING:
                    swingingPart.localRotation = closedRotation * Quaternion.Euler(data.CustomDoorVector);
                    // Pick closest openRotation, and set target to that
                    if (Quaternion.Angle(swingingPart.localRotation, openRotationLeft) < Quaternion.Angle(swingingPart.localRotation, openRotationRight))
                    {
                        currentSwingOpenRotation = openRotationLeft;
                    }
                    else
                    {
                        currentSwingOpenRotation = openRotationRight;
                    }
                    break;

                case eDoorState.OPEN:
                case eDoorState.BLOCKED_PARTLY_OPEN:
                    swingingPart.localRotation = closedRotation * Quaternion.Euler(data.CustomDoorVector);
                    break;
                
                default:
                    Debug.LogError("FPEAlwaysSwingOutDoor.restoreSaveGameData():: Given bad door state '" + currentDoorState + "'");
                    break;

            }

        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {

            if (drawDebugGizmos)
            {

                Color c = Color.red;

                Transform gizmoSwingingPart = null;

                if (swingingPart != null)
                {
                    gizmoSwingingPart = swingingPart;
                }
                else
                {
                    gizmoSwingingPart = transform.Find("SwingingPart");
                }


                if (gizmoSwingingPart != null)
                {

                    Gizmos.color = c;
                    Gizmos.DrawLine(gizmoSwingingPart.transform.position - gizmoSwingingPart.transform.up, gizmoSwingingPart.transform.position + gizmoSwingingPart.transform.up);
                    Gizmos.DrawWireSphere(gizmoSwingingPart.transform.position, 0.1f);
                    Gizmos.DrawIcon(gizmoSwingingPart.transform.position, "Whilefun/doorHinge.png", false);

                    // Represent the swinging limits of our door in both left and right hand inswing
                    c = Color.blue;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Vector3 fakeDoorScale = new Vector3(2f, 3f, 0.1f);

                    Matrix4x4 tempMatrixPos = Matrix4x4.TRS(gizmoSwingingPart.position, Quaternion.Euler(new Vector3(0f, swingAngle, 0f)) * transform.rotation, fakeDoorScale);
                    Gizmos.matrix = tempMatrixPos;
                    if (swingAngle > 0)
                    {
                        Gizmos.DrawCube(new Vector3(0.5f, 0f, 0f), Vector3.one);
                    }
                    else
                    {
                        Gizmos.DrawCube(new Vector3(-0.5f, 0f, 0f), Vector3.one);
                    }

                    Matrix4x4 tempMatrixNeg = Matrix4x4.TRS(gizmoSwingingPart.position, Quaternion.Euler(new Vector3(0f, -swingAngle, 0f)) * transform.rotation, fakeDoorScale);
                    Gizmos.matrix = tempMatrixNeg;
                    if ((-swingAngle) < 0)
                    {
                        Gizmos.DrawCube(new Vector3(0.5f, 0f, 0f), Vector3.one);
                    }
                    else
                    {
                        Gizmos.DrawCube(new Vector3(-0.5f, 0f, 0f), Vector3.one);
                    }

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

                if (leftHandSide && rightHandSide)
                {

                    c = Color.magenta;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(leftHandSide.position, 0.25f);
                    Gizmos.DrawIcon(leftHandSide.position, "Whilefun/doorLeftInswing.png", false);
                    Gizmos.DrawSphere(rightHandSide.position, 0.25f);
                    Gizmos.DrawIcon(rightHandSide.position, "Whilefun/doorRightInswing.png", false);

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