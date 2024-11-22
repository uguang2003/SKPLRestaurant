using UnityEngine;
using UnityEngine.Events;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPEInteractableDockScript
    // This script allows for the creation of "docks" where the player can sit, stand, etc. and 
    // continue to interact with the world. This is useful for things like computer terminals, 
    // armchairs, arcade cabinets, periscopes, or anything else you can think of.
    //
    // Use: 1) Place this script on an empty game object, and add a box collider or other primitive 
    //         collider. This collider should be a trigger. It will be used to activate the dock.
    //      2) Place two empty child game objects: DockTransform and FocusTransform
    //      3) In the inspector, assign these two transforms to their associated variable slots
    //      4) Position the dock to be the location the player will be locked to, and place
    //         the focus where you want the player's initial focus to be. The view limits will
    //         be relative to this initial focus.
    //      5) Place models or other set pieces as other child objects to complete the dock look 
    //         and feel.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInteractableDockScript : FPEInteractableBaseScript
    {

        [SerializeField, Tooltip("If true, player can interact with this while holding something. If false, they cannot.")]
        protected bool canInteractWithWhileHoldingObject = true;

        [Header("Dock Specific Values")]
        [SerializeField, Tooltip("The transform where the player will be moved when they dock")]
        private Transform myDockTransform = null;
        public Transform DockTransform { get { return myDockTransform; } }

        [SerializeField, Tooltip("The transform where the player will focus when they dock.")]
        private Transform myFocusTransform = null;
        public Transform FocusTransform { get { return myFocusTransform; } }

        [SerializeField, Tooltip("The width and height of the restricted view area. Larger means player has more view freedom. In exact units, should be tweaked through testing with focus transform position, distance from dock transform, etc.")]
        private Vector2 dockedViewLimits = new Vector2(45.0f, 45.0f);
        public Vector2 DockedViewLimits { get { return dockedViewLimits; } }

        [SerializeField, Tooltip("If true, docking will be smoothed. Based on smoothing values found on the FPEFirstPersonController prefab.")]
        private bool smoothDock = true;
        public bool SmoothDock { get { return smoothDock; } }

        [SerializeField, Tooltip("Text hint that is applied to the UI when player looks at this dock object")]
        private string dockHintText = "Sit";
        public string DockHintText {  get { return dockHintText; } }
        [SerializeField, Tooltip("Text hint that is applied to the UI when player is docked (e.g. 'stand up')")]
        private string unDockHintText = "Get up";
        public string UnDockHintText {  get { return unDockHintText; } }

        [Header("Sound")]
        [SerializeField, Tooltip("If true, docking/undocking will make sound")]
        private bool playDockSounds = true;
        [SerializeField, Tooltip("Sound to play when docking")]
        private AudioClip dockingSound = null;
        [SerializeField, Tooltip("Sound to play when un-docking")]
        private AudioClip unDockingSound = null;

        private AudioSource dockAudio = null;

        private bool occupied = false;
        public bool Occupied {
            get { return occupied; }
        }

        private Collider[] myColliders = null;

        [Serializable]
        public class DockEvent : UnityEvent { }
        [SerializeField, Tooltip("If specified, this event will fire when DOCKING is started (e.g. pull chair out from table)")]
        private DockEvent DockStartedEvent = null;
        [SerializeField, Tooltip("If specified, this event will fire when DOCKING is complete (e.g. turn on computer screen)")]
        private DockEvent DockFinishedEvent = null;
        [SerializeField, Tooltip("If specified, this event will fire when UNDOCKING is started (e.g. turn off computer screen)")]
        private DockEvent UnDockStartedEvent = null;
        [SerializeField, Tooltip("If specified, this event will fire when UNDOCKING is complete (e.g. tuck chair under table)")]
        private DockEvent UnDockFinishedEvent = null;

#if UNITY_EDITOR    
        [Header("Editor")]
        [SerializeField, Tooltip("If true, Gizmos will be drawn in editor to aid with layout and testing")]
        private bool drawDockGizmos = true;
#endif

        public override void Awake()
        {

            base.Awake();

            interactionType = eInteractionType.DOCK;
            //canInteractWithWhileHoldingObject = true;

            if(myDockTransform == null || myFocusTransform == null)
            {
                Debug.LogError("FPEInteractableDockScript:: Object '"+gameObject.name+"' does not have a dock transform or focus transform defined. Docking here will not work!");
            }

            myColliders = gameObject.GetComponents<Collider>();

            dockAudio = gameObject.GetComponent<AudioSource>();

            if(dockAudio == null)
            {
                dockAudio = gameObject.AddComponent<AudioSource>();
                dockAudio.playOnAwake = false;
                dockAudio.loop = false;
            }

        }

        public override void Start()
        {
            base.Start();
        }

        public override bool interactionsAllowedWhenHoldingObject()
        {
            return canInteractWithWhileHoldingObject;
        }

        public void dock()
        {

            base.interact();

            occupied = true;

            for(int c = 0; c < myColliders.Length; c++)
            {
                myColliders[c].enabled = false;
            }

            if (playDockSounds)
            {
                dockAudio.clip = dockingSound;
                dockAudio.Play();
            }

            if(DockStartedEvent != null)
            {
                DockStartedEvent.Invoke();
            }

        }

        public void unDock()
        {

            occupied = false;

            for (int c = 0; c < myColliders.Length; c++)
            {
                myColliders[c].enabled = true;
            }

            if (playDockSounds)
            {
                dockAudio.clip = unDockingSound;
                dockAudio.Play();
            }

            if(UnDockStartedEvent != null)
            {
                UnDockStartedEvent.Invoke();
            }

        }

        public void finishDocking()
        {
            if(DockFinishedEvent != null)
            {
                DockFinishedEvent.Invoke();
            }
        }

        public void finishUnDocking()
        {
            if (UnDockFinishedEvent != null)
            {
                UnDockFinishedEvent.Invoke();
            }
        }

        public bool isOccupied()
        {
            return occupied;
        }
        
#if UNITY_EDITOR

        void OnDrawGizmos()
        {

            if (drawDockGizmos)
            {

                Color c = Color.magenta;

                if (myDockTransform)
                {

                    c = Color.magenta;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(myDockTransform.position, 0.5f);
                    Gizmos.DrawWireSphere(myDockTransform.position, 0.5f);
                    Gizmos.DrawIcon(myDockTransform.position, "Whilefun/dockTransformIcon.png", false);

                }

                if (myFocusTransform)
                {

                    c = Color.green;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawSphere(myFocusTransform.position, 0.25f);
                    Gizmos.DrawWireSphere(myFocusTransform.position, 0.25f);
                    Gizmos.DrawIcon(myFocusTransform.position, "Whilefun/focusTransformIcon.png", false);

                }

            }

        }


#endif

    }

}