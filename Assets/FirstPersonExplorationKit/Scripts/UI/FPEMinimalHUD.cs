using UnityEngine;
using UnityEngine.UI;

namespace Whilefun.FPEKit
{

    //
    // FPEMinimalHUD
    // This class implements a sample "Minimalist" HUD with a reduced set of functionality 
    // represented on screen. Specifically, no text and no control hints.
    //
    // Copyright 2018 While Fun Games
    // http://whilefun.com
    //
    public class FPEMinimalHUD : FPEHUD
    {

        [Header("General")]
        [Tooltip("Uncheck this to disabled HUD completely. If this is false, the values of the other checkboxes in this section will have no effect.")]
        public bool HUDEnabled = true;

        [Header("Reticle")]
        [Tooltip("Reticle sprite when it is inactive")]
        public Sprite inactiveReticle;
        [Tooltip("Reticle sprite when it is active")]
        public Sprite activeReticle;
        [Tooltip("Reticle sprite when action is not permitted or unavailable")]
        public Sprite unavailableReticle;
        [Header("Reticle interaction icons")]
        public Sprite interactionIconActivate;
        public Sprite interactionIconActivateTwoHands;
        public Sprite interactionIconPickup;
        public Sprite interactionIconHandsFull;
        public Sprite interactionIconJournal;
        public Sprite interactionIconJournalTwoHands;
        public Sprite interactionIconPutBack;
        public Sprite interactionIconAudioDiary;
        public Sprite interactionIconInventory;
        public Sprite interactionIconStatic;
        public Sprite interactionIconDock;

        // Reticle and Interaction Label (the text under the reticle)
        private RectTransform reticle;
        private RectTransform interactionIcon;

        // Journal stuff
        private GameObject journalCloseIndicator;
        private GameObject journalPreviousButton;
        private GameObject journalNextButton;
        private GameObject journalBackground;
        private GameObject journalPage;

        // Audio Diary stuff
        private RectTransform audioDiaryIcon;
        private bool audioDiaryIsPlaying = false;
        private bool fadingDiaryText = false;
        private Vector3 audioReelRotationPerSecond = new Vector3(0.0f, 0.0f, -120.0f);

        // Notification Stuff
        private RectTransform notificationIcon;
        private float notificationScaleLerpFactor = 5.0f;
        private Vector3 notificationIconTargetScale = Vector3.zero;
        private Vector3 notificationIconLargestScale = Vector3.zero;
        private Vector3 notificationIconSmallestScale = Vector3.zero;
        private bool fadingNotificationText = true;
        private Color defaultNotificationColor;
        private Color notificationFadeColor;
        private float notificationCounter = 0.0f;
        private float notificationDuration = 2.5f;
        

        protected override void Start()
        {
            base.Start();
        }

        public override void initialize()
        {

            if (!initialized)
            {

                initialized = true;

                myHUDData = new FPEHUDData();

                reticle = transform.Find("Reticle").GetComponent<RectTransform>();
                interactionIcon = transform.Find("InteractionIcon").GetComponent<RectTransform>();

                journalBackground = transform.Find("JournalBackground").gameObject;
                journalCloseIndicator = transform.Find("JournalBackground/CloseIndicator").gameObject;
                journalPreviousButton = transform.Find("JournalBackground/PreviousButton").gameObject;
                journalNextButton = transform.Find("JournalBackground/NextButton").gameObject;
                journalPage = transform.Find("JournalBackground/JournalPage").gameObject;

                audioDiaryIcon = transform.Find("AudioDiaryIcon").GetComponent<RectTransform>();
                notificationIcon = transform.Find("NotificationIcon").GetComponent<RectTransform>();

                if (!reticle || !interactionIcon || !audioDiaryIcon || !notificationIcon || !journalCloseIndicator || !journalPreviousButton || !journalNextButton || !journalBackground || !journalPage)
                {
                    Debug.LogError("FPEInteractionManagerScript:: UI and/or Journal Components are missing. Did you change the FPEInteractionManager prefab?");
                }

                notificationIconLargestScale = new Vector3(1.1f, 1.1f, 1.1f);
                notificationIconSmallestScale = new Vector3(0.9f, 0.9f, 0.9f);

                StopPlayingAudioDiary();

            }

        }


        protected override void Update()
        {

            base.Update();

            #region AUDIO_DIARY

            // Animate audio diary reel by making it spin a little //
            if (audioDiaryIsPlaying)
            {
                audioDiaryIcon.Rotate(audioReelRotationPerSecond * Time.deltaTime);
            }

            if (fadingDiaryText)
            {
                audioDiaryIcon.gameObject.SetActive(false);
            }

            #endregion

            #region NOTIFICATIONS

            // Make notification pulse a bit
            if (notificationCounter > 0.0f)
            {

                notificationIcon.localScale = Vector3.Lerp(notificationIcon.localScale, notificationIconTargetScale, notificationScaleLerpFactor * Time.deltaTime);

                if ((notificationIconTargetScale == notificationIconLargestScale) && (Vector3.Distance(notificationIcon.localScale, notificationIconLargestScale) < 0.1f))
                {
                    notificationIconTargetScale = notificationIconSmallestScale;
                }
                else if ((notificationIconTargetScale == notificationIconSmallestScale) && (Vector3.Distance(notificationIcon.localScale, notificationIconSmallestScale) < 0.1f))
                {
                    notificationIconTargetScale = notificationIconLargestScale;
                }

                notificationCounter -= Time.deltaTime;

                if (notificationCounter <= 0.0f)
                {
                    fadingNotificationText = true;
                }

            }

            if (fadingNotificationText)
            {

                notificationIcon.gameObject.SetActive(false);
                fadingNotificationText = false;

            }

            #endregion

        }

        #region RETICLE_INTERACTION_LABEL

        private void activateReticle(string interactionString, Sprite icon)
        {

            if (HUDEnabled)
            {
                reticle.GetComponent<Image>().overrideSprite = activeReticle;
                interactionIcon.GetComponent<Image>().overrideSprite = icon;
                interactionIcon.GetComponent<Image>().enabled = true;
            }

        }

        private void deactivateReticle()
        {

            if (HUDEnabled)
            {
                reticle.GetComponent<Image>().overrideSprite = inactiveReticle;
                interactionIcon.GetComponent<Image>().enabled = false;
            }

        }

        private void hideReticleAndInteractionLabel()
        {

            reticle.GetComponentInChildren<Image>().enabled = false;
            interactionIcon.GetComponent<Image>().enabled = false;

        }

        private void showReticleAndInteractionLabel()
        {

            if (HUDEnabled)
            {
                reticle.GetComponentInChildren<Image>().enabled = true;
                interactionIcon.GetComponent<Image>().overrideSprite = null;
                interactionIcon.GetComponent<Image>().enabled = true;
            }

        }

        #endregion



        protected override void updateHUD()
        {

            // Pre-conditions for HUD. Sometimes you want to just disable everything (e.g. when a transition is in progress, etc.)
            if (myHUDData.dockTransitionHappeningRightNow || HUDEnabled == false)
            {
                hideAllUI();
            }
            else
            {
                unhideAllUI();
            }

            #region MOUSE_HINTS

            #region HINT_HOLDING_NOTHING
            if (myHUDData.heldType == FPEInteractableBaseScript.eInteractionType.NULL_TYPE)
            {

                switch (myHUDData.lookedAtType)
                {

                    case FPEInteractableBaseScript.eInteractionType.STATIC:
                        activateReticle(myHUDData.lookedAtInteractionString, interactionIconStatic);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PICKUP:
                        activateReticle(myHUDData.lookedAtInteractionString, interactionIconPickup);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.ACTIVATE:
                        activateReticle(myHUDData.lookedAtInteractionString, interactionIconActivate);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.JOURNAL:
                        activateReticle(myHUDData.lookedAtInteractionString, interactionIconJournal);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.AUDIODIARY:
                        if (myHUDData.lookedAtAudioDiaryAutoPlay == true)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconAudioDiary);
                        }
                        else
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconAudioDiary);
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.INVENTORY:
                        activateReticle(myHUDData.lookedAtInventoryItemName, interactionIconInventory);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.DOCK:
                        if (myHUDData.lookedAtDockOccupied == false)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconDock);
                        }
                        break;

                    // Holding Nothing, Looking at Nothing //
                    case FPEInteractableBaseScript.eInteractionType.PUT_BACK:
                    case FPEInteractableBaseScript.eInteractionType.NULL_TYPE:
                        deactivateReticle();
                        break;

                    default:
                        deactivateReticle();
                        Debug.LogError("FPEInteractionManagerScript:: Given bad eInteractionType '" + myHUDData.lookedAtType + "'.");
                        break;

                }

            }
            #endregion

            #region HINT_HOLDING_PICKUP
            else if (myHUDData.heldType == FPEInteractableBaseScript.eInteractionType.PICKUP)
            {

                switch (myHUDData.lookedAtType)
                {

                    case FPEInteractableBaseScript.eInteractionType.STATIC:
                        activateReticle(myHUDData.lookedAtInteractionString, interactionIconStatic);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PICKUP:
                        activateReticle(myHUDData.lookedAtInteractionString, interactionIconPickup);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.ACTIVATE:

                        if (myHUDData.lookedAtActivateAllowedWhenHoldingObject == true)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconActivate);
                        }
                        else
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconActivateTwoHands);
                        }

                        break;

                    case FPEInteractableBaseScript.eInteractionType.JOURNAL:
                        activateReticle(myHUDData.lookedAtInteractionString, interactionIconJournal);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.AUDIODIARY:

                        if (myHUDData.lookedAtAudioDiaryAutoPlay == true)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconAudioDiary);
                        }
                        else
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconAudioDiary);
                        }

                        break;

                    case FPEInteractableBaseScript.eInteractionType.INVENTORY:
                        // Special case: When holding something and looking at inventory, just show the name of the item, not "grab the item" since we can't pick it up right now.
                        activateReticle(myHUDData.lookedAtInventoryItemName, interactionIconInventory);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.DOCK:
                        if (myHUDData.lookedAtDockOccupied == false)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconDock);
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PUT_BACK:
                        activateReticle(myHUDData.lookedAtPickupPutbackString, interactionIconPutBack);
                        break;

                    // Holding PICKUP, looking at nothing //
                    case FPEInteractableBaseScript.eInteractionType.NULL_TYPE:
                        deactivateReticle();
                        break;

                    default:
                        deactivateReticle();
                        Debug.LogError("FPEInteractionManagerScript:: Given bad eInteractionType '" + myHUDData.lookedAtType + "'.");
                        break;

                }

            }
            #endregion

            #region HINT_HOLDING_INVENTORY
            else if (myHUDData.heldType == FPEInteractableBaseScript.eInteractionType.INVENTORY)
            {

                switch (myHUDData.lookedAtType)
                {

                    case FPEInteractableBaseScript.eInteractionType.STATIC:
                        activateReticle(myHUDData.lookedAtInteractionString, interactionIconStatic);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PICKUP:
                        activateReticle(myHUDData.lookedAtInteractionString, interactionIconHandsFull);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.ACTIVATE:

                        if (myHUDData.lookedAtActivateAllowedWhenHoldingObject == true)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconActivate);
                        }
                        else
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconActivateTwoHands);
                        }

                        break;

                    case FPEInteractableBaseScript.eInteractionType.JOURNAL:
                        activateReticle(myHUDData.lookedAtInteractionString, interactionIconJournal);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.AUDIODIARY:
                        if (myHUDData.lookedAtAudioDiaryAutoPlay == true)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconAudioDiary);
                        }
                        else
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconAudioDiary);
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.INVENTORY:
                        // Special case: When inventory and looking at inventory, just show the name of the item, not "grab the item" since we can't pick it up right now.
                        activateReticle(myHUDData.lookedAtInventoryItemName, interactionIconInventory);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.DOCK:
                        if (myHUDData.lookedAtDockOccupied == false)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, interactionIconDock);
                        }
                        else
                        {
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PUT_BACK:
                        break;

                    // Holding INVENTORY, looking at nothing //
                    case FPEInteractableBaseScript.eInteractionType.NULL_TYPE:
                        deactivateReticle();
                        break;

                    default:
                        deactivateReticle();
                        Debug.LogError("FPEInteractionManagerScript:: Given bad eInteractionType '" + myHUDData.lookedAtType + "'.");
                        break;

                }

            }
            #endregion

            else
            {
                Debug.LogError("FPEInteractionManagerScript.updateControlHints():: Player is holding object of type '" + myHUDData.heldType + "' which is not implemented.");
            }

            #endregion

        }



        private void hideAllUI()
        {
            hideReticleAndInteractionLabel();
        }

        private void unhideAllUI()
        {
            showReticleAndInteractionLabel();
        }

        #region GENERAL_PUBLIC_INTERFACE

        /// <summary>
        /// Called by FPEInteractionManager when interaction state changes.
        /// </summary>
        /// <param name="updatedState">The new interaction state</param>
        public override void InteractionStateChangeTo(FPEInteractionManagerScript.eInteractionState updatedState)
        {
            // Minimal HUD doesn't need to deal with this for now
        }

        #endregion

        #region NOTIFICATION_PUBLIC_INTERFACE

        /// <summary>
        /// Called by FPEInteractionManager when the player interacts with something that requires a notification (e.g. an Attached Note)
        /// </summary>
        /// <param name="message"></param>
        public override void ShowNotification(string message)
        {

            if (HUDEnabled)
            {
                notificationIcon.gameObject.SetActive(true);
                notificationCounter = notificationDuration;
                notificationIconTargetScale = notificationIconLargestScale;
            }

        }

        #endregion

        #region AUDIO_DIARY_PUBLIC_INTERFACE

        /// <summary>
        /// Called by FPEInteractionManager when the player interacts with a new object that requires audio diary playback.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="isReplay"></param>
        public override void StartPlayingNewAudioDiary(string title, bool isReplay)
        {

            if (HUDEnabled)
            {
                audioDiaryIsPlaying = true;
                audioDiaryIcon.gameObject.SetActive(true);
                fadingDiaryText = false;
            }

        }

        /// <summary>
        /// Called by FPEInteractionManager when diary playback is stopped by some means (e.g. skip button, menu stop button, etc.)
        /// </summary>
        public override void StopPlayingAudioDiary()
        {
            audioDiaryIcon.gameObject.SetActive(false);
        }

        /// <summary>
        /// Called by FPEInteractionManager to reset audio diary UI to handle starting a new diary when an existing diary is already playing.
        /// </summary>
        public override void ResetAudioDiaryLabel()
        {
            // In this case, we don't have anything to do, since we don't have any text to reset.
        }

        public override void FadeAudioDiaryLabel()
        {

            fadingDiaryText = true;
            audioDiaryIsPlaying = false;
            audioDiaryIcon.gameObject.SetActive(false);

        }

        #endregion

        #region JOURNAL_PUBLIC_INTERFACE

        /// <summary>
        /// When called, must show journal UI elements. Called from FPEInteractionManager when player opens a journal to read it.
        /// </summary>
        public override void ShowJournalUI()
        {

            journalCloseIndicator.SetActive(true);
            journalPreviousButton.SetActive(true);
            journalNextButton.SetActive(true);
            journalBackground.SetActive(true);
            journalPage.SetActive(true);

        }

        /// <summary>
        /// Called by FPEInteractionManager to refresh active journal page image.
        /// </summary>
        /// <param name="journalPageSprite"></param>
        public override void SetJournalPage(Sprite journalPageSprite)
        {
            journalPage.transform.gameObject.GetComponentInChildren<Image>().overrideSprite = journalPageSprite;
        }

        /// <summary>
        /// When called, must hide journal UI elements. Called from FPEInteractionManager when player closes the journal they are reading.
        /// </summary>
        public override void HideJournalUI()
        {

            journalCloseIndicator.SetActive(false);
            journalPreviousButton.SetActive(false);
            journalNextButton.SetActive(false);
            journalBackground.SetActive(false);
            journalPage.SetActive(false);

        }

        /// <summary>
        /// Called by UI button to flip to the previous page. Relays that info to FPEInteractionManager to update its internal journal data/states.
        /// </summary>
        public override void PreviousJournalPage()
        {
            FPEInteractionManagerScript.Instance.previousJournalPage();
        }

        /// <summary>
        /// Called by UI button to flip to the next page. Relays that info to FPEInteractionManager to update its internal journal data/states.
        /// </summary>
        public override void NextJournalPage()
        {
            FPEInteractionManagerScript.Instance.nextJournalPage();
        }

        /// <summary>
        /// Called by UI button to close journal. Relays that info to FPEInteractionManager to update its internal journal data/states.
        /// </summary>
        public override void CloseJournal()
        {
            FPEInteractionManagerScript.Instance.closeJournal();
        }

        #endregion

    }

}