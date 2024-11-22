using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEInteractableBaseScript
    // This script is the base or parent of all Interactable object types. The core
    // functionality of Interactable objects happens here. For example, object
    // highlighting, interaction strings, etc. All other Interactable classes
    // should be based on this base class or a child of this class.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [RequireComponent(typeof(Collider))]
    public abstract class FPEInteractableBaseScript : MonoBehaviour
    {

        // Note: You can easily add your own behaviour type by extending this enum. Be sure
        // to also add a case in FPEInteractionManagerScript to handle the new enum value
        public enum eInteractionType
        {

            NULL_TYPE = 0, // Reserved null type for internal checks
            PUT_BACK = 1, // Reserved for put back checks
            STATIC = 2,
            PICKUP = 3,
            ACTIVATE = 4,
            JOURNAL = 5,
            AUDIODIARY = 6,
            INVENTORY = 7,
            DOCK = 8

        };
        protected eInteractionType interactionType = eInteractionType.STATIC;

        // Some or all instances of Interactable may require a reference to the Interaction Manager for playing audio, etc.
        protected FPEInteractionManagerScript interactionManager = null;

        [Header("Highlighting and Interacting")]
        [Tooltip("If true, the object will be highlighted when player moves reticle over the object")]
        public bool highlightOnMouseOver = true;
        [Tooltip("The highlight color, which will tint the object when it is highlighted. Default is blueish, #9292FFFF or RGBA (0.57f, 0.57f, 1.0f, 1.0f)")]
        public Color highlightColor = new Color(0.57f, 0.57f, 1.0f, 1.0f);
        private bool highlightMaterialSet = false;
        
        [Tooltip("The maximum straight-line distance from the player that an object can be interacted with. Default is 2.0.")]
        public float interactionDistance = 2.0f;
        private bool hasBeenDiscovered = false;

        [Header("Optionally play a Secondary Sound on Interact")]
        [Tooltip("Check this box to play a secondary sound when this object is interacted with. For example, the narration of a journal page.")]
        public bool playSecondarySoundOnInteract = false;
        protected bool hasPlayedOnce = false;
        [Tooltip("If checked, the secondary sound will behave the same way as an audio log, including log title and skip functionality. Note that if this is false, the specified sound will not stop until completed and cannot be skipped.")]
        public bool showWithText = false;
        [Tooltip("If 'Show With Text' is checked, this text will appear as the audio log title.")]
        public string audioLogText = "<DEFAULT PICKUP SOUND AUDIO LOG TEXT>";
        public enum eInteractSoundPlaybackType { PLAY_ONCE, PLAY_EVERY_TIME };
        [Tooltip("PLAY_ONCE means the secondary sound will only play on the first interaction. PLAY_EVER_TIME means it will play on every interaction.")]
        public eInteractSoundPlaybackType soundPlaybackBehaviour = eInteractSoundPlaybackType.PLAY_ONCE;
        [Tooltip("The AudioClip for the secondary sound. If this is not specified, no sound will be played, regardless of other values set in this section.")]
        public AudioClip soundToPlayOnInteract = null;

        [Header("Interaction Stings")]
        [Tooltip("The string that appears below the reticle when the object is highlighted")]
        public string interactionString = "<DEFAULT INTERACTION STRING>";

        // Highlight update
        Material[] baseMaterials;
        Material[] highlightMaterials;

        public virtual void Awake()
        {
            // Nothing to do here yet
        }

        public virtual void Start()
        {

            interactionManager = FPEInteractionManagerScript.Instance;

            if (!interactionManager)
            {

                Debug.LogError("FPEInteractableBaseScript:: Cannot find FPE Interaction Manager! Is there an FPECore prefab in your scene?");

#if UNITY_EDITOR
                // Without this, it is possible that a complex scene that is missing an FPECore can print a lot of errors to the console on loop, making it hard to read the console and debug.
                UnityEditor.EditorApplication.isPlaying = false;
#endif

            }

        }

        // Each sub-type must implement this according to the rules of the game world. For 
        // example, for Pickup type this is always false. But for other types, you may want
        // to make it an inspector option as implemented in the Activate type.
        public abstract bool interactionsAllowedWhenHoldingObject();

        // Base version of interact only handles secondary audio. Implement special interactions in 
        // child classes as required (start animations, play audio files, spawn objects, etc.)
        public virtual void interact()
        {

            // If specified, we play a narration or other sound effect when the secondary sound is activated through this interaction
            if (playSecondarySoundOnInteract && soundToPlayOnInteract)
            {

                if (!hasPlayedOnce || soundPlaybackBehaviour == eInteractSoundPlaybackType.PLAY_EVERY_TIME)
                {

                    if (showWithText)
                    {

                        interactionManager.playSecondaryInteractionAudio(soundToPlayOnInteract, true, audioLogText);
                        hasPlayedOnce = true;

                    }
                    else
                    {

                        interactionManager.playSecondaryInteractionAudio(soundToPlayOnInteract, false, "");
                        hasPlayedOnce = true;

                    }

                }

            }
            else if (playSecondarySoundOnInteract && !soundToPlayOnInteract)
            {
                Debug.LogWarning("FPEInteractableBaseScript:: Interactable '" + gameObject.transform.name + "' has 'Play Secondary Sound On Interact' checked, but there is no 'Sound To Play On Interact' Audio Clip specified.");
            }

        }

        public void highlightObject()
        {

            if (!hasBeenDiscovered)
            {
                discoverObject();
            }

            setHighlightMaterial();

        }

        public void unHighlightObject()
        {
            removeHighlightMaterial();
        }

        public eInteractionType getInteractionType()
        {
            return interactionType;
        }

        public float getInteractionDistance()
        {
            return interactionDistance;
        }

        private void setHighlightMaterial()
        {

            if (!highlightMaterialSet && highlightOnMouseOver)
            {

                MeshRenderer[] childMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

                // If we have not saved base materials list, do that
                if (baseMaterials == null)
                {
                    saveBaseMaterials();
                }

                // Create the highlight materials if they don't yet exist or might be stale/bad
                if (highlightMaterials == null || highlightMaterials.Length != baseMaterials.Length)
                {
                    refreshHighlightMaterials();
                }
                else
                {

                    // Apply the highlight materials
                    for (int i = 0; i < childMeshRenderers.Length; i++)
                    {
                        childMeshRenderers[i].material = highlightMaterials[i];
                    }

                }

                highlightMaterialSet = true;

            }

        }

        private void removeHighlightMaterial()
        {

            if (highlightMaterialSet)
            {

                MeshRenderer[] childMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

                for (int i = 0; i < childMeshRenderers.Length; i++)
                {
                    childMeshRenderers[i].material = baseMaterials[i];
                }

                highlightMaterialSet = false;

            }

        }

        /// <summary>
        /// This function forces an update to highlight material set. This is useful when an 
        /// objects "activated" state changes a UV offset of a mesh to show state change. 
        /// Ordinarily, the highlighted material would not change which would look weird. 
        /// By calling this function after you change the UV offset, the highlighted material 
        /// will also incorporate that offset.
        /// </summary>
        public void forceHighlightMaterialUpdate()
        {

            refreshBaseMaterials();
            refreshHighlightMaterials();

        }

        private void saveBaseMaterials()
        {

            MeshRenderer[] childMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            baseMaterials = new Material[childMeshRenderers.Length];

            for (int i = 0; i < childMeshRenderers.Length; i++)
            {
                baseMaterials[i] = childMeshRenderers[i].material;
            }

        }

        // Called when we change one or more materials on an object when we interact with it, and need
        // to refresh it's highlighted state to show the changed material as highlighted. Example of
        // this is switches that change appearence when you're still looking at them via things like
        // materila swaps, texture uv offset changes, etc.
        private void refreshBaseMaterials()
        {

            MeshRenderer[] childMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < childMeshRenderers.Length; i++)
            {

                // Skip any materials that contain "_Highlighted"
                if (!childMeshRenderers[i].material.name.Contains("_Highlighted"))
                {
                    baseMaterials[i] = childMeshRenderers[i].material;
                }

            }

        }

        private void refreshHighlightMaterials()
        {

            MeshRenderer[] childMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

            highlightMaterials = new Material[childMeshRenderers.Length];

            for (int i = 0; i < childMeshRenderers.Length; i++)
            {

                // If we highlight the same object hundreds of times, this may eventually cause a memory leak problem.
                highlightMaterials[i] = new Material(baseMaterials[i]);
                highlightMaterials[i].name = baseMaterials[i].name + "_Highlighted";

                // To make the material stand out, we just tint it a bit. You may want to change other Standard shader properties like Specularity, too.
                highlightMaterials[i].color *= highlightColor;

            }

        }
    
        // Override this (and be sure to call base version) if your child object's discovery includes other functionality (see Audio Diary script)
        public virtual void discoverObject()
        {
            hasBeenDiscovered = true;
        }

        public void setInteractionString(string updatedString)
        {
            interactionString = updatedString;
        }

    }

}