using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEInteractableInventoryItemScript
    // This script is the basis for all Inventory items. To create
    // a new inventory item in the world, add this script, and choose
    // the Inventory Type in the Inspector.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [RequireComponent(typeof(AudioSource))]
    public class FPEInteractableInventoryItemScript : FPEInteractablePickupScript
    {

        [Header("Inventory Type and Quantity")]
        [Tooltip("The type of inventory item this is.")]
        [SerializeField]
        private FPEInventoryManagerScript.eInventoryItems inventoryItemType = FPEInventoryManagerScript.eInventoryItems.KEYCARD;
        public FPEInventoryManagerScript.eInventoryItems InventoryItemType { get { return inventoryItemType; } }
        [SerializeField, Tooltip("The number of items of this type to give in inventory when this item is picked acquired (E.g. Box of 4 batteries would be 4). Default value is 1.")]
        private int inventoryQuantity = 1;
        public int InventoryQuantity { get { return inventoryQuantity; } }

        [Header("Additional Sounds")]
        [Tooltip("Inventory Get sound (optional). This sound is played when the inventory item is grabbed by the player. If no sound is specified, the generic inventory sound will be used instead.")]
        [SerializeField]
        private AudioClip inventoryGetSound = null;
        public AudioClip InventoryGetSound { get { return inventoryGetSound; } }

        [Header("Interaction Options")]
        [SerializeField, Tooltip("If true, player can pick this up to examine it. If false, they can only move object from world position into inventory. Note: for items where this is false, Interaction String should reflect this (e.g. use 'Take' rather than 'Pickup')")]
        private bool pickupPermitted = true;
        public bool PickupPermitted { get { return pickupPermitted; } }
        [Header("Permitted Actions")]
        [SerializeField, Tooltip("If true, the player can move item from inventory into their hand.")]
        private bool canBeHeld = false;
        public bool CanBeHeld { get { return canBeHeld; } }
        [SerializeField, Tooltip("If true, the player can drop from their inventory onto the ground")]
        private bool canBeDropped = false;
        public bool CanBeDropped { get { return canBeDropped; } }
        [SerializeField, Tooltip("If true, the player can consume (eat, use, etc.) the item from inventory.")]
        private bool canBeConsumed = false;
        public bool CanBeConsumed { get { return canBeConsumed; } }

        [Header("Inventory Screen Information")]
        [SerializeField, Tooltip("The short name or 'title' of the inventory item.")]
        private string itemName = "New Inventory Item";
        public string ItemName { get { return itemName; } }
        [SerializeField, Tooltip("The image to represent the inventory item on the inventory screen.")]
        private Sprite itemImage = null;
        public Sprite ItemImage { get { return itemImage; } }
        [SerializeField, Tooltip("The long description of the item.")]
        [TextArea]
        private string itemDescription = "Item Description Here";
        public string ItemDescription { get { return itemDescription; } }
        [SerializeField, Tooltip("If true, all items of this type will be stacked into a single slot.")]
        private bool stackable = false;
        public bool Stackable { get { return stackable; } }

        private FPEInventoryConsumer myConsumer = null;
        private bool hasBeenConsumed = false;

        public override void Awake()
        {

            base.Awake();
            interactionType = eInteractionType.INVENTORY;

            if (enableSounds)
            {

                // If no impact sounds are specified, just use the generic one
                if (!inventoryGetSound)
                {
                    inventoryGetSound = Resources.Load("genericInventoryGet") as AudioClip;
                }

            }

            if (!gameObject.GetComponent<Rigidbody>())
            {
                Debug.LogError("FPEInteractableInventoryItemScript:: Object '"+gameObject.name+"' is missing a RigidBody. Inventory Items must have a RigidBody componenent. If you are upgrading from v1.2 of First Person Exploration Kit, you may have forgot to add one as part of your project migration.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }

        }

        public override void Start()
        {

            base.Start();

            if (canBeConsumed)
            {

                myConsumer = gameObject.GetComponent<FPEInventoryConsumer>();

                if (!myConsumer)
                {
                    Debug.LogError("FPEInteractableInventoryItemScript:: Object '"+gameObject.name+"' has no FPEInventoryConsumer attached, but is marked as consumable! Consuming this item will cause errors.", gameObject);
                }

            }

        }

        public override void Update()
        {

            base.Update();

            if (hasBeenConsumed)
            {

                if (enableSounds)
                {

                    if (!gameObject.GetComponent<AudioSource>().isPlaying)
                    {
                        Destroy(gameObject);
                    }

                }
                else
                {
                    Destroy(gameObject);
                }

            }

        }

        // TODO: Remove this functon in subsequent update
        [System.Obsolete("getInventoryItemType() is deprecated, please use InventoryItemType instead.", true)]
        public FPEInventoryManagerScript.eInventoryItems getInventoryItemType()
        {
            return inventoryItemType;
        }
        // TODO: Remove this functon in subsequent update
        [System.Obsolete("getInventoryQuantity() is deprecated, please use InventoryQuantity instead.", true)]
        public int getInventoryQuantity()
        {
            return inventoryQuantity;
        }

        // Called when inventory item is "grabbed". Here you can do things like change it's appearance,
        // play sounds, etc.
        // E.g. Coin and Question Mark Blocks in mario turn slightly darker when they are emptied
        public void consumeInventoryItem()
        {

            if(canBeConsumed && !hasBeenConsumed)
            {
                myConsumer.consumeItem();
            }

        }

    }

}