using UnityEngine;
using UnityEngine.Events;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPEInteractableAudioDiaryScript
    // This script is similar to the Static type, but it triggers the 
    // playback of an Audio Diary/Log recording. This is ideal for
    // significant game moments when story plot points or world lore 
    // needs to be explained, voice overs are needed for tutorials or 
    // player guidance, etc. When the audio is done playing or skipped, 
    // the stop function is called, and the object can take on a new
    // interaction string to correspond to the lore or plot points 
    // explained in the audio.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInteractableAudioDiaryScript : FPEInteractableBaseScript
    {

        [SerializeField, Tooltip("If true, player can interact with this while holding something. If false, they cannot.")]
        protected bool canInteractWithWhileHoldingObject = true;

        [Tooltip("The audio diary title is displayed on screen when the diary is playing.")]
        public string audioDiaryTitle = "DEFAULT DIARY TITLE";
        [Tooltip("The actual audio clip the diary represents. This is played when the diary is triggered.")]
        public AudioClip audioDiaryClip;
        [Tooltip("The interacton string assigned once the audio diary has started playback. Leave blank to keep the same pre-diary interaction string.")]
        public string duringPlaybackInteractionString = "Playing...";
        [Tooltip("The interacton string assigned after the audio diary has finished/been skipped. Leave blank to keep the same pre-diary interaction string.")]
        public string postPlaybackInteractionString = "";

        [SerializeField, Tooltip("If true, the audio diary will play as soon as player looks at the object. If false, player must manually interact with it to begin playback.")]
        private bool automaticPlayback = true;
        public bool AutomaticPlayback {  get { return automaticPlayback; } }

        [SerializeField, Tooltip("If true, the audio diary will be added to the inventory list once playback starts.")]
        private bool addEntryToInventory = true;
        public bool AddEntryToInventory {  get { return addEntryToInventory; } }

        [SerializeField, Tooltip("If true, audio title will be displayed during playback.")]
        private bool showDiaryTitle = true;
        public bool ShowDiaryTitle {
            get { return showDiaryTitle; }
        }

        [Serializable]
        public class AudioDiaryStopEvent : UnityEvent { }
        [SerializeField, Tooltip("If specified, this event will fire when the audio diary is stopped, or finishes playing on its own.")]
        private AudioDiaryStopEvent myStopEvent = null;

        private bool hasBeenPlayed = false;

        public override void Awake()
        {

            base.Awake();
            interactionType = eInteractionType.AUDIODIARY;

            if (gameObject.GetComponent<FPEPassiveAudioDiary>())
            {
                Debug.LogWarning("FPEInteractableAudioDiaryScript:: Object '"+gameObject.name+ "' is of type AUDIODIARY, but also has an FPEPassiveAudioDiary component attached. The FPEPassiveAudioDiary component is redundant and will not be used.", gameObject);
            }

        }

        public override void Start()
        {
            base.Start();
        }

        public override bool interactionsAllowedWhenHoldingObject()
        {
            return canInteractWithWhileHoldingObject;
        }

        public override void interact()
        {

            base.interact();

            if (!automaticPlayback)
            {
                playDiary();
            }

        }

        public override void discoverObject()
        {

            // Always call base function
            base.discoverObject();

            if (automaticPlayback)
            {
                playDiary();
            }

        }

        public void stopAudioDiary()
        {

            if (postPlaybackInteractionString != "")
            {
                interactionString = postPlaybackInteractionString;
            }

            if(myStopEvent != null)
            {
                myStopEvent.Invoke();
            }

        }

        private void playDiary()
        {

            if (!hasBeenPlayed)
            {

                hasBeenPlayed = true;

                if(duringPlaybackInteractionString != "")
                {
                    interactionString = duringPlaybackInteractionString;
                }

                interactionManager.GetComponent<FPEInteractionManagerScript>().playNewAudioDiary(gameObject);

            }

        }

        public FPEAudioDiaryPlayedStateSaveData getSaveGameData()
        {
            return new FPEAudioDiaryPlayedStateSaveData(gameObject.name, hasBeenPlayed);
        }

        public void restoreSaveGameData(FPEAudioDiaryPlayedStateSaveData data)
        {

            hasBeenPlayed = data.HasBeenPlayed;

            if (hasBeenPlayed && postPlaybackInteractionString != "")
            {
                interactionString = postPlaybackInteractionString;
            }

        }

    }

}