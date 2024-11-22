using System;
using UnityEngine;
using Whilefun.FPEKit;

[RequireComponent(typeof(AudioSource))]
[System.Obsolete("DemoCabinetScript will not be maintained beyond v2.2. Please migrate your objects using this script to use the new FPESwingingDoor class. See demoComplexDesk prefab for an example.")]
public class DemoCabinetScript : FPEGenericSaveableGameObject
{

    private bool cabinetOpen = false;
    private GameObject doorOpenerLeft = null;
    private GameObject doorOpenerRight = null;

    public AudioClip cabinetOpenSound;
    public AudioClip cabinetCloseSound;
    private AudioSource cabinetSpeaker;

    void Awake()
    {

        doorOpenerLeft = transform.Find("LeftDoor/DoorOpenerLeft").gameObject;
        doorOpenerRight = transform.Find("RightDoor/DoorOpenerRight").gameObject;
        cabinetSpeaker = gameObject.GetComponent<AudioSource>();

    }

    public void toggleDoors()
    {

        if (cabinetOpen)
        {
            closeCabinet();
        }
        else
        {
            openCabinet();
        }

    }

    public void openCabinet()
    {

        doorOpenerLeft.GetComponent<FPEInteractableActivateScript>().interactionString = "Close cabinet";
        doorOpenerRight.GetComponent<FPEInteractableActivateScript>().interactionString = "Close cabinet";
        gameObject.GetComponent<Animator>().SetTrigger("OpenCabinet");
        gameObject.GetComponent<Animator>().ResetTrigger("CloseCabinet");
        gameObject.GetComponent<Animator>().SetBool("ForceOpenCabinet", false);

        cabinetSpeaker.clip = cabinetOpenSound;
        cabinetSpeaker.Play();

    }

    public void closeCabinet()
    {

        doorOpenerLeft.GetComponent<FPEInteractableActivateScript>().interactionString = "Open cabinet";
        doorOpenerRight.GetComponent<FPEInteractableActivateScript>().interactionString = "Open cabinet";
        gameObject.GetComponent<Animator>().SetTrigger("CloseCabinet");
        gameObject.GetComponent<Animator>().ResetTrigger("OpenCabinet");
        gameObject.GetComponent<Animator>().SetBool("ForceOpenCabinet", false);

        cabinetSpeaker.clip = cabinetCloseSound;
        cabinetSpeaker.Play();

    }

    public void setCabinetOpen()
    {
        cabinetOpen = true;
    }

    public void setCabinetClosed()
    {
        cabinetOpen = false;
    }

    public bool isCabinetOpen()
    {
        return cabinetOpen;
    }

    public override FPEGenericObjectSaveData getSaveGameData()
    {
        return new FPEGenericObjectSaveData(gameObject.name, 0, 0f, cabinetOpen);
    }

    public override void restoreSaveGameData(FPEGenericObjectSaveData data)
    {

        cabinetOpen = data.SavedBool;

        if (cabinetOpen)
        {

            doorOpenerLeft.GetComponent<FPEInteractableActivateScript>().interactionString = "Close cabinet";
            doorOpenerRight.GetComponent<FPEInteractableActivateScript>().interactionString = "Close cabinet";
            gameObject.GetComponent<Animator>().SetBool("ForceOpenCabinet", true);
            gameObject.GetComponent<Animator>().ResetTrigger("CloseCabinet");
            gameObject.GetComponent<Animator>().ResetTrigger("OpenCabinet");

        }

    }

}
