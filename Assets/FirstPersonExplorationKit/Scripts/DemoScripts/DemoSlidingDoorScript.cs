using UnityEngine;
using Whilefun.FPEKit;

//
// DemoSlidingDoorScript
// This script demonstrates how you can make a complex custom object that is 
// also saveable by making it a child class of FPEGenericSaveableGameObject
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
[System.Obsolete("DemoSlidingDoorScript will not be maintained beyond v2.2. Please migrate your doors to use the new door classes such as FPESlidingDoor, FPESwingingDoor, etc.")]
public class DemoSlidingDoorScript : FPEGenericSaveableGameObject {

    private GameObject thePlayer;
    private GameObject doorA;
    private GameObject doorB;
    private GameObject walkBlocker;
    private Vector3 doorAOpenPosition;
    private Vector3 doorAClosedPosition;
    private Vector3 doorBOpenPosition;
    private Vector3 doorBClosedPosition;
    private float doorMovementSpeed = 3.0f;
    private float doorAutoCloseTime = 3.0f;
    private float doorAutoCloseCountdown = 0.0f;
    private float doorAutoCloseZoneRadius = 2.0f;
    private bool doAutoOpenClose = false;
    private bool doorLocked = true;
    private enum eDoorState { CLOSED, CLOSING, OPENING, OPEN };
    private eDoorState currentDoorState = eDoorState.CLOSED;

    private DemoDoorCardScannerScript myCardScanner;

    void Awake()
    {
        myCardScanner = transform.Find("DoorCardScanner").GetComponent<DemoDoorCardScannerScript>();
    }

    void Start() {

        thePlayer = FPEPlayer.Instance.gameObject;

        doorA = transform.Find("doubleSlidingDoor/DoorA").gameObject;
        doorB = transform.Find("doubleSlidingDoor/DoorB").gameObject;
        walkBlocker = transform.Find("WalkBlocker").gameObject;

        doorAClosedPosition = doorA.transform.position;
        doorAOpenPosition = doorA.transform.position;
        doorAOpenPosition.z -= 1.2f;

        doorBClosedPosition = doorB.transform.position;
        doorBOpenPosition = doorB.transform.position;
        doorBOpenPosition.z += 1.2f;

    }

    void Update() {

        // If the player is within the automated movement zone, always move to OPENING state
        if (Vector3.Distance(transform.position, thePlayer.transform.position) < doorAutoCloseZoneRadius)
        {

            doAutoOpenClose = false;

            if (currentDoorState == eDoorState.CLOSED || currentDoorState == eDoorState.CLOSING)
            {

                if (!doorLocked)
                {
                    gameObject.GetComponent<AudioSource>().Play();
                    currentDoorState = eDoorState.OPENING;
                }

            }

        }
        else
        {

            doAutoOpenClose = true;

        }

        // State Management //
        if (currentDoorState == eDoorState.OPENING)
        {

            doorA.transform.position = Vector3.Lerp(doorA.transform.position, doorAOpenPosition, doorMovementSpeed * Time.deltaTime);
            doorB.transform.position = Vector3.Lerp(doorB.transform.position, doorBOpenPosition, doorMovementSpeed * Time.deltaTime);

            if (Vector3.Distance(doorA.transform.position, doorAOpenPosition) < 0.65f)
            {
                // we want to disable collider sooner than doors being all the way open
                walkBlocker.GetComponent<BoxCollider>().enabled = false;
            }

            if (Vector3.Distance(doorA.transform.position, doorAOpenPosition) < 0.2f)
            {

                setDoorOpen();

            }

        }
        else if (currentDoorState == eDoorState.OPEN)
        {

            // Only auto-close if it is safe to do so
            if (doAutoOpenClose)
            {

                doorAutoCloseCountdown -= Time.deltaTime;

                if (doorAutoCloseCountdown <= 0.0f)
                {

                    currentDoorState = eDoorState.CLOSING;
                    walkBlocker.GetComponent<BoxCollider>().enabled = true;
                    gameObject.GetComponent<AudioSource>().Play();

                }

            }

        }
        else if (currentDoorState == eDoorState.CLOSING)
        {

            doorA.transform.position = Vector3.Lerp(doorA.transform.position, doorAClosedPosition, doorMovementSpeed * Time.deltaTime);
            doorB.transform.position = Vector3.Lerp(doorB.transform.position, doorBClosedPosition, doorMovementSpeed * Time.deltaTime);

            if (Vector3.Distance(doorA.transform.position, doorAClosedPosition) < 0.2f)
            {
                setDoorClosed();
            }

        }

    }

    public bool isDoorLocked()
    {
        return doorLocked;
    }

    public void unlockTheDoor()
    {
        doorLocked = false;
    }

    private void setDoorOpen()
    {
        doorA.transform.position = doorAOpenPosition;
        doorB.transform.position = doorBOpenPosition;
        currentDoorState = eDoorState.OPEN;
        doorAutoCloseCountdown = doorAutoCloseTime;
        walkBlocker.GetComponent<BoxCollider>().enabled = false;
    }

    private void setDoorClosed()
    {
        doorA.transform.position = doorAClosedPosition;
        doorB.transform.position = doorBClosedPosition;
        currentDoorState = eDoorState.CLOSED;
        walkBlocker.GetComponent<BoxCollider>().enabled = true;
    }

    // Here, we implemented the required interface function for saving a generic object. For this door, we use all of the fields.
    public override FPEGenericObjectSaveData getSaveGameData()
    {
        return new FPEGenericObjectSaveData(gameObject.name, (int)currentDoorState, doorAutoCloseCountdown, doorLocked);
    }

    // Here, we implemented the required interface function for loading a generic object. There is some custom logic to properly restore the door's locked state, animation state, timing, etc.
    public override void restoreSaveGameData(FPEGenericObjectSaveData data)
    {

        currentDoorState = (eDoorState)data.SavedInt;

        switch (currentDoorState)
        {

            case eDoorState.OPEN:
            case eDoorState.OPENING:
                setDoorOpen();
                break;

            case eDoorState.CLOSED:
            case eDoorState.CLOSING:
                setDoorClosed();
                break;

        }

        doorAutoCloseCountdown = data.SavedFloat;
        doorLocked = data.SavedBool;
        myCardScanner.setLockLightColor(doorLocked);

    }

}
