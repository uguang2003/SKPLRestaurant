using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Whilefun.FPEKit {

    //
    // FPEInventoryManagerScript
    // This script handles all player inventory items and inventory state.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInventoryManagerScript : MonoBehaviour {

        // To add new inventory items, simply add them to the enum:
        // E.g. add "MY_NEW_ITEM=N"
        //
        // Note: KEYCARD and other demo/defaults can be replaced or modified.
        public enum eInventoryItems {

            KEYCARD = 0,
            BATTERY = 1,
            PUZZLEBALL = 2,
            APPLE = 3,
            COLLECTIBLE = 4,
            SIMPLEKEY = 5

        };

        // Inventory Items
        private List<FPEInteractableInventoryItemScript> inventoryItems;
        private int[] inventoryQuantities;
        // Audio Logs and Notes
        private List<FPEAudioDiaryEntry> audioDiaries;
        private List<FPENoteEntry> notes;

        private static FPEInventoryManagerScript _instance;
        public static FPEInventoryManagerScript Instance {
            get { return _instance; }
        }
        
        void Awake() {

            if (_instance != null)
            {

                Debug.LogWarning("FPEInventoryManagerScript:: Duplicate instance of FPEInventoryManagerScript, deleting second one.");
                Destroy(this.gameObject);

            }
            else
            {

                _instance = this;
                DontDestroyOnLoad(this.gameObject);

                inventoryItems = new List<FPEInteractableInventoryItemScript>();
                inventoryQuantities = new int[Enum.GetNames(typeof(eInventoryItems)).Length];

                audioDiaries = new List<FPEAudioDiaryEntry>();
                notes = new List<FPENoteEntry>();

            }

        }

        #region INVENTORY_ITEMS

        /// <summary>
        /// Returns quantity of specified inventory item type. If there are no such items in the 
        /// inventory, zero is returned;
        /// </summary>
        /// <param name="itemToCheck">The type of item for which quantity will be checked</param>
        /// <returns></returns>
        public int inventoryQuantity(FPEInventoryManagerScript.eInventoryItems itemToCheck)
        {
            return inventoryQuantities[(int)itemToCheck];
        }


        /// <summary>
        /// Removes and Destroys quantity items of type itemType. Assumes a prior check is done using 
        /// inventoryQuantity(). If there is not sufficient quantity, an error is printed and nothing 
        /// else happens.
        /// </summary>
        /// <param name="itemType">The type of item to remove and destroy</param>
        /// <param name="quantity">The quantity of said item to remove and destroy</param>
        public void destroyInventoryItemsOfType(eInventoryItems itemType, int quantity)
        {

            if(inventoryQuantities[(int)itemType] < quantity)
            {
                Debug.LogError("FPEInventoryManagerScript.destroyInventoryItemsOfType() asked to destroy "+quantity+" items of type '"+ itemType + "', but there are only "+ inventoryQuantities[(int)itemType] + " items. Doing nothing.");
            }
            else
            {

                GameObject tempObject = null;

                for(int q = 0; q < quantity; q++)
                {

                    foreach(FPEInteractableInventoryItemScript tempItem in inventoryItems) { 

                        if(tempItem.InventoryItemType == itemType)
                        {

                            // If they stack, its a special case
                            if (tempItem.Stackable)
                            {

                                // If it's the last one, destroy it. 
                                if (inventoryQuantities[(int)itemType] == 1)
                                {

                                    tempObject = tempItem.gameObject;
                                    inventoryItems.Remove(tempItem);
                                    Destroy(tempObject);
                                    inventoryQuantities[(int)itemType]--;
                                    break;

                                }
                                // Otherwise, just reduce the quantity.
                                else
                                {

                                    inventoryQuantities[(int)itemType]--;
                                    break;

                                }

                            }
                            // Else, just delete the number required.
                            else
                            {

                                tempObject = tempItem.gameObject;
                                inventoryItems.Remove(tempItem);
                                Destroy(tempObject);
                                inventoryQuantities[(int)itemType]--;
                                break;

                            }

                        }

                    }

                }

            }

        }

        
        /// <summary>
        /// Gives specified quanity of specified inventory item type. Returns true if we need to keep the 
        /// original around. False if we can discard it (basically if it was stackable, we fake it)
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>True if we need to keep the instance we added in the world. False if we do 
        /// not (e.g. if was stackable and we already had some we ddon't need to keep the 2nd to nth 
        /// ones)</returns>
        public bool addInventoryItem(FPEInteractableInventoryItemScript item) {

            bool needToKeepInstanceInWorld = true;

            if (item.Stackable)
            {

                if (inventoryQuantities[(int)item.InventoryItemType] == 0)
                {
                    inventoryItems.Add(item);
                }
                else
                {
                    needToKeepInstanceInWorld = false;
                }

            }
            else
            {
                inventoryItems.Add(item);
            }

            inventoryQuantities[(int)item.InventoryItemType]++;

            return needToKeepInstanceInWorld;

        }


        /// <summary>
        /// Removes item from inventory based on instance ID (e.g. if calling from inventory screen)
        /// </summary>
        /// <param name="instanceIDToRemove">The game object instance ID to remove</param>
        /// <returns></returns>
        private FPEInteractableInventoryItemScript removeInventoryItem(int instanceIDToRemove)
        {

            FPEInteractableInventoryItemScript itemToDrop = null;

            for (int i = 0; i < inventoryItems.Count; i++)
            {

                if (inventoryItems[i].gameObject.GetInstanceID() == instanceIDToRemove)
                {

                    itemToDrop = inventoryItems[i];

                    if (itemToDrop.Stackable)
                    {

                        // If we have exactly one left, then we need to remove the actual one we have in the list from the list.
                        if (inventoryQuantities[(int)itemToDrop.InventoryItemType] == 1)
                        {
                            inventoryItems.RemoveAt(i);
                        }
                        // Otherwise, we have more than one left. We need to create a clone of the only one we actually have in our list, and use that.
                        else
                        {

                            GameObject clone = Instantiate(itemToDrop.gameObject);
                            clone.name = itemToDrop.name;
                            itemToDrop = clone.GetComponent<FPEInteractableInventoryItemScript>();

                        }

                    }
                    else
                    {
                        inventoryItems.RemoveAt(i);
                    }

                    inventoryQuantities[(int)itemToDrop.InventoryItemType]--;
                    break;

                }

            }

            return itemToDrop;

        }


        /// <summary>
        /// Removes specified item if it exists. Also tells gives the item to FPEInteractionManager to drop into the world.
        /// </summary>
        /// <param name="instanceIDToDrop">The GameObject instance ID of the inventory item to drop.</param>
        /// <returns>Amount of this item left in inventory post-removal</returns>
        public int dropItemFromInventory(int instanceIDToDrop)
        {

            int howManyLeftAfterRemoval = -1;
            FPEInteractableInventoryItemScript itemToDrop = removeInventoryItem(instanceIDToDrop);

            if (itemToDrop != null)
            {

                howManyLeftAfterRemoval = inventoryQuantities[(int)itemToDrop.InventoryItemType];
                FPEInteractionManagerScript.Instance.dropObjectFromInventory(itemToDrop);

            }
            else
            {
                Debug.LogError("FPEInventoryManagerScript.dropItemFromInventory() could not find inventory item with instanceID '"+instanceIDToDrop+"'");
            }

            return howManyLeftAfterRemoval;

        }

        /// <summary>
        /// Removes specified item if it exists. Also tells FPEInteractionManager to place item into player's hand
        /// </summary>
        /// <param name="instanceIDToDrop">The GameObject instance ID of the inventory item to drop.</param>
        /// <returns>Amount of this item left in inventory post-removal</returns>
        public int holdItemFromInventory(int instanceIDToHold)
        {

            int howManyLeftAfterRemoval = -1;
            FPEInteractableInventoryItemScript itemToHold = removeInventoryItem(instanceIDToHold);
            
            if (itemToHold != null)
            {

                howManyLeftAfterRemoval = inventoryQuantities[(int)itemToHold.InventoryItemType];
                FPEInteractionManagerScript.Instance.holdObjectFromInventory(itemToHold);

            }
            else
            {
                Debug.LogError("FPEInventoryManagerScript.holdItemFromInventory() could not find inventory item with instanceID '" + instanceIDToHold + "'");
            }

            return howManyLeftAfterRemoval;

        }

        /// <summary>
        /// Removes specified item if it exists. Also tells FPEInteractionManager to execute the 'consume' script
        /// to ensure the item's effects take place in the game world.
        /// </summary>
        /// <param name="instanceIDToConsume">The GameObject instance ID of the inventory item to drop.</param>
        /// <returns>Amount of this item left in inventory post-removal</returns>
        public int consumeItemFromInventory(int instanceIDToConsume)
        {

            int howManyLeftAfterRemoval = -1;
            FPEInteractableInventoryItemScript itemToConsume = removeInventoryItem(instanceIDToConsume);

            if (itemToConsume != null)
            {

                howManyLeftAfterRemoval = inventoryQuantities[(int)itemToConsume.InventoryItemType];
                FPEInteractionManagerScript.Instance.consumeObjectFromInventory(itemToConsume);

            }
            else
            {
                Debug.LogError("FPEInventoryManagerScript.consumeItemFromInventory() could not find inventory item with instanceID '" + instanceIDToConsume + "'");
            }

            return howManyLeftAfterRemoval;

        }

        /// <summary>
        /// Returns an array of FPEInventoryItemData representing the items currently in 
        /// inventory. Note that stackable items are only represented by ONE entry, but 
        /// their quantity represents how many are in the stack.
        /// </summary>
        /// <returns>Array of FPEInventoryItemData representing the items currently in 
        /// inventory. Can be used to visually represent inventory on hand.</returns>
        public FPEInventoryItemData[] getInventoryData()
        {

            FPEInventoryItemData[] data = new FPEInventoryItemData[inventoryItems.Count];
            sortInventory();

            for (int i = 0; i < inventoryItems.Count; i++)
            {

                data[i] = new FPEInventoryItemData
                    (
                        inventoryItems[i].ItemName,
                        inventoryItems[i].ItemImage,
                        inventoryItems[i].ItemDescription,
                        inventoryItems[i].Stackable,
                        inventoryQuantities[(int)inventoryItems[i].InventoryItemType],
                        inventoryItems[i].CanBeHeld,
                        inventoryItems[i].CanBeDropped,
                        inventoryItems[i].CanBeConsumed,
                        inventoryItems[i].gameObject.GetInstanceID()
                    );

            }

            return data;

        }

        // Replace this with your own sorting method as desired. Perhaps you want to sort by name, quantity, or have 
        // multple sorting types that the player can choose.
        private void sortInventory()
        {
            inventoryItems.Sort((p, q) => p.InventoryItemType.CompareTo(q.InventoryItemType));
        }

        #endregion


        #region AUDIO_DIARIES

        /// <summary>
        /// Adds an audio diary entry to inventory
        /// </summary>
        /// <param name="diary">The diary to add.</param>
        public void addAudioDiaryEntry(FPEAudioDiaryEntry diary)
        {
            if (diary != null)
            {
                audioDiaries.Add(diary);
            }
        }

        /// <summary>
        /// Festches audio diary data, for use when visually representing collected 
        /// diaries for things like inventory screens.
        /// </summary>
        /// <returns>An array of diary data</returns>
        public FPEAudioDiaryData[] getAudioDiaryData()
        {

            FPEAudioDiaryData[] data = new FPEAudioDiaryData[audioDiaries.Count];
            
            // Sort by title
            //audioDiaries.Sort((p, q) => p.DiaryTitle.CompareTo(q.DiaryTitle));

            for (int i = 0; i < audioDiaries.Count; i++)
            {
                data[i] = new FPEAudioDiaryData(audioDiaries[i].DiaryTitle);
            }

            return data;

        }

        /// <summary>
        /// Plays back diary at specified index
        /// </summary>
        /// <param name="diaryIndex"></param>
        public void playbackAudioDiary(int diaryIndex)
        {

            if (diaryIndex >= 0 && diaryIndex < audioDiaries.Count)
            {
                FPEInteractionManagerScript.Instance.playAudioDiaryEntry(audioDiaries[diaryIndex]);
            }
            else
            {
                Debug.LogError("FPEInventoryManagerScript.playbackAudioDiary() given bad index of '"+diaryIndex+"'");
            }

        }

        /// <summary>
        /// Halts all diary playback
        /// </summary>
        public void stopAllDiaryPlayback()
        {
            FPEInteractionManagerScript.Instance.stopAllDiaryPlayback();
        }

        #endregion


        #region NOTES

        /// <summary>
        /// Adds a note entry to the inventory
        /// </summary>
        /// <param name="note">The note entry to add</param>
        public void addNoteEntry(FPENoteEntry note)
        {
            if (note != null)
            {
                notes.Add(note);
            }
        }

        /// <summary>
        /// Fetches data about notes in the inventory for visually representing 
        /// notes on hand in things like the inventory screen.
        /// </summary>
        /// <returns>An array of note data</returns>
        public FPENoteData[] getNoteData()
        {

            FPENoteData[] data = new FPENoteData[notes.Count];

            // Sort by title
            //notes.Sort((p, q) => p.NoteTitle.CompareTo(q.NoteTitle));

            for (int i = 0; i < notes.Count; i++)
            {
                data[i] = new FPENoteData(notes[i].NoteTitle, notes[i].NoteBody);
            }

            return data;

        }

        #endregion


        #region DATA_SAVE_AND_LOAD

        //
        // These functions serve as an interface for saving and loading game data for player inventory
        //
        // NOTE: It is very important that these functions not be called while the player is in control 
        // of the game, as results will not be predictable. Calling these functions should be reserved 
        // strictly for use in a saving or loading screen.
        //

        public List<FPEInteractableInventoryItemScript> getInventoryItems()
        {
            return inventoryItems;
        }

        public void setInventoryItems(List<FPEInteractableInventoryItemScript> items)
        {
            inventoryItems = items;
        }

        public int[] getInventoryQuantities()
        {
            return inventoryQuantities;
        }

        public void setInventoryQuantities(int[] quantities)
        {
            inventoryQuantities = quantities;
        }

        public FPENoteEntry[] getNoteDataForSavedGame()
        {
            return notes.ToArray();
        }

        public void setNoteDataFromSavedGame(List<FPENoteEntry> loadedNotes)
        {
            notes = loadedNotes;
        }

        public FPEAudioDiaryEntry[] getAudioDiaryDataForSavedGame()
        {
            return audioDiaries.ToArray();
        }

        public void setAudioDiaryDataFromSavedGame(List<FPEAudioDiaryEntry> loadedAudioDiaries)
        {
            audioDiaries = loadedAudioDiaries;
        }

        /// <summary>
        /// Destroys all Inventory Item objects in the world, and resets inventory items and quantities
        /// </summary>
        public void clearInventoryItems()
        {

            foreach(FPEInteractableInventoryItemScript item in inventoryItems)
            {
                Destroy(item.gameObject);
            }

            inventoryItems.Clear();

            for(int i = 0; i < inventoryQuantities.Length; i++)
            {
                inventoryQuantities[i] = 0;
            }

        }

        /// <summary>
        /// Clears all Audio Diaries and Notes from inventory
        /// </summary>
        public void clearAudioDiariesAndNotes()
        {

            audioDiaries.Clear();
            notes.Clear();

        }

        #endregion

    }

}