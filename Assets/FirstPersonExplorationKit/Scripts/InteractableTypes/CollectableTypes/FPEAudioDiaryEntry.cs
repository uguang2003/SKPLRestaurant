using UnityEngine;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPEAudioDiaryEntry
    // This class holds audio diary data and allows that data to be 
    // passed back and forth to various game systems without the
    // overhead of a monobehaviour.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [Serializable]
    public class FPEAudioDiaryEntry
    {

        private string diaryTitle = "";
        public string DiaryTitle {
            get { return diaryTitle; }
        }

        private AudioClip diaryAudio = null;
        public AudioClip DiaryAudio {
            get { return diaryAudio; }
        }

        private bool showDiaryTitle = true;
        public bool ShowDiaryTitle {
            get { return showDiaryTitle; }
        }

        private bool collected = false;
        public bool Collected {
            get { return collected; }
        }

        public FPEAudioDiaryEntry(string title, AudioClip audio, bool showTitle)
        {
            diaryTitle = title;
            diaryAudio = audio;
            showDiaryTitle = showTitle;
        }

        public void collectAudioDiary()
        {
            collected = true;
        }

        public string getAudioDiaryClipPath()
        {
            return diaryAudio.name;
        }

    }

}

