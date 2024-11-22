using UnityEngine;

using UnityEngine.UI;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPEGameMenu
    // This script contains all logic to manage core game menu interactions.
    //
    // Note: When editing the canvas in the editor, you can simply disable 
    // the MenuCanvas game object before saving your scene. This will keep 
    // it out of the way when editing other scene objects.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEGameMenu : FPEMenu
    {

        public enum eMenuTab
        {
            ITEMS = 0,
            AUDIO_DIARIES = 1,
            NOTES = 2,
            SYSTEM = 3
        }
        private eMenuTab currentMenuTab = eMenuTab.ITEMS;

        [Header("Menu Audio")]
        [SerializeField]
        private AudioClip menuOpen = null;
        [SerializeField]
        private AudioClip menuClose = null;
        [SerializeField]
        private AudioClip menuSelect = null;
        [SerializeField]
        private AudioClip menuTabSelect = null;
        [SerializeField]
        private AudioClip menuBack = null;
        [SerializeField]
        private AudioClip menuError = null;
        [SerializeField]
        private AudioClip menuDiaryPlayback = null;
        [SerializeField]
        private AudioClip menuDiaryStop = null;
        [SerializeField]
        private AudioClip menuPageTurn = null;
        //[SerializeField]
        //private AudioClip menuGenericButton = null;

        private AudioSource menuAudio = null;
        private GameObject menuCanvas = null;

        // Item Data
        private FPEInventoryItemData[] itemData = null;
        private FPEAudioDiaryData[] audioDiaryData = null;
        private FPENoteData[] noteData = null;

        // Our menu tabs - will need to be updated if you add or remove tabs
        private FPEMenuTab itemsTab = null;
        private FPEMenuTab audioDiariesTab = null;
        private FPEMenuTab notesTab = null;
        private FPEMenuTab systemTab = null;
        // Our menu tab panels - will need to be updated if you add or remove tabs
        private GameObject inventoryItemsListPanel = null;
        private GameObject inventoryItemInfoPanelParent = null;
        private FPEInventoryItemInfoPanel itemInfoPanel = null;
        private GameObject audioDiariesPanel = null;
        private GameObject notesPanel = null;
        private GameObject noteContentsPanelParent = null;
        private FPENoteContentsPanel noteContentsPanel = null;
        private GameObject systemPanel = null;
        private GameObject exitConfirmationPanel = null;
        private GameObject actionsPanelParent = null;
        private FPEInventoryActionsPanel inventoryActionsPanel = null;
        private GameObject pageControlPanel = null;

        // Panels and slots of different types //
        private FPEInventoryItemSlot[] inventorySlots = null;
        private FPEAudioDiaryEntrySlot[] audioDiarySlots = null;
        private FPENoteEntrySlot[] noteSlots = null;
        private FPEMenuButton[] systemButtons = null;
        private FPEMenuButton[] exitConfirmationButtons = null;

        // Page selection buttons and gamepad hints
        private GameObject previousPageButton = null;
        private GameObject nextPageButton = null;
        private GameObject previousPageHint = null;
        private GameObject nextPageHint = null;
        private Text pageIndicatorText = null;

        // Remember selections between menu uses
        private eMenuTab previouslySelectedTab = eMenuTab.ITEMS;
        private FPEInventoryItemSlot previouslySelectedItemSlot = null;
        private int slotActionItemIndex = -1;
        private FPEAudioDiaryEntrySlot previouslySelectedAudioDiarySlot = null;
        private int previouslySelectedNoteSlotIndex = 0;

        private int[] itemsPerPage = null;
        private int[] previouslySelectedPage = null;

        // System Stuff
        [SerializeField, Tooltip("Minimum allowed mouse sensitivity value.")]
        private float minSensitivity = 0.5f;
        [SerializeField, Tooltip("Maximum allowed mouse sensitivity value")]
        private float maxSensitivity = 32.0f;
        private Text mouseSensitivityValueText = null;
        private FPEMenuToggle lookSmoothingToggle = null;
        private FPEMenuToggle useGamepadToggle = null;
        private FPEMenuToggle flipMouseYAxisToggle = null;
        private FPEMenuToggle flipGamepadYAxisToggle = null;
        private FPEMenuButton loadGameButton = null;

        // Visual feedback when errors occur, etc.
        [SerializeField, Tooltip("If true, elements such as tabs will jiggle when an error occurs. For example, when player tries to move left from the leftmost tab.")]
        private bool shakeUIOnError = false;
        private RectTransform elementToJiggle = null;
        private Vector3 originalElementPosition = Vector3.zero;
        private float jiggleDuration = 0.1f;
        private float jiggleDistance = 2.0f;
        private float jiggleTimer = 0.0f;
        private float jiggleOffset = 0.0f;


        public override void Awake()
        {

            base.Awake();

            menuCanvas = transform.Find("MenuCanvas").gameObject;
            menuCanvas.SetActive(false);
            itemData = new FPEInventoryItemData[1];
            audioDiaryData = new FPEAudioDiaryData[1];
            noteData = new FPENoteData[1];

            // Find tabs - will need to be updated if you add or remove tabs
            FPEMenuTab[] menuTabs = menuCanvas.gameObject.GetComponentsInChildren<FPEMenuTab>();

            for (int t = 0; t < menuTabs.Length; t++)
            {

                if (menuTabs[t].transform.name == "ItemsTab")
                {
                    itemsTab = menuTabs[t];
                }
                else if (menuTabs[t].transform.name == "AudioDiariesTab")
                {
                    audioDiariesTab = menuTabs[t];
                }
                else if (menuTabs[t].transform.name == "NotesTab")
                {
                    notesTab = menuTabs[t];
                }
                else if (menuTabs[t].transform.name == "SystemTab")
                {
                    systemTab = menuTabs[t];
                }

            }

            if (!itemsTab || !audioDiariesTab || !notesTab || !systemTab)
            {
                Debug.LogError("FPEGameMenu:: Cannot find one or more of the menu tabs! Did you rename or remove them?");
            }

            // Find panels  - will need to be updated if you add or remove tabs
            inventoryItemInfoPanelParent = menuCanvas.gameObject.transform.Find("InventoryItemInfoPanel").gameObject;
            inventoryItemsListPanel = menuCanvas.gameObject.transform.Find("InventoryListPanel").gameObject;
            audioDiariesPanel = menuCanvas.gameObject.transform.Find("AudioDiariesPanel").gameObject;
            notesPanel = menuCanvas.gameObject.transform.Find("NotesPanel").gameObject;
            noteContentsPanelParent = menuCanvas.gameObject.transform.Find("NoteContentsPanel").gameObject;
            systemPanel = menuCanvas.gameObject.transform.Find("SystemPanel").gameObject;
            exitConfirmationPanel = menuCanvas.gameObject.transform.Find("ExitConfirmationPanel").gameObject;
            actionsPanelParent = menuCanvas.gameObject.transform.Find("InventoryActionsPanel").gameObject;
            pageControlPanel = menuCanvas.gameObject.transform.Find("PageControlsPanel").gameObject;

            if (!inventoryItemInfoPanelParent || !inventoryItemsListPanel || !audioDiariesPanel || !notesPanel || !noteContentsPanelParent || !systemPanel || !actionsPanelParent || !pageControlPanel)
            {
                Debug.LogError("FPEGameMenu:: Cannot find one or more of the menu panels! Did you rename or remove them?");
            }

            inventoryItemInfoPanelParent.SetActive(true);
            itemInfoPanel = inventoryItemInfoPanelParent.GetComponent<FPEInventoryItemInfoPanel>();

            if (itemInfoPanel == null)
            {
                Debug.LogError("FPEGameMenu:: InventoryItemInfoPanel is missing its 'FPEInventoryItemInfoPanel' (script) component! Did you remove it?");
            }

            actionsPanelParent.SetActive(true);
            inventoryActionsPanel = actionsPanelParent.GetComponent<FPEInventoryActionsPanel>();

            if (inventoryActionsPanel == null)
            {
                Debug.LogError("FPEGameMenu:: InventoryActionsPanel is missing its 'FPEInventoryActionsPanel' (script) component! Did you remove it?");
            }

            // Inventory item slots (We should have more than one of these)
            inventoryItemsListPanel.SetActive(true);
            inventorySlots = menuCanvas.gameObject.GetComponentsInChildren<FPEInventoryItemSlot>();

            if (inventorySlots == null || inventorySlots.Length < 2)
            {
                Debug.LogError("FPEGameMenu:: There are 1 or fewer inventory item slots on the inventory item panel! Things will break.");
            }

            // Audio diary slots (we should have more than one of these as well)
            audioDiariesPanel.SetActive(true);
            audioDiarySlots = menuCanvas.gameObject.GetComponentsInChildren<FPEAudioDiaryEntrySlot>();

            if (audioDiarySlots == null || audioDiarySlots.Length < 2)
            {
                Debug.LogError("FPEGameMenu:: There are 1 or fewer audio diary slots on the audio diary panel! Things will break.");
            }

            // Note slots (we should have more than one of these as well)
            notesPanel.SetActive(true);
            noteSlots = menuCanvas.gameObject.GetComponentsInChildren<FPENoteEntrySlot>();

            if(noteSlots == null || noteSlots.Length < 2)
            {
                Debug.LogError("FPEGameMenu:: There are 1 or fewer note slots on the notes panel! Things will break.");
            }

            noteContentsPanelParent.SetActive(true);
            noteContentsPanel = noteContentsPanelParent.GetComponent<FPENoteContentsPanel>();

            if (noteContentsPanel == null)
            {
                Debug.LogError("FPEGameMenu:: noteContentsPanel is missing its 'FPENoteContentsPanel' (script) component! Did you remove it?");
            }

            // Find all the system buttons
            systemPanel.SetActive(true);
            systemButtons = systemPanel.gameObject.GetComponentsInChildren<FPEMenuButton>();
            exitConfirmationPanel.SetActive(true);
            exitConfirmationButtons = exitConfirmationPanel.gameObject.GetComponentsInChildren<FPEMenuButton>();
            mouseSensitivityValueText = systemPanel.gameObject.transform.Find("MouseSensitivityValue").GetComponent<Text>();
            lookSmoothingToggle = systemPanel.gameObject.transform.Find("LookSmoothingToggle").GetComponent<FPEMenuToggle>();
            useGamepadToggle = systemPanel.gameObject.transform.Find("UseGamepadToggle").GetComponent<FPEMenuToggle>();
            flipMouseYAxisToggle = systemPanel.gameObject.transform.Find("FlipMouseYAxisToggle").GetComponent<FPEMenuToggle>();
            flipGamepadYAxisToggle = systemPanel.gameObject.transform.Find("FlipGamepadYAxisToggle").GetComponent<FPEMenuToggle>();
            loadGameButton = systemPanel.gameObject.transform.Find("LoadGameButton").GetComponent<FPEMenuButton>();

            if (!mouseSensitivityValueText || !lookSmoothingToggle || !useGamepadToggle || !flipMouseYAxisToggle || !flipGamepadYAxisToggle || !loadGameButton)
            {
                Debug.LogError("FPEGameMenu:: mouseSensitivityValue Text, options toggles, or load game button component(s) are missing! Did you remove them from the prefab?");
            }

            // Find Previous/Next page buttons and hints
            pageControlPanel.SetActive(true);
            previousPageButton = pageControlPanel.transform.Find("PreviousPageButton").gameObject;
            nextPageButton = pageControlPanel.transform.Find("NextPageButton").gameObject;
            previousPageHint = pageControlPanel.transform.Find("RightStickHintLeft").gameObject;
            nextPageHint = pageControlPanel.transform.Find("RightStickHintRight").gameObject;
            pageIndicatorText = pageControlPanel.transform.Find("PageIndicatorText").gameObject.GetComponent<Text>();

            if (!previousPageButton || !nextPageButton || !previousPageHint || !nextPageHint || !pageIndicatorText)
            {
                Debug.LogError("FPEGameMenu:: Page changing buttons, hints, or text are missing from InventoryListPanel! Did you remove them?");
            }

            // items per page and previous page selections for each tab
            itemsPerPage = new int[Enum.GetNames(typeof(eMenuTab)).Length];
            itemsPerPage[(int)eMenuTab.ITEMS] = inventorySlots.Length;
            itemsPerPage[(int)eMenuTab.AUDIO_DIARIES] = audioDiarySlots.Length;
            itemsPerPage[(int)eMenuTab.NOTES] = noteSlots.Length;

            previouslySelectedPage = new int[Enum.GetNames(typeof(eMenuTab)).Length];
            previouslySelectedPage[(int)eMenuTab.ITEMS] = 0;
            previouslySelectedPage[(int)eMenuTab.AUDIO_DIARIES] = 0;

            previouslySelectedTab = eMenuTab.ITEMS;
            previouslySelectedItemSlot = inventorySlots[0];
            previouslySelectedAudioDiarySlot = audioDiarySlots[0];
            previouslySelectedNoteSlotIndex = -1;

        }

        public override void Start()
        {

            base.Start();

            menuAudio = gameObject.GetComponent<AudioSource>();

            if (!menuAudio)
            {
                Debug.LogError("FPEGameMenu:: Cannot find attached AudioSource Component! Menu Audio will not work.");
            }

        }

        public override void Update()
        {

            base.Update();

            // We wrap menu input checks in menuActive because we don't want menu 
            // actions and buttons to take place when the menu is closed :)
            if (menuActive)
            {

                // Allow player to switch tabs without clicking them
                if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_MENU_PREVIOUS_TAB))
                {
                    selectPreviousMenuTab();
                }
                if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_MENU_NEXT_TAB))
                {
                    selectNextMenuTab();
                }

                // Allow player to switch inventory pages without clicking them
                if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_MENU_PREVIOUS_PAGE))
                {
                    moveToPreviousPage();
                }
                if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_MENU_NEXT_PAGE))
                {
                    moveToNextPage();
                }

                // Allow player to close actions panel without having to select and "click" the cancel button (e.g. gamepad)
                if (actionsPanelParent.activeSelf)
                {
                    if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_CLOSE))
                    {
                        hideActionsForItem();
                    }
                }

                // Allow player to stop all diaries wihtout having to select and "click" stop button (e.g. gamepad)
                if (audioDiariesPanel.activeSelf)
                {
                    if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_CLOSE))
                    {
                        stopAllDiaries();
                    }
                }

                if (elementToJiggle)
                {

                    jiggleTimer -= Time.unscaledDeltaTime;

                    if (jiggleTimer <= 0.0f)
                    {

                        elementToJiggle.localPosition = originalElementPosition;
                        elementToJiggle = null;

                    }
                    else
                    {

                        Vector3 updatedPosition = elementToJiggle.GetComponent<RectTransform>().localPosition;
                        jiggleOffset *= jiggleTimer / jiggleDuration;
                        updatedPosition.x += jiggleOffset;
                        elementToJiggle.GetComponent<RectTransform>().localPosition = updatedPosition;

                    }

                }

            }

        }

        public override void activateMenu()
        {

            if (!menuActive)
            {

                menuAudio.clip = menuOpen;
                menuAudio.Play();
                menuCanvas.SetActive(true);

                // Note: Doing all of these every time seemed wasteful. Though if your specific menu slows down for a couple frames when you refresh data views, this is a good place to conceal that slowdown.
                // refreshItemSlotsAndDetailsView();
                // refreshAudioDiarySlotsView();
                // refreshNoteSlotsView();

                // Restore previous selections, or switch to defaults if first time using menu
                restoreTabSelection(previouslySelectedTab);
                menuActive = true;

            }

        }

        public override void deactivateMenu()
        {

            if (menuActive)
            {

                menuAudio.clip = menuClose;
                menuAudio.Play();
                // Just in case player hits close when they are in the middle of an action
                hideActionsForItem();
                menuCanvas.SetActive(false);
                // Save selections for next time
                previouslySelectedTab = currentMenuTab;
                saveSelectedItemSlot();
                menuActive = false;

                if (FPESaveLoadManager.Instance != null)
                {
                    FPESaveLoadManager.Instance.SaveGameOptions();
                }

            }

        }
        
        public void changeMenuTabTo(int tab)
        {

            switch (tab)
            {

                case (int)eMenuTab.ITEMS:
                    currentMenuTab = eMenuTab.ITEMS;
                    break;
                case (int)eMenuTab.AUDIO_DIARIES:
                    currentMenuTab = eMenuTab.AUDIO_DIARIES;
                    break;
                case (int)eMenuTab.NOTES:
                    currentMenuTab = eMenuTab.NOTES;
                    break;
                case (int)eMenuTab.SYSTEM:
                    currentMenuTab = eMenuTab.SYSTEM;
                    break;
                default:
                    Debug.LogError("FPEGameMenu.changeMenuModeTo() given bad mode '" + tab + "'. Defaulting to ITEMS.");
                    currentMenuTab = eMenuTab.ITEMS;
                    break;

            }

            refreshMenuTab();

        }

        private void restoreTabSelection(eMenuTab tab)
        {

            switch (tab)
            {

                case eMenuTab.ITEMS:
                    currentMenuTab = eMenuTab.ITEMS;
                    break;
                case eMenuTab.AUDIO_DIARIES:
                    currentMenuTab = eMenuTab.AUDIO_DIARIES;
                    break;
                case eMenuTab.NOTES:
                    currentMenuTab = eMenuTab.NOTES;
                    break;
                case eMenuTab.SYSTEM:
                    currentMenuTab = eMenuTab.SYSTEM;
                    break;
                default:
                    Debug.LogError("FPEGameMenu.changeMenuModeTo() given bad mode '" + tab + "'. Defaulting to ITEMS.");
                    currentMenuTab = eMenuTab.ITEMS;
                    break;

            }

            refreshMenuTab();

        }

        private void selectPreviousMenuTab()
        {

            switch (currentMenuTab)
            {

                case eMenuTab.ITEMS:
                    menuTabError(itemsTab);
                    break;
                case eMenuTab.AUDIO_DIARIES:
                    currentMenuTab = eMenuTab.ITEMS;
                    refreshMenuTab();
                    break;
                case eMenuTab.NOTES:
                    currentMenuTab = eMenuTab.AUDIO_DIARIES;
                    refreshMenuTab();
                    break;
                case eMenuTab.SYSTEM:
                    currentMenuTab = eMenuTab.NOTES;
                    refreshMenuTab();
                    break;
                default:
                    Debug.LogError("FPEGameMenu.changeMenuModeTo() given bad mode (which should not be possible for this function). Something has gone wrong with the eMenuMode enum.");
                    break;

            }

        }

        private void selectNextMenuTab()
        {

            switch (currentMenuTab)
            {

                case eMenuTab.ITEMS:
                    currentMenuTab = eMenuTab.AUDIO_DIARIES;
                    refreshMenuTab();
                    break;
                case eMenuTab.AUDIO_DIARIES:
                    currentMenuTab = eMenuTab.NOTES;
                    refreshMenuTab();
                    break;
                case eMenuTab.NOTES:
                    currentMenuTab = eMenuTab.SYSTEM;
                    refreshMenuTab();
                    break;
                case eMenuTab.SYSTEM:
                    menuTabError(systemTab);
                    break;
                default:
                    Debug.LogError("FPEGameMenu.changeMenuModeTo() given bad mode (which should not be possible for this function). Something has gone wrong with the eMenuMode enum.");
                    break;

            }

        }
        
        private void menuTabError(FPEMenuTab tab)
        {

            menuAudio.clip = menuError;
            menuAudio.Play();

            if (shakeUIOnError)
            {
                elementToJiggle = tab.gameObject.GetComponent<RectTransform>();
                originalElementPosition = elementToJiggle.localPosition;
                jiggleTimer = jiggleDuration;

                // Set direction of jiggle based on tab (there will only ever be 2 cases - left most or right most)
                if (currentMenuTab == eMenuTab.ITEMS)
                {
                    jiggleOffset = -jiggleDistance;
                }
                else
                {
                    jiggleOffset = jiggleDistance;
                }
            }

        }

        private void menuButtonError(FPEMenuButton button)
        {

            if (shakeUIOnError)
            {
                elementToJiggle = button.gameObject.GetComponent<RectTransform>();
                originalElementPosition = elementToJiggle.localPosition;
                jiggleTimer = jiggleDuration;
                jiggleOffset = jiggleDistance;
            }

        }

        // This function will need to be updated if you add or remove tabs
        private void refreshMenuTab()
        {

            // Attempt to save selected item and diary slots. Note: we don't save the selected 
            // note slot because that behaves a little differently, and is saved on activation.
            saveSelectedItemSlot();
            saveSelectedAudioDiarySlot();

            // And De-activate ALL panels
            inventoryItemsListPanel.SetActive(false);
            inventoryItemInfoPanelParent.SetActive(false);
            audioDiariesPanel.SetActive(false);
            notesPanel.SetActive(false);
            noteContentsPanelParent.SetActive(false);
            systemPanel.SetActive(false);
            exitConfirmationPanel.SetActive(false);
            actionsPanelParent.SetActive(false);
            pageControlPanel.SetActive(false);

            // Then force select tab and activate the correct panel(s)
            switch (currentMenuTab)
            {

                case eMenuTab.ITEMS:
                    itemsTab.ForceSelectTab();
                    refreshItemSlotsAndDetailsView();
                    inventoryItemsListPanel.SetActive(true);
                    inventoryItemInfoPanelParent.SetActive(true);
                    pageControlPanel.SetActive(true);
                    restoreSelectedItemSlot();
                    break;

                case eMenuTab.AUDIO_DIARIES:
                    audioDiariesTab.ForceSelectTab();
                    refreshAudioDiarySlotsView();
                    audioDiariesPanel.SetActive(true);
                    pageControlPanel.SetActive(true);
                    restoreSelectedAudioDiarySlot();
                    break;

                case eMenuTab.NOTES:
                    notesTab.ForceSelectTab();
                    refreshNoteSlotsView();
                    notesPanel.SetActive(true);
                    noteContentsPanelParent.SetActive(true);
                    pageControlPanel.SetActive(true);
                    //restoreSelectedNoteSlot();
                    break;

                case eMenuTab.SYSTEM:
                    systemTab.ForceSelectTab();
                    systemPanel.SetActive(true);
                    refreshSystemMenu();
                    break;

                default:
                    Debug.LogError("FPEGameMenu.refreshMenuTab() in bad state. Current Menu Tab is '" + currentMenuTab + "' which is not a valid tab. Something upstream (probably menu tab selection functions) broke.");
                    break;

            }

            menuAudio.clip = menuTabSelect;
            menuAudio.Play();

        }

        #region INVENTORY_ITEMS

        /// <summary>
        /// Updates the side "Inventory Item" detailed view with the details of a given inventory item index
        /// </summary>
        /// <param name="itemIndex">The index stored in the inventory slot UI element</param>
        public void updateItemDataView(int itemIndex, bool playAudio = true)
        {

            int actualItemIndex = itemIndex + (previouslySelectedPage[(int)eMenuTab.ITEMS] * itemsPerPage[(int)eMenuTab.ITEMS]);

            if (actualItemIndex < 0 || actualItemIndex > itemData.Length)
            {
                Debug.LogError("FPEGameMenu.updateItemDataView() given bad itemIndex of '" + actualItemIndex + "'. Cannot retrieve item data!");
            }
            else
            {

                // Manually unhighlight all slots that are not the calling slot, because Unity UI using mouseovers is tricky sometimes apparently
                for (int s = 0; s < inventorySlots.Length; s++)
                {
                    if (inventorySlots[s].CurrentInventoryDataIndex != itemIndex)
                    {
                        inventorySlots[s].ForceUnhighlight();
                    }
                }

                itemInfoPanel.setItemDetails(itemData[actualItemIndex]);

                if (playAudio)
                {
                    menuAudio.PlayOneShot(menuSelect);
                }

            }

        }

        public void clearItemDataView()
        {
            itemInfoPanel.clearItemDetails();
        }

        /// <summary>
        /// Updates the previously selected inventory item slot to be the currently selected UI element, if applicable.
        /// </summary>
        private void saveSelectedItemSlot()
        {

            if (currentMenuTab == eMenuTab.ITEMS)
            {

                GameObject selection = myEventSystem.currentSelectedGameObject;

                if (selection != null && selection.GetComponent<FPEInventoryItemSlot>() && selection.GetComponent<FPEInventoryItemSlot>().CurrentInventoryDataIndex != -1)
                {
                    previouslySelectedItemSlot = selection.GetComponent<FPEInventoryItemSlot>();
                }
                else
                {
                    previouslySelectedItemSlot = inventorySlots[0];
                }

            }

        }

        private void restoreSelectedItemSlot()
        {

            if (currentMenuTab == eMenuTab.ITEMS)
            {

                myEventSystem.SetSelectedGameObject(previouslySelectedItemSlot.gameObject);

                if (previouslySelectedItemSlot.CurrentInventoryDataIndex != -1)
                {
                    itemInfoPanel.setItemDetails(itemData[previouslySelectedItemSlot.CurrentInventoryDataIndex]);
                }
                else
                {
                    itemInfoPanel.clearItemDetails();
                }

            }

        }

        public void showActionsForItem(int itemIndex, bool playAudio = true)
        {

            saveSelectedItemSlot();
            slotActionItemIndex = itemIndex + (previouslySelectedPage[(int)eMenuTab.ITEMS] * itemsPerPage[(int)eMenuTab.ITEMS]);

            // Disable all slots so that the actions button is modal
            for (int s = 0; s < inventorySlots.Length; s++)
            {
                inventorySlots[s].disableSlot();
            }

            actionsPanelParent.SetActive(true);
            inventoryActionsPanel.setButtonStates(itemData[slotActionItemIndex].CanBeHeld, itemData[slotActionItemIndex].CanBeDropped, itemData[slotActionItemIndex].CanBeConsumed);
            myEventSystem.SetSelectedGameObject(inventoryActionsPanel.getFirstPermittedActionButton().gameObject);

        }

        public void hideActionsForItem()
        {

            actionsPanelParent.SetActive(false);

            for (int s = 0; s < inventorySlots.Length; s++)
            {
                inventorySlots[s].enableSlot();
            }

            slotActionItemIndex = -1;
            menuAudio.PlayOneShot(menuBack);
            restoreSelectedItemSlot();

        }

        private void refreshItemSlotsAndDetailsView()
        {

            itemData = FPEInventoryManagerScript.Instance.getInventoryData();

            if (itemData.Length == 0)
            {
                itemInfoPanel.clearItemDetails();
            }

            selectItemPage(previouslySelectedPage[(int)eMenuTab.ITEMS]);

        }
        
        private void selectItemPage(int pageNumber)
        {

            int maxPageNumber = getMaxPagesForTab();

            if(pageNumber >= maxPageNumber)
            {
                previouslySelectedPage[(int)eMenuTab.ITEMS] = maxPageNumber-1;
            }
            else
            {
                previouslySelectedPage[(int)eMenuTab.ITEMS] = pageNumber;
            }

            for (int i = 0; i < inventorySlots.Length; i++)
            {

                if ((i + (previouslySelectedPage[(int)eMenuTab.ITEMS] * itemsPerPage[(int)eMenuTab.ITEMS])) < itemData.Length)
                {

                    inventorySlots[i].setItemData(i, itemData[i + (previouslySelectedPage[(int)eMenuTab.ITEMS] * itemsPerPage[(int)eMenuTab.ITEMS])]);

                    // If its the first slot, set the detailed info panel based on the item in that first slot
                    if (i == 0)
                    {
                        itemInfoPanel.setItemDetails(itemData[i + (previouslySelectedPage[(int)eMenuTab.ITEMS] * itemsPerPage[(int)eMenuTab.ITEMS])]);
                    }

                }
                else
                {
                    inventorySlots[i].clearItemData();
                }

            }

            refreshPageHintsUI(previouslySelectedPage[(int)eMenuTab.ITEMS]);

        }

        #region INVENTORY_ACTIONS

        public void performDropAction()
        {

            FPEInventoryItemData tempData = itemData[slotActionItemIndex];
            FPEInventoryManagerScript.Instance.dropItemFromInventory(tempData.GameObjectInstanceID);

            // More expensive than simply refreshing the one item's quantity, but it will ensure that the data is fresh
            refreshItemSlotsAndDetailsView();
            hideActionsForItem();

        }

        public void performHoldAction()
        {

            FPEInventoryItemData tempData = itemData[slotActionItemIndex];
            FPEInventoryManagerScript.Instance.holdItemFromInventory(tempData.GameObjectInstanceID);

            refreshItemSlotsAndDetailsView();
            hideActionsForItem();

        }

        public void performConsumeAction()
        {

            FPEInventoryItemData tempData = itemData[slotActionItemIndex];
            int quantityRemaining = FPEInventoryManagerScript.Instance.consumeItemFromInventory(tempData.GameObjectInstanceID);

            if (quantityRemaining == 0)
            {
                inventorySlots[slotActionItemIndex].clearItemData();
                itemInfoPanel.clearItemDetails();
            }
            else
            {
                refreshItemSlotsAndDetailsView();
            }

            hideActionsForItem();

        }

        #endregion

        #endregion

        #region AUDIO_DIARY_ITEMS

        private void saveSelectedAudioDiarySlot()
        {

            if (currentMenuTab == eMenuTab.AUDIO_DIARIES)
            {

                GameObject selection = myEventSystem.currentSelectedGameObject;

                if (selection != null && selection.GetComponent<FPEAudioDiaryEntrySlot>() && selection.GetComponent<FPEAudioDiaryEntrySlot>().CurrentAudioDiaryIndex != -1)
                {
                    previouslySelectedAudioDiarySlot = selection.GetComponent<FPEAudioDiaryEntrySlot>();
                }
                else
                {
                    previouslySelectedAudioDiarySlot = audioDiarySlots[0];
                }

            }

        }

        private void restoreSelectedAudioDiarySlot()
        {

            if (currentMenuTab == eMenuTab.AUDIO_DIARIES)
            {
                myEventSystem.SetSelectedGameObject(previouslySelectedAudioDiarySlot.gameObject);
            }

        }

        private void refreshAudioDiarySlotsView()
        {

            audioDiaryData = FPEInventoryManagerScript.Instance.getAudioDiaryData();
            selectAudioDiaryPage(previouslySelectedPage[(int)eMenuTab.AUDIO_DIARIES]);

        }

        private void selectAudioDiaryPage(int pageNumber)
        {

            int maxPageNumber = getMaxPagesForTab();

            if (pageNumber >= maxPageNumber)
            {
                previouslySelectedPage[(int)eMenuTab.AUDIO_DIARIES] = maxPageNumber - 1;
            }
            else
            {
                previouslySelectedPage[(int)eMenuTab.AUDIO_DIARIES] = pageNumber;
            }

            for (int i = 0; i < audioDiarySlots.Length; i++)
            {

                if ((i + (previouslySelectedPage[(int)eMenuTab.AUDIO_DIARIES] * itemsPerPage[(int)eMenuTab.AUDIO_DIARIES])) < audioDiaryData.Length)
                {
                    audioDiarySlots[i].setAudioData(i, audioDiaryData[i + (previouslySelectedPage[(int)eMenuTab.AUDIO_DIARIES] * itemsPerPage[(int)eMenuTab.AUDIO_DIARIES])].DiaryTitle);
                }
                else
                {
                    audioDiarySlots[i].clearAudioData();
                }

            }

            refreshPageHintsUI(previouslySelectedPage[(int)eMenuTab.AUDIO_DIARIES]);

        }

        public void performReplayAudioDiaryAction(int diaryIndex)
        {

            int pagedDiaryIndex = diaryIndex + (previouslySelectedPage[(int)eMenuTab.AUDIO_DIARIES] * itemsPerPage[(int)eMenuTab.AUDIO_DIARIES]);

            if (pagedDiaryIndex >= 0)
            {
                FPEInventoryManagerScript.Instance.playbackAudioDiary(pagedDiaryIndex);
                menuAudio.PlayOneShot(menuDiaryPlayback);
            }
            else
            {
                menuAudio.PlayOneShot(menuSelect);
            }

        }

        public void stopAllDiaries()
        {
            FPEInventoryManagerScript.Instance.stopAllDiaryPlayback();
            menuAudio.PlayOneShot(menuDiaryStop);
        }

        #endregion

        #region NOTE_ITEMS

        private void saveSelectedNoteSlot()
        {

            if (currentMenuTab == eMenuTab.NOTES)
            {

                GameObject selection = myEventSystem.currentSelectedGameObject;

                if (selection != null && selection.GetComponent<FPENoteEntrySlot>() && selection.GetComponent<FPENoteEntrySlot>().CurrentNoteIndex != -1)
                {
                    previouslySelectedNoteSlotIndex = selection.GetComponent<FPENoteEntrySlot>().CurrentNoteIndex;
                }
                else
                {
                    previouslySelectedNoteSlotIndex = -1;
                }

            }

        }

        private void restoreSelectedNoteSlot()
        {

            if (currentMenuTab == eMenuTab.NOTES)
            {

                bool matchedNote = false;

                for (int i = 0; i < noteSlots.Length; i++)
                {

                    if ((noteSlots[i].CurrentNoteIndex + (previouslySelectedPage[(int)eMenuTab.NOTES] * itemsPerPage[(int)eMenuTab.NOTES])) == previouslySelectedNoteSlotIndex && previouslySelectedNoteSlotIndex != -1)
                    {
                        matchedNote = true;
                        noteSlots[i].ForceActivateSlot();
                        myEventSystem.SetSelectedGameObject(noteSlots[i].gameObject);
                    }
                    else
                    {
                        noteSlots[i].ForceDeactivateSlot();
                    }

                }

                if (!matchedNote)
                {
                    noteContentsPanel.clearNoteContents();
                    myEventSystem.SetSelectedGameObject(noteSlots[0].gameObject);
                    noteSlots[0].ForceHighlight();
                }

            }

        }

        private void refreshNoteSlotsView()
        {

            noteData = FPEInventoryManagerScript.Instance.getNoteData();
            selectNotePage(previouslySelectedPage[(int)eMenuTab.NOTES]);

        }

        private void selectNotePage(int pageNumber)
        {

            int maxPageNumber = getMaxPagesForTab();

            if (pageNumber >= maxPageNumber)
            {
                previouslySelectedPage[(int)eMenuTab.NOTES] = maxPageNumber - 1;
            }
            else
            {
                previouslySelectedPage[(int)eMenuTab.NOTES] = pageNumber;
            }

            for (int i = 0; i < noteSlots.Length; i++)
            {

                if ((i + (previouslySelectedPage[(int)eMenuTab.NOTES] * itemsPerPage[(int)eMenuTab.NOTES])) < noteData.Length)
                {
                    noteSlots[i].setNoteData(i, noteData[i + (previouslySelectedPage[(int)eMenuTab.NOTES] * itemsPerPage[(int)eMenuTab.NOTES])].NoteTitle);
                }
                else
                {
                    noteSlots[i].clearNoteData();
                }

            }

            restoreSelectedNoteSlot();
            refreshPageHintsUI(previouslySelectedPage[(int)eMenuTab.NOTES]);

        }

        public void displayNote(int noteIndex)
        {

            previouslySelectedNoteSlotIndex = noteIndex + (previouslySelectedPage[(int)eMenuTab.NOTES] * itemsPerPage[(int)eMenuTab.NOTES]);
            noteContentsPanel.displayNoteContents(noteData[noteIndex + (previouslySelectedPage[(int)eMenuTab.NOTES] * itemsPerPage[(int)eMenuTab.NOTES])].NoteTitle, noteData[noteIndex + (previouslySelectedPage[(int)eMenuTab.NOTES] * itemsPerPage[(int)eMenuTab.NOTES])].NoteBody);

        }

        #endregion

        #region SYSTEM

        private void refreshSystemMenu()
        {

            // By default, always select the first button in the system panel
            if (currentMenuTab == eMenuTab.SYSTEM)
            {

                if (FPESaveLoadManager.Instance != null)
                {
                    FPESaveLoadManager.Instance.LoadGameOptions();
                }

                myEventSystem.SetSelectedGameObject(systemButtons[0].gameObject);
                refreshOptionsValues();

                // Also check if there is a saved game. If there is not, we want to disable the load game button.
                if (FPESaveLoadManager.Instance != null)
                {
                    if (FPESaveLoadManager.Instance.SavedGameExists())
                    {
                        loadGameButton.enableButton();
                    }
                    else
                    {
                        loadGameButton.disableButton();
                    }
                }

            }

        }

        public void SaveGame()
        {

            deactivateMenu();
            Time.timeScale = 1.0f;
            FPESaveLoadManager.Instance.SaveGame();

        }

        public void LoadGame()
        {

            deactivateMenu();
            Time.timeScale = 1.0f;
            FPESaveLoadManager.Instance.LoadGame();

        }

        public void showExitConfirmation()
        {

            exitConfirmationPanel.SetActive(true);
            // Highlight the 'no' button by default
            myEventSystem.SetSelectedGameObject(exitConfirmationButtons[exitConfirmationButtons.Length-1].gameObject);

        }

        public void hideExitConfirmation()
        {

            exitConfirmationPanel.SetActive(false);
            // When returning to menu, assume that we should re-highlight the exit button
            myEventSystem.SetSelectedGameObject(systemButtons[systemButtons.Length-1].gameObject);

        }

        public void exitGameButtonPressed()
        {

            deactivateMenu();
            Time.timeScale = 1.0f;
            FPEInteractionManagerScript.Instance.stopAllDiaryPlayback();
            FPESaveLoadManager.Instance.ReturnToMainMenu();

        }


        public void changeMouseSensitivity(float amount)
        {

            float changedSensitivity = FPEInputManager.Instance.LookSensitivity.x + amount;
            if(changedSensitivity < minSensitivity)
            {
                changedSensitivity = minSensitivity;
            }
            else if (changedSensitivity > maxSensitivity)
            {
                changedSensitivity = maxSensitivity;
            }

            FPEInteractionManagerScript.Instance.changeMouseSensitivityFromMenu(changedSensitivity);
            refreshOptionsValues();

        }

        private void refreshOptionsValues()
        {

            mouseSensitivityValueText.text = FPEInputManager.Instance.LookSensitivity.x.ToString("n1");
            //lookSmoothingToggle.ForceToggleState(FPEInputManager.Instance.LookSmoothing);
            //useGamepadToggle.ForceToggleState(FPEInputManager.Instance.UseGamepad);
            flipMouseYAxisToggle.ForceToggleState(FPEInputManager.Instance.FlipYAxisMouseOnly);
            //flipGamepadYAxisToggle.ForceToggleState(FPEInputManager.Instance.FlipYAxisGamepadOnly);

            // Always save options when they are changed - assumes they are refreshed using this function when changed from UI.
            if (FPESaveLoadManager.Instance != null)
            {
                FPESaveLoadManager.Instance.SaveGameOptions();
            }

        }

        public void toggleLookSmoothing(bool toggleValue)
        {
            FPEInputManager.Instance.LookSmoothing = toggleValue;
            refreshOptionsValues();
        }

        public void toggleUseGamepad(bool toggleValue)
        {
            FPEInputManager.Instance.UseGamepad = toggleValue;
            refreshOptionsValues();
        }


        // Toggles on Y-axis flip for gamepad and mouse
        // Note: If you want to use this instead of distinct checkboxes 
        // for mouse and gamepad, customize the FPEGameMenu prefab or 
        // create a new menu prefab accordingly
        public void toggleYAxisFlipForAll(bool toggleValue)
        {

            FPEInputManager.Instance.FlipYAxisMouseOnly = toggleValue;
            FPEInputManager.Instance.FlipYAxisGamepadOnly = toggleValue;
            refreshOptionsValues();

        }

        // Toggles on Y-axis flip for MOUSE ONLY
        public void toggleYAxisFlipForMouseOnly(bool toggleValue)
        {

            FPEInputManager.Instance.FlipYAxisMouseOnly = toggleValue;
            refreshOptionsValues();

        }

        // Toggles on Y-axis flip for GAMEPAD ONLY
        public void toggleYAxisFlipForGamepadOnly(bool toggleValue)
        {

            FPEInputManager.Instance.FlipYAxisGamepadOnly = toggleValue;
            refreshOptionsValues();

        }

        #endregion

        #region COMMON

        public void moveToPreviousPage()
        {

            if (previouslySelectedPage[(int)currentMenuTab] > 0)
            {
                previouslySelectedPage[(int)currentMenuTab] -= 1;
                menuAudio.PlayOneShot(menuPageTurn);
                refreshPage();
            }

        }

        public void moveToNextPage()
        {

            int maxPageNumber = getMaxPagesForTab();

            if ((previouslySelectedPage[(int)currentMenuTab] + 1) < maxPageNumber)
            {
                previouslySelectedPage[(int)currentMenuTab] += 1;
                menuAudio.PlayOneShot(menuPageTurn);
                refreshPage();
            }

        }

        private void refreshPage()
        {

            switch (currentMenuTab)
            {
                case eMenuTab.ITEMS:
                    selectItemPage(previouslySelectedPage[(int)eMenuTab.ITEMS]);
                    break;
                case eMenuTab.AUDIO_DIARIES:
                    selectAudioDiaryPage(previouslySelectedPage[(int)eMenuTab.AUDIO_DIARIES]);
                    break;
                case eMenuTab.NOTES:
                    selectNotePage(previouslySelectedPage[(int)eMenuTab.NOTES]);
                    break;
                default:
                    Debug.LogError("FPEGameMenu.refreshPage() encountered bad currentMenuTab value of '" + currentMenuTab + "'. Defaulting to Items tab behaviour.");
                    break;

            }

        }

        // Refreshes the page hints for current tab
        private void refreshPageHintsUI(int pageNumber)
        {

            int maxPageNumber = getMaxPagesForTab();
            pageIndicatorText.text = (pageNumber + 1) + "/" + maxPageNumber + " 页";

            // If we have 1 page, disable all buttons
            if (maxPageNumber == 1)
            {

                previousPageButton.GetComponent<FPEMenuButton>().disableButton();
                nextPageButton.GetComponent<FPEMenuButton>().disableButton();
                previousPageHint.SetActive(false);
                nextPageHint.SetActive(false);

                if(itemData.Length == 0)
                {
                    pageIndicatorText.text = "-/- 页";
                }

            }
            // If we have more than one, and are on the first page, only enable next button
            else if (maxPageNumber > 1 && pageNumber == 0)
            {

                previousPageButton.GetComponent<FPEMenuButton>().disableButton();
                nextPageButton.GetComponent<FPEMenuButton>().enableButton();
                previousPageHint.SetActive(false);
                nextPageHint.SetActive(true);

            }
            // If we have more than one, and are not on that one, enable both buttons
            else if (maxPageNumber > 1 && pageNumber != (maxPageNumber-1))
            {

                previousPageButton.GetComponent<FPEMenuButton>().enableButton();
                nextPageButton.GetComponent<FPEMenuButton>().enableButton();
                previousPageHint.SetActive(true);
                nextPageHint.SetActive(true);

            }
            // If we have more than one, and are on last page, only enable previous button
            else if (maxPageNumber > 1 && pageNumber == (maxPageNumber - 1))
            {

                previousPageButton.GetComponent<FPEMenuButton>().enableButton();
                nextPageButton.GetComponent<FPEMenuButton>().disableButton();
                previousPageHint.SetActive(true);
                nextPageHint.SetActive(false);

            }
            else
            {
                Debug.LogError("FPEGameMenu.refreshPageHintsUI():: Encountered bad combination of number of inventory items and max pages for '"+currentMenuTab+"' tab. Prev/Next page buttons won't work.");
            }

        }

        private int getMaxPagesForTab()
        {

            int maxPageNumber = 0;

            switch (currentMenuTab)
            {
                case eMenuTab.ITEMS:
                    maxPageNumber = ((itemData.Length - 1) / itemsPerPage[(int)eMenuTab.ITEMS]) + 1;
                    break;
                case eMenuTab.AUDIO_DIARIES:
                    maxPageNumber = ((audioDiaryData.Length - 1) / itemsPerPage[(int)eMenuTab.AUDIO_DIARIES]) + 1;
                    break;
                case eMenuTab.NOTES:
                    maxPageNumber = ((noteData.Length - 1) / itemsPerPage[(int)eMenuTab.NOTES]) + 1;
                    break;
                case eMenuTab.SYSTEM:
                    // Nothing to do here
                    break;
                default:
                    Debug.Log("FPEGameMenu.getMaxPagesForTab() encountered bad currentMenuTab value of '" + currentMenuTab + "'. Returning zero.");
                    break;
            }

            return maxPageNumber;

        }

        #endregion

    }

}