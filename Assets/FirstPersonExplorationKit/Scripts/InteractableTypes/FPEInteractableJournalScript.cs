using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEInteractableJournalScript
    // This script is for Journal type Interactable objects. In addition to base
    // functionality, these objects can be triggered to open the journal UI, and 
    // display the assigned journal pages.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInteractableJournalScript : FPEInteractableBaseScript
    {

        [Tooltip("You can optionally give the journal a new interaction string once it has been read (e.g. 'Some crumpled paper' becomes 'That old note from Grandma'). If left blank, the interaction string will remain unchanged.")]
        public string postReadInteractionString = "";

        [Header("Journal Pages")]
        [Tooltip("The journal pages that will be readable when the journal is opened. Must be 1 or more pages.")]
        public Sprite[] journalPages;

        private bool hasBeenRead = false;

        public override void Awake()
        {

            base.Awake();
            interactionType = eInteractionType.JOURNAL;
            // Journals require both hands to read
            //canInteractWithWhileHoldingObject = false;

        }

        // Note: we disable the collider when activated so UI mouse events don't interfere
        // If you wanted to make a similar class that "consumed" the journals when found,
        // you could instead Destroy the game object here and set a variable in another script.
        public void activateJournal()
        {

            base.interact();
            gameObject.GetComponent<Collider>().enabled = false;

        }

        public void deactivateJournal()
        {

            hasBeenRead = true;

            if (postReadInteractionString != "")
            {
                interactionString = postReadInteractionString;
            }
            gameObject.GetComponent<Collider>().enabled = true;

        }

        // Journals always require two hands to read
        public override bool interactionsAllowedWhenHoldingObject()
        {
            return false;
        }

        public FPEJournalSaveData getSaveGameData()
        {
            return new FPEJournalSaveData(gameObject.name, hasBeenRead);
        }

        public void restoreSaveGameData(FPEJournalSaveData data)
        {

            hasBeenRead = data.HasBeenRead;

            if (hasBeenRead && postReadInteractionString != "")
            {
                interactionString = postReadInteractionString;
            }

        }

    }

}