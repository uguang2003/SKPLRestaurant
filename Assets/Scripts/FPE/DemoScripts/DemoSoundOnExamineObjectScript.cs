using UnityEngine;
using System.Collections;

using Whilefun.FPEKit;

//
// DemoSoundOnExamineObjectScript
//
// This script is an example of how you make a special Pickup type
// object that plays an additional sound when the object is examined,
// and stops playing it when the player stops examining it.
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
[RequireComponent(typeof(AudioSource))]
public class DemoSoundOnExamineObjectScript : FPEInteractablePickupScript {

	[Header("Special 'Examine' Sound Items")]
	[Tooltip("Set to true if you want to play a sound when Examination starts.")]
	public bool playSoundOnExamination = false;
	[Tooltip("If set to true, the specified sound is looped as long as player is examining object. If player stops examining object, looping sounds are stopped. Non-looping sounds will play to completion.")]
	public bool loopExaminationSound = false;
	[Tooltip("Set to true if the object should play the sound every time the object is examined.")]
	public bool playSoundEveryTime = false;
	[Tooltip("The Audio Clip you want to be played on examination start. If no Audio Clip is specified, no sound is played.")]
	public AudioClip examinationSound = null;

	private AudioSource myAudioSource;
	private bool havePlayedSoundOnce = false;

	public override void Awake(){
		
		// Always call base Awake
		base.Awake();

		if(playSoundOnExamination){

			if(examinationSound){

				myAudioSource = gameObject.GetComponent<AudioSource>();
				myAudioSource.clip = examinationSound;

			}else{
				Debug.LogWarning("DemoSoundOnExamineObjectScript:: 'Play Sound On Examination' set to true, but no 'Examination Sound' Audio Clip specified.");
			}

		}

	}
	
	public override void Start(){

		// Always call base Start
		base.Start();

	}
	
	public override void Update(){
		
		// Always call base Update
		base.Update();

	}

	// The start/end examination functions are overidden so we can
	// start and stop the additional "examination" sound
	public override void startExamination(){

		base.startExamination();

		if(playSoundOnExamination && !havePlayedSoundOnce || playSoundEveryTime){

			myAudioSource.clip = examinationSound;
			myAudioSource.loop = loopExaminationSound;
			myAudioSource.Play();
			havePlayedSoundOnce = true;

		}


	}

	public override void endExamination(){

		base.endExamination();

		// Only do a hard stop on looping sounds.
		if(playSoundOnExamination && myAudioSource.isPlaying && loopExaminationSound){
			myAudioSource.Stop();
			myAudioSource.loop = false;
		}

	}
	
}
