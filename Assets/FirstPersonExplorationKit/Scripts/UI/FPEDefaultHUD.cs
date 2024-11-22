using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Whilefun.FPEKit
{

    //
    // FPEDefaultHUD
    // This class implements the default HUD seen in the demo, with basic representation of all functions in UI.
    //
    // Copyright 2018 While Fun Games
    // http://whilefun.com
    //
    public class FPEDefaultHUD : FPEHUD
    {

        [Header("General")]
        [Tooltip("Uncheck this to disabled HUD completely. If this is false, the values of the other checkboxes in this section will have no effect.")]
        public bool HUDEnabled = true;
        [Tooltip("Uncheck this to disable Reticle completely")]
        public bool reticleEnabled = true;
        [Tooltip("Uncheck this to disable Interaction Text completely")]
        public bool interactionTextEnabled = true;
        [Tooltip("Uncheck this to disable Control Hints completely")]
        public bool controlHintUIEnabled = true;
        [Tooltip("Uncheck this to disable Audio Diary labels and icon")]
        public bool audioDiaryUIEnabled = true;
        [Tooltip("Uncheck this to disable Notification label")]
        public bool notificationUIEnabled = true;

        [Header("Reticle")]
        [Tooltip("Reticle sprite when it is inactive")]
        public Sprite inactiveReticle;
        [Tooltip("Reticle sprite when it is active")]
        public Sprite activeReticle;
        [Tooltip("Reticle sprite when action is not permitted or unavailable")]
        public Sprite unavailableReticle;
        [Header("Custom Journal Cursor. Note: Only works when using Unity 5+")]
        public Texture2D journalCursor;

        // Reticle and Interaction Label (the text under the reticle)
        private RectTransform reticle;
        private RectTransform interactionLabel;
        private Vector3 interactionLabelTargetScale = Vector3.zero;
        private Vector3 interactionLabelLargestScale = Vector3.zero;
        private Vector3 interactionLabelSmallestScale = Vector3.zero;
        private float interactionLabelScaleChangeFactor = 24.0f;

        // Journal stuff
        private GameObject journalCloseIndicator;
        private GameObject journalPreviousButton;
        private GameObject journalNextButton;
        private GameObject journalBackground;
        private GameObject journalPage;

        //Audio diary stuff
        private RectTransform audioDiaryLabel;
        private Vector3 audioDiaryLabelTargetScale = Vector3.zero;
        private Vector3 audioDiaryLabelLargestScale = Vector3.zero;
        private Vector3 audioDiaryLabelSmallestScale = Vector3.zero;
        private RectTransform audioDiarySkipHintLabel;
        private bool audioDiaryIsPlaying = false;
        //private bool audioDiaryWasPlayingLastFrame = false;
        private bool fadingDiaryText = false;
        private Color defaultDiaryColor;
        private Color diaryFadeColor;

        // Notification Stuff
        private RectTransform notificationLabel;
        private Vector3 notificationLabelTargetScale = Vector3.zero;
        private Vector3 notificationLabelLargestScale = Vector3.zero;
        private Vector3 notificationLabelSmallestScale = Vector3.zero;
        private bool fadingNotificationText = true;
        private Color defaultNotificationColor;
        private Color notificationFadeColor;
        private float notificationCounter = 0.0f;
        private float notificationDuration = 2.5f;

        // Audio Diary stuff
        private float notificationColorLerpFactor = 1.8f;
        private float notificationScaleLerpFactor = 0.5f;

        // UI Hint options
        [Header("Control Hints UI")]
        [Tooltip("Text hint for Pick Up action")]
        public string mouseHintPickUpText = "Pick Up";
        [Tooltip("Text hint for Pick Up object when hands are full")]
        public string mouseHintPickUpHandsFullText = "(There's something already in your hand)";
        [Tooltip("Text hint for Put Back action")]
        public string mouseHintPutBackText = "Put Back";
        [Tooltip("Text hint for Examine action")]
        public string mouseHintExamineText = "Examine";
        [Tooltip("Text hint for Drop action")]
        public string mouseHintDropText = "Drop";
        [Tooltip("Text hint for Zoom In action")]
        public string mouseHintZoomText = "Zoom In";
        [Tooltip("Text hint for Activate action")]
        public string mouseHintActivateText = "Activate";
        [Tooltip("Text hint for telling player they need 2 hands to activate an object")]
        public string activateTwoHandsHint = "(You need both hands free for this)";
        [Tooltip("Text hint for Journal Read action")]
        public string mouseHintJournalText = "Read";
        [Tooltip("Text hint for telling player they need 2 hands to read a journal")]
        public string journalTwoHandsHint = "(You need both hands to read this)";
        [Tooltip("Text hint for Inventory 'Pre' action-from-world text (e.g. Take [x] ")]
        public string inventoryHintFromWorldPreText = "Take";
        [Tooltip("Text hint for Inventory 'Post' action-from-hand text (e.g. PUT [x] [postText]")]
        public string inventoryHintFromHandPreText = "Store";
        [Tooltip("Text hint for Inventory 'Post' action-from-hand text (e.g. [preText] [x] AWAY")]
        public string inventoryHintFromHandPostText = "In Inventory";

        [Tooltip("Text hint for Audio Diary manual playback")]
        public string audioDiaryHint = "Play";
        [Tooltip("UI label for prefix audio diary title (e.g. PLAYING [diaryTitle] [postText]")]
        public string audioDiaryPlayingPreText = "Playing";
        [Tooltip("UI label for prefix audio diary title replay from inventory (e.g. RE-PLAYING [diaryTitle] [postText]")]
        public string audioDiaryReplayPreText = "Re-playing";
        [Tooltip("UI label for postfix audio diary title (e.g. [preText] [diaryTitle] [postText]")]
        public string audioDiaryPlayingPostText = "";

        private FPEUIHint zoomExamineHint = null;
        private FPEUIHint interactHint = null;
        private FPEUIHint inventoryHint = null;
        private FPEUIHint unDockHint = null;

        private bool manuallyHidingInterface = false;

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
                interactionLabel = transform.Find("InteractionTextLabel").GetComponent<RectTransform>();

                if (!HUDEnabled && (reticleEnabled || interactionTextEnabled || controlHintUIEnabled || audioDiaryUIEnabled || notificationUIEnabled))
                {
                    Debug.LogWarning("FPEDefaultHUD:: HUD is disabled, but one or more other UI sections is enabled. HUD setting will disable all HUD elements regardless of individual setting.");
                }

                journalBackground = transform.Find("JournalBackground").gameObject;
                journalCloseIndicator = transform.Find("JournalBackground/CloseIndicator").gameObject;
                journalPreviousButton = transform.Find("JournalBackground/PreviousButton").gameObject;
                journalNextButton = transform.Find("JournalBackground/NextButton").gameObject;
                journalPage = transform.Find("JournalBackground/JournalPage").gameObject;
                audioDiaryLabel = transform.Find("AudioDiaryTitleLabel").GetComponent<RectTransform>();
                audioDiarySkipHintLabel = transform.Find("AudioDiarySkipHintLabel").GetComponent<RectTransform>();
                notificationLabel = transform.Find("NotificationLabel").GetComponent<RectTransform>();
                zoomExamineHint = transform.Find("ZoomExamineHint").GetComponent<FPEUIHint>();
                interactHint = transform.Find("InteractHint").GetComponent<FPEUIHint>();
                inventoryHint = transform.Find("InventoryHint").GetComponent<FPEUIHint>();
                unDockHint = transform.Find("UndockHint").GetComponent<FPEUIHint>();

                // UI component error checks
                if (!zoomExamineHint || !interactHint || !inventoryHint || !unDockHint)
                {
                    Debug.LogError("FPEDefaultHUD:: UI Hints are missing. Did you change the FPEInteractionManager prefab?");
                }

                // Just in case they were disabled during an edit to the prefab
                zoomExamineHint.gameObject.SetActive(true);
                interactHint.gameObject.SetActive(true);
                inventoryHint.gameObject.SetActive(true);
                unDockHint.gameObject.SetActive(true);

                // The undock hint is sort of special, as we don't necessarily set it like the others. So disable it here to start.
                unDockHint.setHint("");

                if (!reticle || !interactionLabel || !audioDiaryLabel || !audioDiarySkipHintLabel || !notificationLabel || !journalCloseIndicator || !journalPreviousButton || !journalNextButton || !journalBackground || !journalPage)
                {
                    Debug.LogError("FPEDefaultHUD:: UI and/or Journal Components are missing. Did you change the FPEInteractionManager prefab?");
                }

                if (!reticleEnabled)
                {
                    reticle.GetComponentInChildren<Image>().enabled = false;
                }

                if (!interactionTextEnabled)
                {
                    interactionLabel.GetComponentInChildren<Text>().enabled = false;
                }

                interactionLabelLargestScale = new Vector3(1.0f, 1.0f, 1.0f);
                interactionLabelSmallestScale = new Vector3(0.0f, 0.0f, 0.0f);

                audioDiaryLabelLargestScale = new Vector3(1.1f, 1.1f, 1.1f);
                audioDiaryLabelSmallestScale = new Vector3(0.9f, 0.9f, 0.9f);

                defaultDiaryColor = audioDiaryLabel.GetComponent<Text>().color;
                diaryFadeColor = audioDiaryLabel.GetComponent<Text>().color;
                diaryFadeColor.a = 0.0f;

                notificationLabelLargestScale = new Vector3(1.1f, 1.1f, 1.1f);
                notificationLabelSmallestScale = new Vector3(0.9f, 0.9f, 0.9f);

                defaultNotificationColor = notificationLabel.GetComponent<Text>().color;
                notificationFadeColor = notificationLabel.GetComponent<Text>().color;
                notificationFadeColor.a = 0.0f;

                setMouseHints("", "", "");
                StopPlayingAudioDiary();

            }

        }


        protected override void Update()
        {

            base.Update();

            // Animate the size of the interaction text //
            interactionLabel.localScale = Vector3.Lerp(interactionLabel.localScale, interactionLabelTargetScale, interactionLabelScaleChangeFactor * Time.deltaTime);

            #region AUDIO_DIARY

            // Animate audio diary title when visible //
            if (audioDiaryIsPlaying)
            {

                audioDiaryLabel.localScale = Vector3.Lerp(audioDiaryLabel.localScale, audioDiaryLabelTargetScale, notificationScaleLerpFactor * Time.deltaTime);

                if ((audioDiaryLabelTargetScale == audioDiaryLabelLargestScale) && (Vector3.Distance(audioDiaryLabel.localScale, audioDiaryLabelLargestScale) < 0.1f))
                {
                    audioDiaryLabelTargetScale = audioDiaryLabelSmallestScale;
                }
                else if ((audioDiaryLabelTargetScale == audioDiaryLabelSmallestScale) && (Vector3.Distance(audioDiaryLabel.localScale, audioDiaryLabelSmallestScale) < 0.1f))
                {
                    audioDiaryLabelTargetScale = audioDiaryLabelLargestScale;
                }

            }

            // Fade out diary text when done playing //
            if (fadingDiaryText)
            {

                audioDiaryLabel.GetComponent<Text>().color = Color.Lerp(audioDiaryLabel.GetComponent<Text>().color, diaryFadeColor, notificationColorLerpFactor * Time.deltaTime);

                if (audioDiaryLabel.GetComponent<Text>().color.a <= 0.1f)
                {
                    audioDiaryLabel.GetComponent<Text>().text = "";
                    audioDiaryLabel.GetComponent<Text>().color = defaultDiaryColor;
                    fadingDiaryText = false;
                }

            }

            #endregion

            #region NOTIFICATIONS

            // Make notification pulse a bit
            if (notificationCounter > 0.0f)
            {

                notificationLabel.localScale = Vector3.Lerp(notificationLabel.localScale, notificationLabelTargetScale, notificationScaleLerpFactor * Time.deltaTime);

                if ((notificationLabelTargetScale == notificationLabelLargestScale) && (Vector3.Distance(notificationLabel.localScale, notificationLabelLargestScale) < 0.1f))
                {
                    notificationLabelTargetScale = notificationLabelSmallestScale;
                }
                else if ((notificationLabelTargetScale == notificationLabelSmallestScale) && (Vector3.Distance(notificationLabel.localScale, notificationLabelSmallestScale) < 0.1f))
                {
                    notificationLabelTargetScale = notificationLabelLargestScale;
                }

                notificationCounter -= Time.deltaTime;

                if (notificationCounter <= 0.0f)
                {
                    fadingNotificationText = true;
                }

            }

            // Fade out our notification
            if (fadingNotificationText)
            {

                notificationLabel.GetComponent<Text>().color = Color.Lerp(notificationLabel.GetComponent<Text>().color, notificationFadeColor, notificationColorLerpFactor * Time.deltaTime);

                if (notificationLabel.GetComponent<Text>().color.a <= 0.1f)
                {
                    notificationLabel.GetComponent<Text>().text = "";
                    notificationLabel.GetComponent<Text>().color = defaultNotificationColor;
                    fadingNotificationText = false;
                }

            }

            #endregion

        }

        #region RETICLE_INTERACTION_LABEL

        private void activateReticle(string interactionString, string additionalHint = "")
        {

            if (reticleEnabled)
            {
                reticle.GetComponent<Image>().overrideSprite = ((additionalHint == "") ? activeReticle : unavailableReticle);
            }

            if (interactionTextEnabled)
            {

                interactionLabel.GetComponent<Text>().text = interactionString + ((additionalHint != "") ? "\n" + additionalHint : "");
                interactionLabelTargetScale = interactionLabelLargestScale;

            }

        }

        private void deactivateReticle()
        {

            if (reticleEnabled)
            {
                reticle.GetComponent<Image>().overrideSprite = inactiveReticle;
            }

            if (interactionTextEnabled)
            {

                interactionLabel.GetComponent<Text>().text = "";
                interactionLabelTargetScale = interactionLabelSmallestScale;

            }

        }

        private void hideReticleAndInteractionLabel()
        {

            if (reticleEnabled)
            {
                reticle.GetComponentInChildren<Image>().enabled = false;
            }

            if (interactionTextEnabled)
            {

                interactionLabel.GetComponentInChildren<Text>().enabled = false;
                interactionLabel.GetComponentInChildren<Text>().text = "";

            }

        }

        private void showReticleAndInteractionLabel()
        {

            if (reticleEnabled)
            {
                reticle.GetComponentInChildren<Image>().enabled = true;
            }

            if (interactionTextEnabled)
            {
                interactionLabel.GetComponentInChildren<Text>().enabled = true;
                interactionLabel.GetComponentInChildren<Text>().text = "";
            }

        }

        #endregion


        #region CONTROL_HINTS

        private void setMouseHints(string zoomExamineHintText, string interactHintText, string inventoryHintText)
        {

            if (controlHintUIEnabled)
            {

                zoomExamineHint.setHint(zoomExamineHintText);
                interactHint.setHint(interactHintText);
                inventoryHint.setHint(inventoryHintText);

            }
            else
            {

                zoomExamineHint.setHint("");
                interactHint.setHint("");
                inventoryHint.setHint("");

            }

        }

        private void setDockHint(string dockHintText)
        {

            if (controlHintUIEnabled)
            {
                unDockHint.setHint(dockHintText);
            }
            else
            {
                unDockHint.setHint("");
            }

        }

        #endregion

        protected override void updateHUD()
        {

            // Pre-conditions for HUD. Sometimes you want to just disable everything (e.g. when a transition is in progress, etc.)
            if (myHUDData.dockTransitionHappeningRightNow || manuallyHidingInterface || HUDEnabled == false)
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
                        activateReticle(myHUDData.lookedAtInteractionString);
                        setMouseHints(mouseHintZoomText, "", "");
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PICKUP:
                        activateReticle(myHUDData.lookedAtInteractionString);
                        setMouseHints(mouseHintZoomText, mouseHintPickUpText, "");
                        break;

                    case FPEInteractableBaseScript.eInteractionType.ACTIVATE:
                        activateReticle(myHUDData.lookedAtInteractionString);
                        setMouseHints(mouseHintZoomText, mouseHintActivateText, "");
                        break;

                    case FPEInteractableBaseScript.eInteractionType.JOURNAL:
                        activateReticle(myHUDData.lookedAtInteractionString);
                        setMouseHints(mouseHintZoomText, mouseHintJournalText, "");
                        break;

                    case FPEInteractableBaseScript.eInteractionType.AUDIODIARY:
                        if (myHUDData.lookedAtAudioDiaryAutoPlay == true)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintExamineText, "", "");
                        }
                        else
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintExamineText, audioDiaryHint, "");
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.INVENTORY:
                        activateReticle(myHUDData.lookedAtInventoryItemName);
                        if (myHUDData.lookedAtInventoryPickupPermitted == true)
                        {
                            setMouseHints(mouseHintZoomText, mouseHintPickUpText, inventoryHintFromWorldPreText + " " + myHUDData.lookedAtInventoryItemName);
                        }
                        else
                        {
                            setMouseHints(mouseHintZoomText, "", inventoryHintFromWorldPreText + " " + myHUDData.lookedAtInventoryItemName);
                        }

                        break;

                    case FPEInteractableBaseScript.eInteractionType.DOCK:
                        if (myHUDData.lookedAtDockOccupied == false)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintZoomText, myHUDData.lookedAtDockHint, "");
                        }
                        else
                        {
                            setMouseHints(mouseHintZoomText, "", "");
                        }
                        break;

                    // Holding Nothing, Looking at Nothing //
                    case FPEInteractableBaseScript.eInteractionType.PUT_BACK:
                    case FPEInteractableBaseScript.eInteractionType.NULL_TYPE:
                        deactivateReticle();
                        setMouseHints(mouseHintZoomText, "", "");
                        break;

                    default:
                        deactivateReticle();
                        setMouseHints(mouseHintZoomText, "", "");
                        Debug.LogError("FPEDefaultHUD:: Given bad eInteractionType '" + myHUDData.lookedAtType + "'.");
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
                        activateReticle(myHUDData.lookedAtInteractionString);
                        setMouseHints(mouseHintExamineText, mouseHintDropText, "");
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PICKUP:
                        activateReticle(myHUDData.lookedAtInteractionString, mouseHintPickUpHandsFullText);
                        setMouseHints(mouseHintExamineText, mouseHintDropText, "");
                        break;

                    case FPEInteractableBaseScript.eInteractionType.ACTIVATE:
                        if (myHUDData.lookedAtActivateAllowedWhenHoldingObject == true)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintExamineText, mouseHintActivateText, "");
                        }
                        else
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, activateTwoHandsHint);
                            setMouseHints(mouseHintExamineText, "", "");
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.JOURNAL:
                        activateReticle(myHUDData.lookedAtInteractionString, journalTwoHandsHint);
                        setMouseHints(mouseHintExamineText, "", "");
                        break;

                    case FPEInteractableBaseScript.eInteractionType.AUDIODIARY:
                        if (myHUDData.lookedAtAudioDiaryAutoPlay == true)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintExamineText, mouseHintDropText, "");
                        }
                        else
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintExamineText, audioDiaryHint, "");
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.INVENTORY:
                        // Special case: When holding something and looking at inventory, just show the name of the item, not "grab the item" since we can't pick it up right now.
                        activateReticle(myHUDData.lookedAtInventoryItemName);
                        setMouseHints(mouseHintExamineText, mouseHintDropText, inventoryHintFromWorldPreText + " " + myHUDData.lookedAtInventoryItemName);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.DOCK:
                        if (myHUDData.lookedAtDockOccupied == false)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintZoomText, myHUDData.lookedAtDockHint, "");
                        }
                        else
                        {
                            setMouseHints(mouseHintZoomText, "", "");
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PUT_BACK:
                        activateReticle(myHUDData.lookedAtPickupPutbackString);
                        setMouseHints(mouseHintExamineText, mouseHintPutBackText, "");
                        break;

                    // Holding PICKUP, looking at nothing //
                    case FPEInteractableBaseScript.eInteractionType.NULL_TYPE:
                        deactivateReticle();
                        setMouseHints(mouseHintExamineText, mouseHintDropText, "");
                        break;

                    default:
                        deactivateReticle();
                        setMouseHints(mouseHintExamineText, mouseHintDropText, "");
                        Debug.LogError("FPEDefaultHUD:: Given bad eInteractionType '" + myHUDData.lookedAtType + "'.");
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
                        activateReticle(myHUDData.lookedAtInteractionString);
                        setMouseHints(mouseHintExamineText, mouseHintDropText, inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PICKUP:
                        activateReticle(myHUDData.lookedAtInteractionString, mouseHintPickUpHandsFullText);
                        setMouseHints(mouseHintExamineText, mouseHintDropText, inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.ACTIVATE:

                        if (myHUDData.lookedAtActivateAllowedWhenHoldingObject == true)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintExamineText, mouseHintActivateText, inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        }
                        else
                        {
                            activateReticle(myHUDData.lookedAtInteractionString, activateTwoHandsHint);
                            setMouseHints(mouseHintExamineText, "", inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        }

                        break;

                    case FPEInteractableBaseScript.eInteractionType.JOURNAL:
                        activateReticle(myHUDData.lookedAtInteractionString, journalTwoHandsHint);
                        setMouseHints(mouseHintExamineText, "", inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.AUDIODIARY:

                        if (myHUDData.lookedAtAudioDiaryAutoPlay == true)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintExamineText, mouseHintDropText, inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        }
                        else
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintExamineText, audioDiaryHint, inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        }

                        break;

                    case FPEInteractableBaseScript.eInteractionType.INVENTORY:
                        // Special case: When inventory and looking at inventory, just show the name of the item, not "grab the item" since we can't pick it up right now.
                        activateReticle(myHUDData.lookedAtInventoryItemName);
                        setMouseHints(mouseHintExamineText, mouseHintDropText, inventoryHintFromWorldPreText + " " + myHUDData.lookedAtInventoryItemName);
                        break;

                    case FPEInteractableBaseScript.eInteractionType.DOCK:
                        if (myHUDData.lookedAtDockOccupied == false)
                        {
                            activateReticle(myHUDData.lookedAtInteractionString);
                            setMouseHints(mouseHintExamineText, myHUDData.lookedAtDockHint, inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        }
                        else
                        {
                            setMouseHints(mouseHintExamineText, "", inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        }
                        break;

                    case FPEInteractableBaseScript.eInteractionType.PUT_BACK:
                        if (myHUDData.usingCustomLookedAtInteractionString)
                        {
                            activateReticle(myHUDData.lookedAtPickupPutbackString);
                        }
                        else
                        {
                            activateReticle(mouseHintPutBackText + " " + myHUDData.lookedAtPickupPutbackString);
                        }
                        setMouseHints(mouseHintExamineText, mouseHintDropText, inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        break;

                    // Holding INVENTORY, looking at nothing //
                    case FPEInteractableBaseScript.eInteractionType.NULL_TYPE:
                        deactivateReticle();
                        setMouseHints(mouseHintExamineText, mouseHintDropText, inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        break;

                    default:
                        deactivateReticle();
                        setMouseHints(mouseHintExamineText, mouseHintDropText, inventoryHintFromHandPreText + " " + myHUDData.heldObjectInventoryItemName + " " + inventoryHintFromHandPostText);
                        Debug.LogError("FPEDefaultHUD:: Given bad eInteractionType '" + myHUDData.lookedAtType + "'.");
                        break;

                }

            }
            #endregion

            else
            {
                Debug.LogError("FPEDefaultHUD.updateHUD():: Player is holding object of type '" + myHUDData.heldType + "' which is not implemented.");
            }

            #endregion

            #region DOCK_ZOOM_EXAMINE

            setDockHint((myHUDData.dockedRightNow == true) ? myHUDData.currentUndockHint : "");

            // Special override cases
            if (myHUDData.examiningObject)
            {

                setMouseHints("", "", "");
                setDockHint("");
                reticle.GetComponentInChildren<Image>().enabled = false;

            }
            else if (myHUDData.zoomedIn)
            {

                setMouseHints("", "", "");
                setDockHint("");

            }

            #endregion

        }

       

        private void hideAllUI()
        {

            zoomExamineHint.setHintVisibility(false);
            interactHint.setHintVisibility(false);
            inventoryHint.setHintVisibility(false);
            unDockHint.setHintVisibility(false);
            hideReticleAndInteractionLabel();

        }

        private void unhideAllUI()
        {

            zoomExamineHint.setHintVisibility(true);
            interactHint.setHintVisibility(true);
            inventoryHint.setHintVisibility(true);
            unDockHint.setHintVisibility(true);
            showReticleAndInteractionLabel();

        }


        #region GENERAL_PUBLIC_INTERFACE

        /// <summary>
        /// Called by FPEInteractionManager when interaction state changes.
        /// </summary>
        /// <param name="updatedState">The new interaction state</param>
        public override void InteractionStateChangeTo(FPEInteractionManagerScript.eInteractionState updatedState)
        {

            if(updatedState == FPEInteractionManagerScript.eInteractionState.SUSPENDED)
            {
                hideAllUI();
                manuallyHidingInterface = true;
            }
            else
            {
                manuallyHidingInterface = false;
            }

        }

        #endregion

        #region NOTIFICATION_PUBLIC_INTERFACE

        /// <summary>
        /// Called by FPEInteractionManager when the player interacts with something that requires a notification (e.g. an Attached Note)
        /// </summary>
        /// <param name="message"></param>
        public override void ShowNotification(string message)
        {

            if (notificationUIEnabled)
            {

                notificationLabel.GetComponent<Text>().color = defaultDiaryColor;
                notificationLabel.GetComponent<Text>().text = message;
                notificationCounter = notificationDuration;
                notificationLabelTargetScale = notificationLabelLargestScale;
                fadingNotificationText = false;

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

            if (audioDiaryUIEnabled)
            {

                audioDiaryIsPlaying = true;
                audioDiaryLabel.GetComponent<Text>().color = defaultDiaryColor;

                if (isReplay)
                {
                    audioDiaryLabel.GetComponent<Text>().text = audioDiaryReplayPreText + " '" + title + "' " + audioDiaryPlayingPostText;
                }
                else
                {
                    audioDiaryLabel.GetComponent<Text>().text = audioDiaryPlayingPreText + " '" + title + "' " + audioDiaryPlayingPostText;
                }

                audioDiaryLabelTargetScale = audioDiaryLabelLargestScale;
                audioDiarySkipHintLabel.gameObject.SetActive(true);
                fadingDiaryText = false;

            }

        }

        /// <summary>
        /// Called by FPEInteractionManager when diary playback is stopped by some means (e.g. skip button, menu stop button, etc.)
        /// </summary>
        public override void StopPlayingAudioDiary()
        {

            audioDiaryLabel.GetComponent<Text>().text = "";
            audioDiaryLabel.GetComponent<Text>().color = defaultDiaryColor;
            audioDiarySkipHintLabel.gameObject.SetActive(false);

        }

        /// <summary>
        /// Called by FPEInteractionManager to reset audio diary UI to handle starting a new diary when an existing diary is already playing.
        /// </summary>
        public override void ResetAudioDiaryLabel()
        {

            audioDiaryLabel.GetComponent<Text>().text = "";
            audioDiaryLabel.GetComponent<Text>().color = defaultDiaryColor;

        }

        public override void FadeAudioDiaryLabel()
        {

            fadingDiaryText = true;
            audioDiaryIsPlaying = false;
            audioDiarySkipHintLabel.gameObject.SetActive(false);

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

            FPEEventSystem.Instance.gameObject.GetComponent<EventSystem>().SetSelectedGameObject(journalNextButton);

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