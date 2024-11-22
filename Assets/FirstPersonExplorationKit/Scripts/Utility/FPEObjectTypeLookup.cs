using System.Collections.Generic;
using UnityEngine;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPEObjectTypeLookup
    // This class contains all Resource path, string delimiter, and LUT information for 
    // saving and loading Pickup and Inventory Item type objects in the game. 
    //
    // It must be maintained with additions or deletions of Pickup and Inventory Item types prefabs.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEObjectTypeLookup
    {

        public static readonly string InventoryResourcePath = "InventoryItems/";
        public static readonly string PickupResourcePath = "Pickups/";
        public static readonly string AudioDiaryResourcePath = "AudioDiaryAudioClips/";
        public static readonly char[] PickupPrefabDelimiter = { ' ', '(' };

        private Dictionary<FPEInventoryManagerScript.eInventoryItems, string> inventoryItemsLookup;
        public Dictionary<FPEInventoryManagerScript.eInventoryItems, string> InventoryItemsLookup {
            get { return inventoryItemsLookup; }
        }

        public FPEObjectTypeLookup()
        {

            //
            // IMPORTANT: This table must be maintained in order to ensure saving and loading game data can work correctly.
            //
            inventoryItemsLookup = new Dictionary<FPEInventoryManagerScript.eInventoryItems, string>();

            try
            {

                inventoryItemsLookup.Add(FPEInventoryManagerScript.eInventoryItems.APPLE, "demoApple");
                inventoryItemsLookup.Add(FPEInventoryManagerScript.eInventoryItems.BATTERY, "demoBattery");
                inventoryItemsLookup.Add(FPEInventoryManagerScript.eInventoryItems.KEYCARD, "demoKeycard");
                inventoryItemsLookup.Add(FPEInventoryManagerScript.eInventoryItems.PUZZLEBALL, "demoPuzzleBall");
                inventoryItemsLookup.Add(FPEInventoryManagerScript.eInventoryItems.COLLECTIBLE, "demoCollectible");
                inventoryItemsLookup.Add(FPEInventoryManagerScript.eInventoryItems.SIMPLEKEY, "demoSimpleKey");

                // 
                // To add a new item, simply add a new dictionary entry for that eInventoryItems type and 
                // string, where string is the name of the prefab inside the InventoryItems Resources sub-folder
                // For example, if you had a custom inventory item with enum name "MY_CUSTOM_INV_A", with a prefab 
                // named "myCustomInventoryItem.prefab", your entry would be: 
                // inventoryItemsLookup.Add(FPEInventoryManagerScript.eInventoryItems.MY_CUSTOM_INV_A, "myCustomInventoryItem");
                //




            }
            catch (Exception e)
            {
                Debug.LogError("FPEObjectTypeLookup:: Something went wrong when building the inventoryItemsLookup (Null argument or duplicate). Double check dictionary building for these issues. Reason: " +  e.Message);
            }

        }

    }

}