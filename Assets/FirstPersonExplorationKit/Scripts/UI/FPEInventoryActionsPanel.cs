using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEInventoryActionsPanel
    // A small panel that houses action buttons, and is displayed when the
    // player clicks on a selected inventory item slot.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInventoryActionsPanel : MonoBehaviour
    {

        private FPEMenuButton holdButton;
        private FPEMenuButton dropButton;
        private FPEMenuButton consumeButton;
        private FPEMenuButton cancelButton;

        void Awake()
        {

            holdButton = transform.Find("HoldButton").GetComponent<FPEMenuButton>();
            dropButton = transform.Find("DropButton").GetComponent<FPEMenuButton>();
            consumeButton = transform.Find("ConsumeButton").GetComponent<FPEMenuButton>();
            cancelButton = transform.Find("CancelButton").GetComponent<FPEMenuButton>();

            if (!holdButton || !dropButton || !consumeButton || !cancelButton)
            {
                Debug.LogError("FPEInventoryActionsPanel:: One of the Hold, Drop, or Consume buttons are missing! Did you rename or remove them?");
            }
            
        }

        public void setButtonStates(bool canHold, bool canDrop, bool canConsume)
        {

            holdButton.setButtonInteractionState(canHold);
            dropButton.setButtonInteractionState(canDrop);
            consumeButton.setButtonInteractionState(canConsume);

        }

        public FPEMenuButton getFirstPermittedActionButton()
        {

            FPEMenuButton firstPermittedButton = cancelButton;

            if (holdButton.IsInteractable())
            {
                firstPermittedButton = holdButton;
            }
            else if (dropButton.IsInteractable())
            {
                firstPermittedButton = dropButton;
            }
            else if (consumeButton.IsInteractable())
            {
                firstPermittedButton = consumeButton;
            }

            return firstPermittedButton;

        }

    }

}
