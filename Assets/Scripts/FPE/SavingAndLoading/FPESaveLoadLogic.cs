using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

namespace Whilefun.FPEKit
{

    //
    // FPESaveLoadLogic
    // This class contains all the functionality to gather and restore game object 
    // data from a scene in order to transport it to and from save game files.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPESaveLoadLogic : MonoBehaviour
    {

        private FPEObjectTypeLookup myLookup;
        private Dictionary<FPEInventoryManagerScript.eInventoryItems, string> inventoryLookupTable;

        void Start()
        {
            myLookup = new FPEObjectTypeLookup();
            inventoryLookupTable = myLookup.InventoryItemsLookup;
        }

        //
        // The following regions all relate to gathering and restoring 
        // saved game data types with relation to real in game objects.
        //
        // Note: If you create your own types, the gather...() and 
        // restore...() functions should go in the CUSTOM_SAVE_LOAD_LOGIC
        // region at the bottom of this file.
        //

        #region SCENE_DATA

        /// <summary>
        /// Gathers scene data for saving
        /// </summary>
        /// <returns>Scene data package for active scene</returns>
        public FPESceneSaveData gatherSceneData()
        {
            return new FPESceneSaveData(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion

        #region INVENTORY_ITEM_DATA

        /// <summary>
        /// Gathers data from player's inventory (items, audio diaries, notes)
        /// </summary>
        /// <returns>A saveable data package for the inventory at save time</returns>
        public FPEInventorySaveData gatherInventorySaveData()
        {

            #region HELD_OBJECT

            FPEInteractableBaseScript.eInteractionType heldObjectType = FPEInteractionManagerScript.Instance.getHeldObjectType();
            GameObject heldObject = FPEInteractionManagerScript.Instance.getHeldObject();

            FPEHeldObjectSaveData heldObjectData = null;

            if (heldObjectType == FPEInteractableBaseScript.eInteractionType.PICKUP)
            {
                string scrubbedName = heldObject.gameObject.name.Split(FPEObjectTypeLookup.PickupPrefabDelimiter)[0];
                // Here, we pass in the first eInventoryItems value since we'll be ignoring it anyway, and it's "better" than exposing a "NO TYPE" in the enum list in the Inspector.
                heldObjectData = new FPEHeldObjectSaveData(true, scrubbedName, FPEInventoryManagerScript.eInventoryItems.APPLE, heldObject.transform.localRotation);
            }
            else if (heldObjectType == FPEInteractableBaseScript.eInteractionType.INVENTORY)
            {
                heldObjectData = new FPEHeldObjectSaveData(true, "", heldObject.GetComponent<FPEInteractableInventoryItemScript>().InventoryItemType, heldObject.transform.localRotation);
            }
            else
            {
                heldObjectData = new FPEHeldObjectSaveData(false, "", FPEInventoryManagerScript.eInventoryItems.APPLE, Quaternion.identity);
            }

            #endregion

            FPEInventoryManagerScript invManager = FPEInventoryManagerScript.Instance;

            #region INVENTORY_ITEMS

            List<FPEInteractableInventoryItemScript> invItems = invManager.getInventoryItems();
            int[] invQty = invManager.getInventoryQuantities();
            List<FPEInventoryItemSaveData> invData = new List<FPEInventoryItemSaveData>();
            int tempQty = 0;

            for (int i = 0; i < invItems.Count; i++)
            {

                if (invItems[i].Stackable)
                {

                    tempQty = invQty[(int)invItems[i].InventoryItemType];

                    for (int q = 0; q < tempQty; q++)
                    {
                        invData.Add(new FPEInventoryItemSaveData(invItems[i].InventoryItemType));
                    }

                }
                else
                {
                    invData.Add(new FPEInventoryItemSaveData(invItems[i].InventoryItemType));
                }

            }

            #endregion

            #region NOTES_AND_AUDIO_DIARIES

            // Notes
            FPENoteEntry[] notes = invManager.getNoteDataForSavedGame();
            FPENoteSaveData[] noteData = new FPENoteSaveData[notes.Length];

            for (int n = 0; n < notes.Length; n++)
            {
                noteData[n] = new FPENoteSaveData(notes[n].NoteTitle, notes[n].NoteBody);
            }

            // Audio Diaries
            FPEAudioDiaryEntry[] diaries = invManager.getAudioDiaryDataForSavedGame();
            FPEAudioDiaryEntrySaveData[] diaryData = new FPEAudioDiaryEntrySaveData[diaries.Length];

            for (int a = 0; a < diaries.Length; a++)
            {
                diaryData[a] = new FPEAudioDiaryEntrySaveData(diaries[a].DiaryTitle, diaries[a].getAudioDiaryClipPath(), diaries[a].ShowDiaryTitle);
            }

            #endregion

            FPEInventorySaveData inventoryData = new FPEInventorySaveData(heldObjectData, invData.ToArray(), diaryData, noteData);

            return inventoryData;

        }

        /// <summary>
        /// Restores player's inventory (items, audio diaries, notes)
        /// </summary>
        /// <param name="data">The data from which to base restored inventory</param>
        public void restoreInventorySaveData(FPEInventorySaveData data)
        {

            FPEInventoryManagerScript invManager = FPEInventoryManagerScript.Instance;
            // Clear existing inventory
            invManager.clearInventoryItems();

            // Held Object
            FPEHeldObjectSaveData heldObjData = data.HeldObjectData;

            #region HELD_OBJECT
            if (heldObjData.HeldSomething)
            {

                GameObject tempLoadedObject = null;

                // Pickup Type
                if (heldObjData.PickupPrefabName != "")
                {

                    Object tempObject = Resources.Load(FPEObjectTypeLookup.PickupResourcePath + heldObjData.PickupPrefabName);

                    if (tempObject != null)
                    {

                        tempLoadedObject = Instantiate(tempObject) as GameObject;
                        tempLoadedObject.name = heldObjData.PickupPrefabName;
                        tempLoadedObject.transform.localRotation = heldObjData.LocalRotation;

                    }
                    else
                    {
                        Debug.LogError("FPESaveLoadLogic:: Loading data encountered unknown Pickup named '" + heldObjData.PickupPrefabName + "'. No prefab was found with this name. This object will NOT be loaded.  Ensure that all Pickup prefabs are located in the '" + FPEObjectTypeLookup.PickupResourcePath + "' sub folder of a Resources folder.");
                    }

                }
                // Inventory Type
                else
                {

                    if (inventoryLookupTable.ContainsKey(heldObjData.InventoryItemType))
                    {

                        string tempPath = "";

                        if (inventoryLookupTable.TryGetValue(heldObjData.InventoryItemType, out tempPath))
                        {

                            tempLoadedObject = Instantiate(Resources.Load(FPEObjectTypeLookup.InventoryResourcePath + tempPath)) as GameObject;
                            tempLoadedObject.transform.localRotation = heldObjData.LocalRotation;

                        }
                        else
                        {
                            Debug.LogError("FPESaveLoadLogic:: Loading data could not get value for InventoryItemType '" + heldObjData.InventoryItemType + "'");
                        }

                    }
                    else
                    {
                        Debug.LogError("FPESaveLoadLogic:: Loading data encountered unknown InventoryItemType '" + heldObjData.InventoryItemType + "'. This object will NOT be loaded.  Ensure that all Inventory Item prefabs are located in the '" + FPEObjectTypeLookup.InventoryResourcePath + "' sub folder of a Resources folder. Also ensure that there is an entry in the FPEObjectTypeLookup 'inventoryItemsLookup' Dictionary for type '" + heldObjData.InventoryItemType + "'");
                    }

                }

                // Lastly, put the object into player's hand
                FPEInteractablePickupScript pickup = tempLoadedObject.GetComponent<FPEInteractablePickupScript>();

                if (pickup)
                {
                    FPEInteractionManagerScript.Instance.holdObjectFromGameLoad(pickup);
                }
                else
                {
                    Debug.LogError("FPESaveLoadLogic:: Loading held object for object '" + tempLoadedObject.gameObject.name + "', but its prefab had no attached FPEInteractablePickupScript component. Object will not be loaded. Check prefab.");
                }

            }
            #endregion

            #region INVENTORY_ITEMS

            FPEInventoryItemSaveData[] loadedItemData = data.InventoryItemData;

            // Create all the items, and add each to inventory
            GameObject tempInvObject = null;
            string tempInvPath;
            FPEInteractableInventoryItemScript tempInvItem = null;

            // We start at index 1 to skip over our padded 0th value
            for (int i = 0; i < loadedItemData.Length; i++)
            {

                if (inventoryLookupTable.ContainsKey(loadedItemData[i].InventoryItemType))
                {

                    // Added this as an if, did that break things?
                    if (inventoryLookupTable.TryGetValue(loadedItemData[i].InventoryItemType, out tempInvPath))
                    {

                        tempInvObject = Instantiate(Resources.Load(FPEObjectTypeLookup.InventoryResourcePath + tempInvPath)) as GameObject;
                        tempInvItem = tempInvObject.GetComponent<FPEInteractableInventoryItemScript>();

                        if (tempInvItem != null)
                        {
                            FPEInteractionManagerScript.Instance.putObjectIntoInventory(tempInvItem, false);
                        }
                        else
                        {
                            Debug.LogError("FPESaveLoadLogic:: Loaded Inventory Item prefab '" + FPEObjectTypeLookup.InventoryResourcePath + tempInvPath + "' had no attached FPEInteractableInventoryItemScript component. Item will NOT be restored into player inventory");
                        }

                    }
                    else
                    {
                        Debug.LogError("FPESaveLoadLogic:: Loading data could not get value for InventoryItemType '" + loadedItemData[i].InventoryItemType + "'");
                    }

                }
                else
                {
                    Debug.LogError("FPESaveLoadLogic:: Loading data encountered unknown InventoryItemType '" + loadedItemData[i].InventoryItemType + "'. This object will NOT be restored into player inventory.  Ensure that there is an entry in the FPEObjectTypeLookup 'inventoryItemsLookup' Dictionary for type '" + loadedItemData[i].InventoryItemType + "'");
                }

            }

            #endregion

            #region NOTES_AND_AUDIO_DIARIES

            FPENoteSaveData[] loadedNoteData = data.NoteData;
            List<FPENoteEntry> noteEntries = new List<FPENoteEntry>();

            for (int n = 0; n < loadedNoteData.Length; n++)
            {
                noteEntries.Add(loadedNoteData[n].getNoteEntry());
            }

            invManager.setNoteDataFromSavedGame(noteEntries);

            FPEAudioDiaryEntrySaveData[] loadedDiaryData = data.AudioDiaryData;
            List<FPEAudioDiaryEntry> diaryEntries = new List<FPEAudioDiaryEntry>();

            for (int a = 0; a < loadedDiaryData.Length; a++)
            {
                diaryEntries.Add(loadedDiaryData[a].getAudioDiaryEntry());
            }

            invManager.setAudioDiaryDataFromSavedGame(diaryEntries);

            #endregion

        }

        #endregion

        #region INVENTORY_IN_WORLD

        /// <summary>
        /// Gathers information about Inventory Item type objects in the world, and stores that data into a save data container.
        /// The 0th value in the returned array will always be ignored, and is present as 'padding' for the case where there are 
        /// no such objects in the level.
        /// </summary>
        /// <returns>Array of Inventory Item type objects in the world. 0th value is padding.</returns>
        public FPEInventoryWorldSaveData[] gatherInventoryInWorld()
        {

            FPEInteractableInventoryItemScript[] invObjs = GameObject.FindObjectsOfType<FPEInteractableInventoryItemScript>();
            List<FPEInventoryWorldSaveData> invData = new List<FPEInventoryWorldSaveData>();

            // Zeroth value in our array is a padded value in case the level we're saving had no inventory world objects in it
            invData.Add(new FPEInventoryWorldSaveData(Vector3.zero, Quaternion.identity, 0));

            GameObject heldObject = FPEInteractionManagerScript.Instance.getHeldObject();
            int heldObjectID = 0;

            // Case 1: Player was holding something
            if (heldObject != null)
            {

                heldObjectID = heldObject.gameObject.GetInstanceID();

                for (int i = 0; i < invObjs.Length; i++)
                {

                    // Only add it to our list if player was not holding it (prevent cloning)
                    if (heldObjectID != invObjs[i].gameObject.GetInstanceID())
                    {
                        invData.Add(new FPEInventoryWorldSaveData(invObjs[i].transform.position, invObjs[i].transform.rotation, invObjs[i].InventoryItemType));
                    }

                }

            }
            // Case 2: Player was holding nothing
            else
            {

                for (int i = 0; i < invObjs.Length; i++)
                {
                    invData.Add(new FPEInventoryWorldSaveData(invObjs[i].transform.position, invObjs[i].transform.rotation, invObjs[i].InventoryItemType));
                }

            }

            return invData.ToArray(); ;

        }

        /// <summary>
        /// Simply destroys all Inventory Item type objects in the world
        /// </summary>
        /// <param name="removeHeldObject">If true, and if player was holding an Inventory Item type object, that object will also be destroyed</param>
        public void removeAllInventoryInWorld(bool removeHeldObject)
        {

            FPEInteractableInventoryItemScript[] invObjs = GameObject.FindObjectsOfType<FPEInteractableInventoryItemScript>();

            GameObject heldObject = FPEInteractionManagerScript.Instance.getHeldObject();
            int heldObjectID = 0;

            // Case 1: Avoid deleting the object held by player. This is the slower of the two options.
            // But we only have to do this if: 
            // A) The held object exists
            // B) The held object is an Inventory Type.
            // C) We are NOT remove held object 
            if (heldObject != null && heldObject.GetComponent<FPEInteractableInventoryItemScript>() && removeHeldObject == false)
            {

                heldObjectID = heldObject.GetInstanceID();

                for (int i = 0; i < invObjs.Length; i++)
                {

                    if (invObjs[i].gameObject.GetInstanceID() != heldObjectID)
                    {
                        Destroy(invObjs[i].gameObject);
                    }

                }

            }
            // Case 2: Don't bother checking because we don't have to (Faster)
            else
            {

                for (int i = 0; i < invObjs.Length; i++)
                {
                    Destroy(invObjs[i].gameObject);
                }

            }

        }

        public void createWorldInventory(FPEInventoryWorldSaveData[] data)
        {

            GameObject tempLoadedObject = null;
            string tempPath;

            // We start at index 1 to skip over our padded 0th value
            for (int i = 1; i < data.Length; i++)
            {

                if (inventoryLookupTable.ContainsKey(data[i].InventoryItemType))
                {

                    // Added this as an if, did that break things?
                    if (inventoryLookupTable.TryGetValue(data[i].InventoryItemType, out tempPath))
                    {
                        tempLoadedObject = Instantiate(Resources.Load(FPEObjectTypeLookup.InventoryResourcePath + tempPath)) as GameObject;
                        tempLoadedObject.transform.position = data[i].getPosition();
                        tempLoadedObject.transform.rotation = data[i].getRotation();
                    }
                    else
                    {
                        Debug.LogError("FPESaveLoadLogic:: Loading data could not get value for InventoryItemType '" + data[i].InventoryItemType + "'");
                    }

                }
                else
                {
                    Debug.LogError("FPESaveLoadLogic:: Loading data encountered unknown InventoryItemType '" + data[i].InventoryItemType + "'. This object will NOT be loaded. Ensure that all Inventory Item prefabs are located in the '"+ FPEObjectTypeLookup.InventoryResourcePath + "' sub folder of a Resources folder.  Also ensure that there is an entry in the FPEObjectTypeLookup 'inventoryItemsLookup' Dictionary for type '" + data[i].InventoryItemType + "'");
                }

            }

        }

        #endregion

        #region PICKUPS_IN_WORLD

        /// <summary>
        /// Gathers all Pickup type objects from the active scene and returns their save data.
        /// </summary>
        /// <returns>An array of saveable data for the pickups found</returns>
        public FPEPickupWorldSaveData[] gatherPickupsInWorld()
        {

            FPEInteractablePickupScript[] puObjs = GameObject.FindObjectsOfType<FPEInteractablePickupScript>();
            List<FPEPickupWorldSaveData> puData = new List<FPEPickupWorldSaveData>();
            string scrubbedName = "";

            GameObject heldObject = FPEInteractionManagerScript.Instance.getHeldObject();
            int heldObjectID = 0;

            // Zeroth value in our list is a padded value in case the level we're saving had no pickup world objects in it
            puData.Add(new FPEPickupWorldSaveData(Vector3.zero, Quaternion.identity, "PAD_VALUE"));

            // Case 1: Player was holding something
            if (heldObject != null)
            {

                heldObjectID = heldObject.gameObject.GetInstanceID();

                for (int i = 0; i < puObjs.Length; i++)
                {

                    // Only add if it was not also an inventory type AND the player was not holding it
                    if ((puObjs[i].gameObject.GetComponent<FPEInteractableInventoryItemScript>() == null) && (heldObjectID != puObjs[i].gameObject.GetInstanceID()))
                    {

                        scrubbedName = puObjs[i].gameObject.name.Split(FPEObjectTypeLookup.PickupPrefabDelimiter)[0];
                        puData.Add(new FPEPickupWorldSaveData(puObjs[i].transform.position, puObjs[i].transform.rotation, scrubbedName));

                    }

                }

            }
            // Case 2: Player was holding nothing
            else
            {

                for (int i = 0; i < puObjs.Length; i++)
                {

                    // Only add if it was not an inventory type
                    if (puObjs[i].gameObject.GetComponent<FPEInteractableInventoryItemScript>() == null)
                    {

                        scrubbedName = puObjs[i].gameObject.name.Split(FPEObjectTypeLookup.PickupPrefabDelimiter)[0];
                        puData.Add(new FPEPickupWorldSaveData(puObjs[i].transform.position, puObjs[i].transform.rotation, scrubbedName));

                    }

                }

            }

            return puData.ToArray();

        }

        /// <summary>
        /// Simply destroys all Pickup type objects in the world
        /// </summary>
        /// <param name="removeHeldObject">If true, and if player was holding a pickup type object, that object will also be destroyed</param>
        public void removeAllPickupsInWorld(bool removeHeldObject)
        {

            FPEInteractablePickupScript[] puObjs = GameObject.FindObjectsOfType<FPEInteractablePickupScript>();
            GameObject heldObject = FPEInteractionManagerScript.Instance.getHeldObject();

            int heldObjectID = 0;

            // Case 1: Avoid deleting the object held by player. This is the slower of the two options.
            // But we only have to do this if: 
            // A) The held object exists
            // B) The held object is a Pickup Type and Not an Inventory Type.
            // C) We are NOT remove held object 
            if (heldObject != null && (heldObject.GetComponent<FPEInteractablePickupScript>() && heldObject.GetComponent<FPEInteractableInventoryItemScript>() == null) && removeHeldObject == false)
            {

                heldObjectID = heldObject.GetInstanceID();

                for (int i = 0; i < puObjs.Length; i++)
                {

                    // Only destroy it if it is NOT the held object
                    if (puObjs[i].gameObject.GetComponent<FPEInteractableInventoryItemScript>() == null && puObjs[i].gameObject.GetInstanceID() != heldObjectID)
                    {
                        Destroy(puObjs[i].gameObject);
                    }

                }

            }
            // Case 2: Don't bother checking because we don't have to (Faster)
            else
            {

                for (int i = 0; i < puObjs.Length; i++)
                {

                    // Skip it if it is ALSO an inventory item
                    if (puObjs[i].gameObject.GetComponent<FPEInteractableInventoryItemScript>() == null)
                    {
                        Destroy(puObjs[i].gameObject);
                    }

                }

            }

        }

        /// <summary>
        /// Creates Pickup objects by instantiating their associated prefabs at the previously saved location, rotation, etc.
        /// </summary>
        /// <param name="data">An array pickup data from which to restore objects</param>
        public void createWorldPickups(FPEPickupWorldSaveData[] data)
        {

            GameObject tempLoadedObject = null;
            UnityEngine.Object tempObject = null;

            // We start at index 1 to skip over our padded 0th value
            for (int i = 1; i < data.Length; i++)
            {

                tempObject = Resources.Load(FPEObjectTypeLookup.PickupResourcePath + data[i].PrefabName);

                if (tempObject != null)
                {
                    
                    tempLoadedObject = Instantiate(tempObject) as GameObject;
                    tempLoadedObject.name = data[i].PrefabName;
                    tempLoadedObject.transform.position = data[i].getPosition();
                    tempLoadedObject.transform.rotation = data[i].getRotation();

                }
                else
                {
                    Debug.LogError("FPESaveLoadLogic:: Loading data encountered unknown Pickup named '" + data[i].PrefabName + "'. No prefab was found with this name. This object will NOT be loaded. Ensure that all Pickup prefabs are located in the '" + FPEObjectTypeLookup.PickupResourcePath + "' sub folder of a Resources folder.");
                }

            }

        }
        
        #endregion

        #region PLAYER_DATA

        /// <summary>
        /// Gathers player's data for saving (position, rotation, etc.)
        /// </summary>
        /// <returns>The packaged data based on player state</returns>
        public FPEPlayerStateSaveData gatherPlayerData()
        {

            FPEPlayerStateSaveData stateData = null;
            GameObject thePlayer = FPEPlayer.Instance.gameObject;

            if (!thePlayer)
            {
                Debug.LogError("FPESaveLoadManager.gatherPlayerData():: Cannot find Player! Is there a object tagged 'Player' in your scene?");
            }
            else
            {
                stateData = thePlayer.GetComponent<FPEFirstPersonController>().getPlayerStateDataForSavedGame();
            }

            return stateData;

        }

        /// <summary>
        /// Places the player back at the required location based on provided data package
        /// </summary>
        /// <param name="stateData">The data from which to restore the player's previous state</param>
        public void relocatePlayer(FPEPlayerStateSaveData stateData)
        {

            GameObject thePlayer = FPEPlayer.Instance.gameObject;

            if (!thePlayer)
            {
                Debug.LogError("FPESaveLoadManager.gatherPlayerData():: Cannot find Player! Is there a object tagged 'Player' in your scene?");
            }
            else
            {
                thePlayer.GetComponent<FPEFirstPersonController>().restorePlayerStateFromSavedGame(stateData);
            }

        }

        #endregion

        #region  TRIGGER_TYPE

        /// <summary>
        /// Gathers data about triggers from the world
        /// </summary>
        /// <returns></returns>
        public FPETriggerSaveData[] gatherTriggerData()
        {

            FPEEventTrigger[] allTriggers = GameObject.FindObjectsOfType<FPEEventTrigger>();
            FPETriggerSaveData[] saveData = new FPETriggerSaveData[allTriggers.Length];

            for (int t = 0; t < allTriggers.Length; t++)
            {
                saveData[t] = allTriggers[t].getSaveData();
            }

            return saveData;

        }

        /// <summary>
        /// Restores trigger states from saved data
        /// </summary>
        /// <param name="data">The trigger data from which to restore states</param>
        public void restoreTriggerData(FPETriggerSaveData[] data)
        {

            FPEEventTrigger[] allTriggers = GameObject.FindObjectsOfType<FPEEventTrigger>();
            bool foundMatch = false;

            for (int d = 0; d < data.Length; d++)
            {

                for (int t = 0; t < allTriggers.Length; t++)
                {

                    if (data[d].ObjectName == allTriggers[t].gameObject.name)
                    {
                        foundMatch = true;
                        allTriggers[t].restoreSaveGameData(data[d]);
                        break;
                    }

                }

                if (!foundMatch)
                {
                    Debug.LogWarning("FPESaveLoadManager.restoreTriggerData():: Found no match for matching saved Trigger object called '" + data[d].ObjectName + "'. Skipping this data. This means your scene object names changed, or your old saved game data is not compatible with the current version of your scene or game.");
                }

                foundMatch = false;

            }

        }

        #endregion

        #region ACTIVATE_TYPE

        /// <summary>
        /// Gathers save data for all Activate type objects in the world
        /// </summary>
        /// <returns>Array of data for the Activate type objects</returns>
        public FPEActivateSaveData[] gatherActivateTypeData()
        {

            FPEInteractableActivateScript[] activateObjs = GameObject.FindObjectsOfType<FPEInteractableActivateScript>();
            FPEActivateSaveData[] saveData = new FPEActivateSaveData[activateObjs.Length];

            for (int a = 0; a < activateObjs.Length; a++)
            {
                saveData[a] = activateObjs[a].getSaveGameData();
            }

            return saveData;

        }

        /// <summary>
        /// Restores data to Activate type objects found in the world
        /// </summary>
        /// <param name="data">The array of Activate type saved data from which to restore</param>
        public void restoreActivateData(FPEActivateSaveData[] data)
        {

            FPEInteractableActivateScript[] activateObjs = GameObject.FindObjectsOfType<FPEInteractableActivateScript>();
            bool foundMatch = false;

            for (int d = 0; d < data.Length; d++)
            {

                for (int a = 0; a < activateObjs.Length; a++)
                {

                    if (data[d].ObjectName == activateObjs[a].gameObject.name)
                    {
                        foundMatch = true;
                        activateObjs[a].restoreSaveGameData(data[d]);
                        break;
                    }

                }

                if (!foundMatch)
                {
                    Debug.LogWarning("FPESaveLoadManager.restoreActivateData():: Found no match for matching saved Activate object called '" + data[d].ObjectName + "'. Skipping this data. This means your scene object names changed, or your old saved game data is not compatible with the current version of your scene or game.");
                }

                foundMatch = false;

            }

        }

        #endregion

        #region DOORS

        /// <summary>
        /// Gathers save data for FPEDoor objects
        /// </summary>
        /// <returns>Array of saveable data for FPEDoors</returns>
        public FPEDoorSaveData[] gatherDoorTypeData()
        {

            FPEDoor[] allTheDoors = GameObject.FindObjectsOfType<FPEDoor>();
            List<FPEDoorSaveData> doorData = new List<FPEDoorSaveData>();

            // We add padding to 0th entry in case there were no doors in the scene that was saved
            doorData.Add(new FPEDoorSaveData("PADDING", 0, "", Vector3.zero, false, false));

            for(int d = 0; d < allTheDoors.Length; d++)
            {
                doorData.Add(allTheDoors[d].getSaveGameData());
            }

            return doorData.ToArray();

        }

        /// <summary>
        /// Restores saved data to FPEDoors
        /// </summary>
        /// <param name="data">Array of FPEDoor data from which to restore states</param>
        public void restoreDoorData(FPEDoorSaveData[] data)
        {

            FPEDoor[] doorObjs = GameObject.FindObjectsOfType<FPEDoor>();

            bool foundMatch = false;

            // We start at index 1 since we always have a padding entry at the 0th index
            for (int d = 1; d < data.Length; d++)
            {

                for (int i = 0; i < doorObjs.Length; i++)
                {

                    if (data[d].ObjectName == doorObjs[i].gameObject.name)
                    {
                        foundMatch = true;
                        doorObjs[i].restoreSaveGameData(data[d]);
                        break;
                    }

                }

                if (!foundMatch)
                {
                    Debug.LogWarning("FPESaveLoadManager.restoreDoorData():: Found no match for matching saved Simple Door object called '" + data[d].ObjectName + "'. Skipping this data. This means your scene object names changed, or your old saved game data is not compatible with the current version of your scene or game.");
                }

                foundMatch = false;

            }

        }

        #endregion

        #region DRAWERS

        /// <summary>
        /// Gathers save data for FPEDrawer objects
        /// </summary>
        /// <returns>Array of saveable data for FPEDrawers</returns>
        public FPEDrawerSaveData[] gatherDrawerTypeData()
        {

            FPEDrawer[] allTheDrawers = GameObject.FindObjectsOfType<FPEDrawer>();
            List<FPEDrawerSaveData> drawerData = new List<FPEDrawerSaveData>();

            // We add padding to 0th entry in case there were no doors in the scene that was saved
            drawerData.Add(new FPEDrawerSaveData("PADDING", 0, "", Vector3.zero, false, false));

            for (int d = 0; d < allTheDrawers.Length; d++)
            {
                drawerData.Add(allTheDrawers[d].getSaveGameData());
            }

            return drawerData.ToArray();

        }

        /// <summary>
        /// Restores saved data to FPEDrawers
        /// </summary>
        /// <param name="data">Array of FPEDrawer data from which to restore states</param>
        public void restoreDrawerData(FPEDrawerSaveData[] data)
        {

            FPEDrawer[] drawerObjs = GameObject.FindObjectsOfType<FPEDrawer>();

            bool foundMatch = false;

            // We start at index 1 since we always have a padding entry at the 0th index
            for (int d = 1; d < data.Length; d++)
            {

                for (int i = 0; i < drawerObjs.Length; i++)
                {

                    if (data[d].ObjectName == drawerObjs[i].gameObject.name)
                    {
                        foundMatch = true;
                        drawerObjs[i].restoreSaveGameData(data[d]);
                        break;
                    }

                }

                if (!foundMatch)
                {
                    Debug.LogWarning("FPESaveLoadManager.restoreDrawerData():: Found no match for matching saved Drawer object called '" + data[d].ObjectName + "'. Skipping this data. This means your scene object names changed, or your old saved game data is not compatible with the current version of your scene or game.");
                }

                foundMatch = false;

            }

        }

        #endregion

        #region ATTACHED_NOTE_TYPE

        /// <summary>
        /// Gathers save data for Attached Note type objects
        /// </summary>
        /// <returns>Array of saveable Attached Note data</returns>
        public FPEAttachedNoteSaveData[] gatherAttachedNoteTypeData()
        {

            FPEAttachedNote[] attachedNoteObjs = GameObject.FindObjectsOfType<FPEAttachedNote>();
            FPEAttachedNoteSaveData[] saveData = new FPEAttachedNoteSaveData[attachedNoteObjs.Length];

            for (int n = 0; n < attachedNoteObjs.Length; n++)
            {
                saveData[n] = attachedNoteObjs[n].getSaveGameData();
            }

            return saveData;

        }

        /// <summary>
        /// Restores Attached Note type data
        /// </summary>
        /// <param name="data">Aray of Attached Note data from which to restore</param>
        public void restoreAttachedNoteData(FPEAttachedNoteSaveData[] data)
        {

            FPEAttachedNote[] allAttachedNotes = GameObject.FindObjectsOfType<FPEAttachedNote>();
            bool foundMatch = false;

            for (int d = 0; d < data.Length; d++)
            {

                for (int n = 0; n < allAttachedNotes.Length; n++)
                {

                    if (data[d].ObjectName == allAttachedNotes[n].gameObject.name)
                    {
                        foundMatch = true;
                        allAttachedNotes[n].restoreSaveGameData(data[d]);
                        break;
                    }

                }

                if (!foundMatch)
                {
                    Debug.LogWarning("FPESaveLoadManager.restoreAttachedNoteData():: Found no match for matching saved AttachedNote object called '" + data[d].ObjectName + "'. Skipping this data. This means your scene object names changed, or your old saved game data is not compatible with the current version of your scene or game.");
                }

                foundMatch = false;

            }

        }

        #endregion

        #region AUDIO_DIARY_STATE_TYPE

        /// <summary>
        /// Gathers saveable data from both regular and passive Audio Diary type objects
        /// </summary>
        /// <returns>Array of save data for both of these audio diary types</returns>
        public FPEAudioDiaryPlayedStateSaveData[] gatherAudioDiaryPlayedStateData()
        {

            FPEPassiveAudioDiary[] passiveDiaryObjs = GameObject.FindObjectsOfType<FPEPassiveAudioDiary>();
            FPEInteractableAudioDiaryScript[] diaryObjs = GameObject.FindObjectsOfType<FPEInteractableAudioDiaryScript>();
            FPEAudioDiaryPlayedStateSaveData[] saveData = new FPEAudioDiaryPlayedStateSaveData[passiveDiaryObjs.Length + diaryObjs.Length];

            for (int ad1 = 0; ad1 < passiveDiaryObjs.Length; ad1++)
            {
                saveData[ad1] = passiveDiaryObjs[ad1].getSaveGameData();
            }

            for (int ad2 = 0; ad2 < diaryObjs.Length; ad2++)
            {
                saveData[passiveDiaryObjs.Length + ad2] = diaryObjs[ad2].getSaveGameData();
            }

            return saveData;

        }

        /// <summary>
        /// Restores state for both regular and passive Audio Diary type objects
        /// </summary>
        /// <param name="data">Array of saved data from which to restore</param>
        public void restoreAudioDiaryPlaybackStateData(FPEAudioDiaryPlayedStateSaveData[] data)
        {

            FPEPassiveAudioDiary[] allPassiveDiaries = GameObject.FindObjectsOfType<FPEPassiveAudioDiary>();
            FPEInteractableAudioDiaryScript[] allRegularDiaries = GameObject.FindObjectsOfType<FPEInteractableAudioDiaryScript>();
            bool foundMatch = false;

            for (int d = 0; d < data.Length; d++)
            {

                for (int pd = 0; pd < allPassiveDiaries.Length; pd++)
                {

                    if (data[d].ObjectName == allPassiveDiaries[pd].gameObject.name)
                    {
                        foundMatch = true;
                        allPassiveDiaries[pd].restoreSaveGameData(data[d]);
                        break;
                    }

                }

                // Only check the second set of objects if we didn't find a match in the first set of objects
                if (!foundMatch)
                {

                    for (int rd = 0; rd < allRegularDiaries.Length; rd++)
                    {

                        if (data[d].ObjectName == allRegularDiaries[rd].gameObject.name)
                        {
                            foundMatch = true;
                            allRegularDiaries[rd].restoreSaveGameData(data[d]);
                            break;
                        }

                    }

                }

                if (!foundMatch)
                {
                    Debug.LogWarning("FPESaveLoadManager.restoreAudioDiaryPlaybackStateData():: Found no match for matching saved Audio Diary object called '" + data[d].ObjectName + "'. Skipping this data. This means your scene object names changed, or your old saved game data is not compatible with the current version of your scene or game.");
                }

                foundMatch = false;

            }

        }

        #endregion

        #region JOURNAL_TYPE

        /// <summary>
        /// Gathers save data from all Journals
        /// </summary>
        /// <returns>An array of save data for Journals</returns>
        public FPEJournalSaveData[] gatherJournalSaveData()
        {

            FPEInteractableJournalScript[] allJournals = GameObject.FindObjectsOfType<FPEInteractableJournalScript>();
            FPEJournalSaveData[] saveData = new FPEJournalSaveData[allJournals.Length];

            for (int n = 0; n < allJournals.Length; n++)
            {
                saveData[n] = allJournals[n].getSaveGameData();
            }

            return saveData;

        }

        /// <summary>
        /// Restores all loaded save data for Journals
        /// </summary>
        /// <param name="data">An array of loaded journal data to be restored</param>
        public void restoreJournalData(FPEJournalSaveData[] data)
        {

            FPEInteractableJournalScript[] allJournals = GameObject.FindObjectsOfType<FPEInteractableJournalScript>();
            bool foundMatch = false;

            for (int d = 0; d < data.Length; d++)
            {

                for (int j = 0; j < allJournals.Length; j++)
                {

                    if (data[d].ObjectName == allJournals[j].gameObject.name)
                    {
                        foundMatch = true;
                        allJournals[j].restoreSaveGameData(data[d]);
                        break;
                    }

                }

                if (!foundMatch)
                {
                    Debug.LogWarning("FPESaveLoadManager.restoreJournalData():: Found no match for matching saved FPEInteractableJournalScript object called '" + data[d].ObjectName + "'. Skipping this data. This means your scene object names changed, or your old saved game data is not compatible with the current version of your scene or game.");
                }

                foundMatch = false;

            }

        }

        #endregion

        #region GAME_OPTIONS

        /// <summary>
        /// Gathers saveable data for Game Options (mouse sensitivity, look smoothing, etc.)
        /// </summary>
        /// <returns>Saveable type containing the options data</returns>
        public FPEGameOptionsSaveData gatherGameOptionsSaveData()
        {
            
            FPEGameOptionsSaveData optionsData = new FPEGameOptionsSaveData(FPEInputManager.Instance.LookSensitivity.x,
                                                                            FPEInputManager.Instance.LookSmoothing, 
                                                                            FPEInputManager.Instance.UseGamepad,
                                                                            FPEInputManager.Instance.FlipYAxisMouseOnly,
                                                                            FPEInputManager.Instance.FlipYAxisGamepadOnly);

            return optionsData;

        }

        /// <summary>
        /// Restores the game options to their previously saved values
        /// </summary>
        /// <param name="data">The saved data type from which to restore game options</param>
        public void restoreGameOptionsData(FPEGameOptionsSaveData data)
        {

            FPEInputManager.Instance.LookSensitivity = new Vector2(data.LookSensitivity, data.LookSensitivity);
            FPEInputManager.Instance.LookSmoothing = data.LookSmoothing;
            FPEInputManager.Instance.UseGamepad = data.UseGamepad;
            FPEInputManager.Instance.FlipYAxisMouseOnly = data.FlipYAxisMouseOnly;
            FPEInputManager.Instance.FlipYAxisGamepadOnly = data.FlipYAxisGamepadOnly;

            if (FPEInteractionManagerScript.Instance != null)
            {
                FPEInteractionManagerScript.Instance.changeMouseSensitivityFromMenu(data.LookSensitivity);
            }

        }

        #endregion

        #region GENERIC_SAVE_TYPE

        /// <summary>
        /// Gathers all Generic Save Data from FPEGenericSaveableGameObject type objects in the world
        /// </summary>
        /// <returns>An array of all the generic save data packages</returns>
        public FPEGenericObjectSaveData[] gatherGenericSaveTypeData()
        {

            FPEGenericSaveableGameObject[] allGenericSaveObjects = GameObject.FindObjectsOfType<FPEGenericSaveableGameObject>();
            List<FPEGenericObjectSaveData> doorData = new List<FPEGenericObjectSaveData>();

            // We add padding to 0th entry in case there were no generic saveable objects in the scene that was saved
            doorData.Add(new FPEGenericObjectSaveData("PADDING", 0, 0f, false));

            for (int d = 0; d < allGenericSaveObjects.Length; d++)
            {
                doorData.Add(allGenericSaveObjects[d].getSaveGameData());
            }

            return doorData.ToArray();

        }

        /// <summary>
        /// Restores Generic Save Data to all FPEGenericSaveableGameObject type objects in the world.
        /// </summary>
        /// <param name="data">An array of the previously saved generic data packages, gathered from gatherGenericSaveTypeData()</param>
        public void restoreGenericSaveTypeData(FPEGenericObjectSaveData[] data)
        {

            FPEGenericSaveableGameObject[] genericObjs = GameObject.FindObjectsOfType<FPEGenericSaveableGameObject>();
            bool foundMatch = false;

            // We start at index 1 since we always have a padding entry at the 0th index
            for (int d = 1; d < data.Length; d++)
            {

                for (int i = 0; i < genericObjs.Length; i++)
                {

                    if (data[d].SavedName == genericObjs[i].gameObject.name)
                    {
                        foundMatch = true;
                        genericObjs[i].restoreSaveGameData(data[d]);
                        break;
                    }

                }

                if (!foundMatch)
                {
                    Debug.LogWarning("FPESaveLoadManager.restoreGenericSaveTypeData():: Found no match for matching saved FPEGenericSaveableGameObject called '" + data[d].SavedName + "'. Skipping this data. This means your scene object names changed, or your old saved game data is not compatible with the current version of your scene or game.");
                }

                foundMatch = false;

            }

        }

        #endregion


        #region CUSTOM_SAVE_LOAD_LOGIC

        //
        // Logic for Gathering and Restoring your custom saveable types goes here
        //

        #endregion

    }

}