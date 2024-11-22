using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEInventoryItemData
    // A basic data container class that allows for data to be passed from Game 
    // Systems (e.g. Inventory Manager) to the UI for display to the player.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInventoryItemData
    {

        private string _itemName = "";
        private Sprite _itemImage;
        private string _itemDescription = "";
        private bool _stackable = false;
        private int _quantity = 1;
        private bool _canBeHeld = false;
        private bool _canBeDropped = false;
        private bool _canBeConsumed = false;
        private int _gameObjectInstanceID = -1;

        public string ItemName {
            get { return _itemName; }
        }

        public Sprite ItemImage {
            get { return _itemImage; }
        }

        public string ItemDescription {
            get { return _itemDescription; }
        }

        public bool Stackable {
            get { return _stackable; }
        }

        public int Quantity {
            get { return _quantity; }
        }

        public bool CanBeHeld {
            get { return _canBeHeld; }
        }

        public bool CanBeDropped {
            get { return _canBeDropped; }
        }

        public bool CanBeConsumed {
            get { return _canBeConsumed; }
        }

        public int GameObjectInstanceID {
            get { return _gameObjectInstanceID; }
        }

        public FPEInventoryItemData(string itemName, Sprite itemImage, string itemDescription, bool stackable, int quantity, bool canBeHeld, bool canBeDropped, bool canBeConsumed, int instanceID)
        {

            _itemName = itemName;
            _itemImage = itemImage;
            _itemDescription = itemDescription;
            _stackable = stackable;
            _quantity = quantity;
            _canBeHeld = canBeHeld;
            _canBeDropped = canBeDropped;
            _canBeConsumed = canBeConsumed;
            _gameObjectInstanceID = instanceID;

        }

    }

}