using UnityEngine;
using UnityEngine.UI;

namespace Whilefun.FPEKit
{

    //
    // FPEHUD
    // This script handles all HUD UI component update based on information set by FPEInteractionManager or any other class that wants to set it.
    //
    // Copyright 2018 While Fun Games
    // http://whilefun.com
    //
    public abstract class FPEHUD : MonoBehaviour
    {

        private static FPEHUD _instance;
        public static FPEHUD Instance {
            get { return _instance; }
        }

        protected bool initialized = false;
        protected FPEHUDData myHUDData = null;

        protected void Awake()
        {

            if (_instance != null)
            {
                Debug.LogWarning("FPEHUD:: Duplicate instance of FPEHUD, deleting second one (called '"+ this.gameObject.name + "').");
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }

        }

        protected virtual void Start()
        {
            initialize();
        }

        public abstract void initialize();

        protected virtual void Update()
        {

            myHUDData = FPEInteractionManagerScript.Instance.GetHUDData();
            updateHUD();

        }

        protected abstract void updateHUD();

        #region GENERAL_PUBLIC_INTERFACE

        /// <summary>
        /// Called by FPEInteractionManager when interaction state changes.
        /// </summary>
        /// <param name="updatedState">The new interaction state</param>
        public abstract void InteractionStateChangeTo(FPEInteractionManagerScript.eInteractionState updatedState);

        #endregion

        #region NOTIFICATION_PUBLIC_INTERFACE

        /// <summary>
        /// Called by FPEInteractionManager when the player interacts with something that requires a notification (e.g. an Attached Note)
        /// </summary>
        /// <param name="message"></param>
        public abstract void ShowNotification(string message);

        #endregion

        #region AUDIO_DIARY_PUBLIC_INTERFACE

        /// <summary>
        /// Called by FPEInteractionManager when the player interacts with a new object that requires audio diary playback.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="isReplay"></param>
        public abstract void StartPlayingNewAudioDiary(string title, bool isReplay);

        /// <summary>
        /// Called by FPEInteractionManager when diary playback is stopped by some means (e.g. skip button, menu stop button, etc.)
        /// </summary>
        public abstract void StopPlayingAudioDiary();

        /// <summary>
        /// Called by FPEInteractionManager to reset audio diary UI to handle starting a new diary when an existing diary is already playing.
        /// </summary>
        public abstract void ResetAudioDiaryLabel();

        /// <summary>
        /// Called by FPEInteractionManager when player skips a diary and the audio fades out.
        /// </summary>
        public abstract void FadeAudioDiaryLabel();

        #endregion

        #region JOURNAL_PUBLIC_INTERFACE

        /// <summary>
        /// When called, must show journal UI elements. Called from FPEInteractionManager when player opens a journal to read it.
        /// </summary>
        public abstract void ShowJournalUI();

        /// <summary>
        /// Called by FPEInteractionManager to refresh active journal page image.
        /// </summary>
        /// <param name="journalPageSprite"></param>
        public abstract void SetJournalPage(Sprite journalPageSprite);

        /// <summary>
        /// When called, must hide journal UI elements. Called from FPEInteractionManager when player closes the journal they are reading.
        /// </summary>
        public abstract void HideJournalUI();

        /// <summary>
        /// Called by UI button to flip to the previous page. Relays that info to FPEInteractionManager to update its internal journal data/states.
        /// </summary>
        public abstract void PreviousJournalPage();

        /// <summary>
        /// Called by UI button to flip to the next page. Relays that info to FPEInteractionManager to update its internal journal data/states.
        /// </summary>
        public abstract void NextJournalPage();

        /// <summary>
        /// Called by UI button to close journal. Relays that info to FPEInteractionManager to update its internal journal data/states.
        /// </summary>
        public abstract void CloseJournal();

        #endregion

    }

}