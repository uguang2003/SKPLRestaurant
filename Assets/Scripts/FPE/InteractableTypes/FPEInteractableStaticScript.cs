using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEInteractableStaticScript
    // This script is for Static type Interactable objects. It simple performs the base
    // highlight and interaction text for the object. These objects cannot be picked up 
    // or moved or interacted with aside from "discovered"/"looked at".
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInteractableStaticScript : FPEInteractableBaseScript
    {

        [SerializeField, Tooltip("If true, player can interact with this while holding something. If false, they cannot.")]
        protected bool canInteractWithWhileHoldingObject = true;

        public override void Awake()
        {

            base.Awake();
            interactionType = eInteractionType.STATIC;

        }

        public override bool interactionsAllowedWhenHoldingObject()
        {
            return canInteractWithWhileHoldingObject;
        }

    }

}