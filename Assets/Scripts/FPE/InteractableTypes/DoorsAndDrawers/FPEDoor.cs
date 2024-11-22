using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Whilefun.FPEKit
{

    //
    // FPEDoor
    // This script is the basis for all other Door types
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public abstract class FPEDoor : MonoBehaviour
    {

        public enum eDoorState
        {

            CLOSED = 0,
            CLOSING,
            OPENING,
            OPEN,
            BLOCKED_PARTLY_OPEN,
            BLOCKED_PARTLY_CLOSED

        }
        protected eDoorState currentDoorState = eDoorState.CLOSED;

        public enum eDoorSide
        {
            LEFT = 0,
            RIGHT = 1
        }

        public enum eDoorActionType
        {
            OPEN = 0,
            CLOSE = 1
        }

        [Header("General Configuration")]
        [SerializeField, Tooltip("If true, the door will be unlocked the first time the player activates its handle using the Required Key.")]
        protected bool startInternallyLocked = false;
        [SerializeField, Tooltip("The type of inventory item required to unlock the door")]
        protected FPEInventoryManagerScript.eInventoryItems requiredKeyType = FPEInventoryManagerScript.eInventoryItems.SIMPLEKEY;
        [SerializeField, Tooltip("If true, the door will never open until an external object unlocks it. (e.g. A security computer is hacked and releases a magnetic lock).")]
        protected bool startExternallyLocked = false;
        [SerializeField, Tooltip("If true, door's default state will be 'OPEN'. Used for doors that are cracked open a little when the game starts. To use, set to true then rotate the SwingingPart transform a little bit in the direction the door swings open.")]
        protected bool startOpened = false;
        [SerializeField, Tooltip("If true, will stop swinging if its FPEDoorAndDrawerHitHelpers hit something.")]
        protected bool stopIfDoorHitsSomething = false;
        [SerializeField, Tooltip("If true, door will consume key from player's inventory when it is internally unlocked")]
        protected bool consumeKeyOnInternalUnlock = false;

        [Header("Door Handle Interaction Text Overrides")]
        [SerializeField, Tooltip("String assigned to door handle when door can be opened")]
        protected string openDoorString = "Open";
        [SerializeField, Tooltip("String assigned to door handle when door can be closed")]
        protected string closeDoorString = "Close";
        [SerializeField, Tooltip("String assigned to door handle when door cannot open and player tries to open it. (E.g. 'It's locked.', 'I think it's jammed.', etc.)")]
        protected string openDoorFailureString = "It's locked.";
        [SerializeField, Tooltip("String assigned to door handle when door was internally locked and player tried to open it with the required key or item. (E.g. 'It's unlocked now.', etc.)")]
        protected string internalUnlockString = "It's unlocked now.";
        [SerializeField, Tooltip("String assigned to door handle when door is externally locked and player tries to open it. (E.g. 'I have to disable the security system first.', etc.)")]
        protected string externalLockString = "I have to disable the security system first.";

        [Header("Sounds")]
        [SerializeField, Tooltip("If true, door will play sounds for various states and actions")]
        protected bool playSounds = true;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Door Opening sounds here. If this field is left unassigned, these sounds will not be played")]
        protected FPESimpleSoundBank doorOpenSounds = null;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Door Closing sounds here. If this field is left unassigned, these sounds will not be played")]
        protected FPESimpleSoundBank doorCloseSounds = null;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Door Latching sounds here. If this field is left unassigned, these sounds will not be played")]
        protected FPESimpleSoundBank doorLatchSounds = null;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Door Blocked sounds here. If this field is left unassigned, these sounds will not be played")]
        protected FPESimpleSoundBank doorBlockedSounds = null;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Door Unlock sounds here. If this field is left unassigned, these sounds will not be played")]
        protected FPESimpleSoundBank doorUnlockSounds = null;
        [SerializeField, Tooltip("If playSounds is true, assign a sound bank for Door Open Failure sounds here. If this field is left unassigned, these sounds will not be played")]
        protected FPESimpleSoundBank doorOpenFailureSounds = null;

        protected AudioSource doorSpeaker = null;
        // Note: There can be more than one door handle, but they should all do the same thing, and will all be given the same 
        // interaction string. If you need special handles that do different things, consider writing a custom door class.
        protected List<Transform> doorHandles = null;
        protected FPEDoorAndDrawerHitHelper[] myHitHelpers = null;
        protected bool internallyLocked = false;
        protected bool externallyLocked = false;

        protected virtual void Awake()
        {

            // Common error checking //
            if(startExternallyLocked && startOpened)
            {
                Debug.LogError("FPEDoor:: Door '" + gameObject.name + "' is configured to start open and start externally locked. This is not allowed.", gameObject);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }

            if (startExternallyLocked)
            {
                externallyLocked = true;
            }

            if (startInternallyLocked)
            {
                internallyLocked = true;
            }

        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {

        }

        /// <summary>
        /// Checks if any of the door hit helpers have hit anything.
        /// </summary>
        /// <returns>True if any child hit helper has hit something.</returns>
        protected virtual bool doorHitSomething()
        {

            bool result = false;

            for (int h = 0; h < myHitHelpers.Length; h++)
            {
                if (myHitHelpers[h].HasHitSomething)
                {
                    result = true;
                    break;
                }
            }

            return result;

        }

        /// <summary>
        /// To reset all hit helpers so we can try to open or close the door again after it became blocked
        /// </summary>
        protected void resetHitHelpers()
        {

            for (int h = 0; h < myHitHelpers.Length; h++)
            {
                if (myHitHelpers[h] != null)
                {
                    myHitHelpers[h].ResetHits();
                }
            }

        }

        protected virtual void setDoorHandleInteractionStrings(string s)
        {

            foreach (Transform handle in doorHandles)
            {
                handle.gameObject.GetComponent<FPEInteractableActivateScript>().setInteractionString(s);
            }

        }

        /// <summary>
        /// Used to disable external lock on the door from another object such (e.g. A security system terminal)
        /// Note: You may want to override this function in your own doors to handle special cases where asking
        /// the door to be unlocked fails.
        /// </summary>
        public virtual bool ExternallyUnlockDoor()
        {

            externallyLocked = false;
            return true;

        }

        /// <summary>
        /// Used to enable external lock on the door from another object such (e.g. A security system terminal)
        /// Note: You may want to override this function in your own doors to handle special cases where asking
        /// the door to be locked fails.
        /// </summary>
        public virtual bool ExternallyLockDoor()
        {

            externallyLocked = true;
            return true;

        }

        /// <summary>
        /// This function is assigned to the "Shared 'Activation' and 'Toggle On' Event" in the 
        /// Inspector of DoorHandle's FPEInteractableActivateScript. However, if you're doing 
        /// something really fancy like separate Activation objects for different door actions, you 
        /// may want to use other functions or write your own. (see FPESwingingDoor.cs for an example) 
        /// </summary>
        public abstract void activateDoor();

        protected abstract void openTheDoor(bool makeSound = true);
        protected abstract void closeTheDoor();

        /// <summary>
        /// This is an internal function that must be implemented to handle cases where the door 
        /// is externally locked, but a call to activateDoor() is made (e.g. player tries to open 
        /// the door using a door handle, but it is externally locked by a security system)
        /// </summary>
        protected abstract void handleExternalLockActivation();

        public abstract FPEDoorSaveData getSaveGameData();
        public abstract void restoreSaveGameData(FPEDoorSaveData data);

    }

}