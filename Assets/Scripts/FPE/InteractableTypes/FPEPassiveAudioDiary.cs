using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEPassiveAudioDiary
    // This script allows audio diaries to be attached to any other Interactable 
    // object. It is useful for triggering an audio diary when another type of 
    // interaction takes place. For example, when a player picks up a photo, you
    // could play a narration revealing some backstory.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEPassiveAudioDiary : MonoBehaviour
    {

        [SerializeField, Tooltip("The audio diary title is displayed on screen when the diary is playing.")]
        private string diaryTitle = "";
        public string DiaryTitle {
            get { return diaryTitle; }
        }

        [SerializeField, Tooltip("The actual audio clip the diary represents. This is played when the diary is triggered.")]
        private AudioClip diaryAudio = null;
        public AudioClip DiaryAudio {
            get { return diaryAudio; }
        }

        [SerializeField, Tooltip("If true, the audio diary entry will play as soon as player looks at the parent object. If false, entry will not play until parent Interactable is interacted with.")]
        private bool automaticPlayback = true;
        public bool AutomaticPlayback { get { return automaticPlayback; } }

        [SerializeField, Tooltip("If true, the audio diary entry will be added to the inventory list once playback starts.")]
        private bool addEntryToInventory = true;
        public bool AddEntryToInventory { get { return addEntryToInventory; } }

        [SerializeField, Tooltip("If true, audio title will be displayed during playback.")]
        private bool showDiaryTitle = true;
        public bool ShowDiaryTitle {
            get { return showDiaryTitle; }
        }

        private bool hasBeenPlayed = false;
        public bool HasBeenPlayed {
            get { return hasBeenPlayed; }
        }

        public void collectAudioDiary()
        {
            hasBeenPlayed = true;
        }

        public FPEAudioDiaryPlayedStateSaveData getSaveGameData()
        {
            return new FPEAudioDiaryPlayedStateSaveData(gameObject.name, hasBeenPlayed);
        }

        public void restoreSaveGameData(FPEAudioDiaryPlayedStateSaveData data)
        {
            hasBeenPlayed = data.HasBeenPlayed;
        }

    }

}

