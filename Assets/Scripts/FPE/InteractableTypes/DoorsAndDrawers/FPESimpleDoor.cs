using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPESimpleDoor
    // This script is the basis for a simple sliding door type object 
    // that can be configured as part of an Activate-Type Toggle interaction.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [System.Obsolete("FPESimpleDoor will not be maintained beyond v2.2. Please migrate your doors to use the new door classes such as FPESlidingDoor, FPESwingingDoor, etc.")]
    public class FPESimpleDoor : FPEDoor
    {

        private Vector3 openedPosition = Vector3.zero;
        private Vector3 closedPosition = Vector3.zero;
        private FPEInteractableActivateScript myDoorHandle = null;

        [Header("Moving Parts Configuration")]
        [SerializeField, Tooltip("How far the sliding part of the door slides.")]
        private float slidingDistance = 2.0f;
        [SerializeField, Tooltip("How quickly the door opens.")]
        private float openSpeed = 6.0f;
        [SerializeField, Tooltip("How quickly the door closes.")]
        private float closeSpeed = 4.0f;
        [SerializeField, Tooltip("The distance at which the door will snap to open position when opening.")]
        private float openSnapDistance = 0.01f;
        [SerializeField, Tooltip("The distance at which the door will snap to closed position when closing.")]
        private float closeSnapDistance = 0.1f;

        [SerializeField, Tooltip("The sliding part of the door.")]
        private GameObject slidingPart = null;
        [SerializeField, Tooltip("This object blocks the player's path when the door is opening, closing, or fully closed. Should have a primitive collider attached (e.g. a basic cube will work fine).")]
        public GameObject playerBlocker = null;
        [SerializeField, Tooltip("This optional object blocks the raycast from player to door handle. It prevents player from closing door on themselves from inside the doorway.")]
        public GameObject playerInteractionBlocker = null;

        [Header("Sound")]
        [SerializeField, Tooltip("Sound plays when door is opened")]
        private AudioClip doorOpen = null;
        [SerializeField, Tooltip("Sound plays when door is closed")]
        private AudioClip doorClose = null;
        [SerializeField, Tooltip("Sound plays when door is locked and someone tries to open it")]
        private AudioClip doorLocked = null;

        private AudioSource myAudio = null;

        [Header("Interaction Text")]
        [SerializeField]
        private string openText = "Open Door";
        [SerializeField]
        private string closeText = "Close Door";
        [SerializeField]
        private string lockedText = "It's Locked";

        protected override void Awake()
        {

            base.Awake();

            if (!slidingPart || !playerBlocker)
            {
                Debug.Log("Door cannot find one of sliding part or player blocker!");
            }

            closedPosition = slidingPart.transform.position;
            openedPosition = closedPosition + slidingPart.transform.right * slidingDistance;

            myAudio = gameObject.GetComponent<AudioSource>();

            if (!myAudio)
            {
                myAudio = gameObject.AddComponent<AudioSource>();
            }

            myAudio.loop = false;
            myAudio.playOnAwake = false;

            // Ensure the door has a door handle
            FPEInteractableActivateScript[] childActivates = gameObject.GetComponentsInChildren<FPEInteractableActivateScript>();

            for (int a = 0; a < childActivates.Length; a++)
            {

                if (childActivates[a].gameObject.name == "DoorHandle")
                {
                    myDoorHandle = childActivates[a];
                    break;
                }

            }

            if (!myDoorHandle)
            {
                Debug.LogError("FPESimpleDoor:: No child Activate Type object called 'DoorHandle' was found on door '" + gameObject.name + "'. Ensure the door has a handle, otherwise it can't be opened!");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }

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

                slidingPart.transform.position = Vector3.Lerp(slidingPart.transform.position, closedPosition, closeSpeed * Time.deltaTime);

                if (Vector3.Distance(slidingPart.transform.position, closedPosition) < closeSnapDistance)
                {
                    setDoorClosed(Vector3.zero);
                }

            }
            else if (currentDoorState == eDoorState.OPENING)
            {

                slidingPart.transform.position = Vector3.Lerp(slidingPart.transform.position, openedPosition, openSpeed * Time.deltaTime);

                if (Vector3.Distance(slidingPart.transform.position, openedPosition) < openSnapDistance)
                {
                    setDoorOpen(Vector3.zero);
                }

            }
            else if (currentDoorState == eDoorState.OPEN)
            {
                // Do nothing
            }

        }

        // Assign to the Shared 'Activation' event in the inspector
        public override void activateDoor()
        {

            if (currentDoorState == eDoorState.CLOSED || currentDoorState == eDoorState.CLOSING)
            {
                openTheDoor();
            }
            else if (currentDoorState == eDoorState.OPEN || currentDoorState == eDoorState.OPENING)
            {
                closeTheDoor();
            }

        }

        protected override void openTheDoor(bool makeSound = true)
        {

            currentDoorState = eDoorState.OPENING;
            playerBlocker.SetActive(false);

            if (playerInteractionBlocker)
            {
                playerInteractionBlocker.SetActive(true);
            }

            myAudio.PlayOneShot(doorOpen);
            setDoorHandleInteractionStrings(closeText);

        }

        protected override void closeTheDoor()
        {

            currentDoorState = eDoorState.CLOSING;
            playerBlocker.SetActive(true);

            if (playerInteractionBlocker)
            {
                playerInteractionBlocker.SetActive(true);
            }

            myAudio.PlayOneShot(doorClose);
            setDoorHandleInteractionStrings(openText);

        }

        // Assign to Activation Failure event in the inspector
        public void doorOpenFailure()
        {

            currentDoorState = eDoorState.CLOSED;
            myAudio.PlayOneShot(doorLocked);
            setDoorHandleInteractionStrings(lockedText);

        }

        protected override void handleExternalLockActivation()
        {
            Debug.LogWarning("Legacy class FPESimpleDoor has intentially not implemented function handleExternalLockActivation()");
        }

        // This is a special case override to ensure this legacy door class is backwards compatible with FPEDoor
        protected override void setDoorHandleInteractionStrings(string s)
        {
            myDoorHandle.setInteractionString(s);
        }

        protected void setDoorClosed(Vector3 customVector)
        {

            slidingPart.transform.position = closedPosition;
            currentDoorState = eDoorState.CLOSED;

            if (playerInteractionBlocker)
            {
                playerInteractionBlocker.SetActive(false);
            }

        }

        protected void setDoorOpen(Vector3 customVector)
        {

            slidingPart.transform.position = openedPosition;
            currentDoorState = eDoorState.OPEN;
            playerBlocker.SetActive(false);

        }

        public override FPEDoorSaveData getSaveGameData()
        {
            return new FPEDoorSaveData(gameObject.name, currentDoorState, myDoorHandle.interactionString, Vector3.zero, false, false);
        }

        public override void restoreSaveGameData(FPEDoorSaveData data)
        {

            currentDoorState = data.DoorState;
            setDoorHandleInteractionStrings(data.DoorHandleString);

            switch (currentDoorState)
            {

                case eDoorState.CLOSING:
                case eDoorState.CLOSED:
                    setDoorClosed(Vector3.zero);
                    break;

                case eDoorState.OPENING:
                case eDoorState.OPEN:
                    setDoorOpen(Vector3.zero);
                    break;

                default:
                    Debug.LogError("FPESimpleDoor.restoreSaveGameData():: Given bad door state '"+ currentDoorState + "'");
                    break;

            }
            
        }

    }

}