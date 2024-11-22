
namespace Whilefun.FPEKit
{

    //
    // FPEHUDData
    // This script serves as a data package supplied for HUD refreshes, based on player's current interaction state.
    //
    // Copyright 2018 While Fun Games
    // http://whilefun.com
    //
    public class FPEHUDData
    {

        // GENERAL //
        public bool examiningObject = false;
        public bool zoomedIn = false;

        // DOCK //
        public bool dockedRightNow = false;
        public bool dockTransitionHappeningRightNow = false;
        public string currentDockHint = "";
        public string currentUndockHint = "";

        // WHAT IS PLAYER HOLDING //
        public FPEInteractableBaseScript.eInteractionType heldType = FPEInteractableBaseScript.eInteractionType.NULL_TYPE;

        // Held object info
        public string heldObjectInteractionString = "";
        public bool heldObjectinteractionsAllowedWhenHoldingObject = false;
        public string heldObjectInventoryItemName = "";

        // WHAT IS PLAYER LOOKING AT //
        public FPEInteractableBaseScript.eInteractionType lookedAtType = FPEInteractableBaseScript.eInteractionType.NULL_TYPE;

        public string lookedAtInteractionString = "";
        public bool usingCustomLookedAtInteractionString = false;

        // Inventory
        public bool lookedAtInventoryPickupPermitted = false;
        public string lookedAtInventoryItemName = "";
        
        // Dock
        public string lookedAtDockHint = "";
        public bool lookedAtDockOccupied = false;

        // Pickup
        public bool lookedAtPickupInteractionsAllowedWhenHoldingObject = false;
        public string lookedAtPickupPutbackString = "";

        // Activate
        public bool lookedAtActivateAllowedWhenHoldingObject = false;

        // Audio Diary
        public bool lookedAtAudioDiaryAutoPlay = false;

        // AUDIO DIARY PLAYBACK //
        public bool audioDiaryPlayingRightNow = false;
        public string audioDiaryTitle = "";
        public bool audioDiaryIsReplay = false;

    }

}