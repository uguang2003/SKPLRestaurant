using UnityEngine;

using Whilefun.FPEKit;

//
// DemoToiletSeatScript
// This script is attached to the toilet seat and facilitates 
// interaction with it
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
[RequireComponent(typeof (AudioSource))]
public class DemoToiletSeatScript : FPEInteractableActivateScript {

	public AudioClip toiletSeatUp;
	public AudioClip toiletSeatDown;
    private AudioSource myAudio;

    private Vector3 seatOpenAngles = new Vector3(0f, 0f, -92.5f);
    private Vector3 seatClosedAngles = new Vector3(0f, 0f, 0f);

    private Quaternion seatOpenRotation = Quaternion.identity;
    private Quaternion seatClosedRotation = Quaternion.identity;
    private float rotationRate = 10.0f;
    private float snapAngle = 5.0f;

    private enum eSeatState
    {
        OPEN = 0,
        OPENING,
        CLOSING,
        CLOSED
    }
    private eSeatState currentSeatState = eSeatState.CLOSED;
    
    public override void Awake(){

		// Always call back to base class Awake function
		base.Awake();

        myAudio = gameObject.GetComponent<AudioSource>();

        seatOpenRotation = Quaternion.Euler(seatOpenAngles);
        seatClosedRotation = Quaternion.Euler(seatClosedAngles);

    }

    void Update()
    {

        if(currentSeatState == eSeatState.CLOSING)
        {

            transform.localRotation = Quaternion.Slerp(transform.localRotation, seatClosedRotation, rotationRate * Time.deltaTime);

            if (Quaternion.Angle(transform.localRotation, seatClosedRotation) < snapAngle)
            {

                currentSeatState = eSeatState.CLOSED;
                transform.localRotation = seatClosedRotation;
                myAudio.clip = toiletSeatDown;
                myAudio.Play();
                interactionString = "Lift seat";

            }

        }
        else if (currentSeatState == eSeatState.OPENING)
        {

            transform.localRotation = Quaternion.Slerp(transform.localRotation, seatOpenRotation, rotationRate * Time.deltaTime);

            if (Quaternion.Angle(transform.localRotation, seatOpenRotation) < snapAngle)
            {

                currentSeatState = eSeatState.OPEN;
                transform.localRotation = seatOpenRotation;
                myAudio.clip = toiletSeatUp;
                myAudio.Play();
                interactionString = "Put seat down";

            }

        }

    }

	public override void activate()
    {


        if(currentSeatState == eSeatState.OPEN)
        {
            currentSeatState = eSeatState.CLOSING;
        }
        else if (currentSeatState == eSeatState.CLOSED)
        {
            currentSeatState = eSeatState.OPENING;
        }
        
    }

    public int GetSeatState()
    {
        return (int)currentSeatState;
    }

    public void RestorSeatState(int state)
    {

        switch (state)
        {

            case (int)eSeatState.CLOSED:
            case (int)eSeatState.CLOSING:
                currentSeatState = eSeatState.CLOSED;
                transform.localRotation = seatClosedRotation;
                break;
            case (int)eSeatState.OPEN:
            case (int)eSeatState.OPENING:
            default:
                currentSeatState = eSeatState.OPEN;
                transform.localRotation = seatOpenRotation;
                break;

        }

    }

}
