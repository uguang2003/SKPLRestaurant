using UnityEngine;
using Random = UnityEngine.Random;


namespace Whilefun.FPEKit
{

    //
    // FPEInteractablePickupScript
    // This script is for Pickup type Interactable objects. In addition to base
    // functionality, these objects can be picked up, carried around, examined,
    // and put back (or dropped).
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    public class FPEInteractablePickupScript : FPEInteractableBaseScript
    {

        [Tooltip("The string that appears when object is being held, and put back position is highlighted by the reticle.")]
        public string putBackString = "<DEFAULT PUT BACK STRING>";
        [Tooltip("This replaces the interaction string for the object once it is examined by the player. If left blank, the interaction string will not be changed")]
        public string postExaminationInteractionString = "";

        [Header("Object Position, Physics, Manipulation")]
        [Tooltip("If checked, the script will automatically generate a put back location for you. NOTE: Game objects that use Models with mesh colliders may have mixed results. Check import rotation and collider vs. mesh rotations carefully.")]
        public bool autoGeneratePutBackObject = false;
        [Tooltip("How far UP from examination position this object will be when examined. For example, if you wanted to direct the player to read the bottom of a bottle label, you'd want the bottle to be offset up so that the bottom of the label around the center of the screen.")]
        public float examinationOffsetUp = 0.0f;
        [Tooltip("How far FORWARD (away from camera) the object will be when examined. When setting up big objects, this value should generally have a higher value (e.g. 1.0). Smaller objects should have a slightly negative value (e.g. -0.2)")]
        public float examinationOffsetForward = 0.0f;
        [Tooltip("Some imported models might be flipped in one or more axes. If so, use this to offset that effect. Recommended approach is to ensure models are imported in same coordinate orientation as Unity, and leave this value as (0,0,0).")]
        public Vector3 pickupRotationOffset = Vector3.zero;
        [Tooltip("Toss Strength acts as a Physics Force multiplier when the object is tossed Higher values mean the object is tossed harder, and goes farther. Default is 1.0. 5.0 or above is probably too strong.")]
        public float tossStrength = 1.0f;
        [Tooltip("How far UP from examination position this object will be when toss/dropped. Bigger/heaver objects should have a higher value (default is 0.1)")]
        public float tossOffsetUp = 0.1f;
        [Tooltip("How far FORWARD (away from camera) the object will be when toss/dropped. Bigger/heavier objects should have a higher value (default is 0.1)")]
        public float tossOffsetForward = 0.1f;
        public enum eRotationType { FREE, HORIZONTAL, VERTICAL, NONE };
        [Tooltip("FREE - Free rotation in both axes.\nHORIZONTAL - Only rotate side to side.\nVERTICAL - Only rotate up and down.\nNONE - No rotation allowed.")]
        public eRotationType rotationLockType = eRotationType.FREE;

        [Header("Sound Management")]
        [Tooltip("Uncheck this if you don't want this object to make sounds")]
        public bool enableSounds = true;
        [Header("If no sounds are specified, generic sounds will be used instead.")]
        [Tooltip("Pick Up sounds (optional). If more than one is specified, a random sound will be chosen each time a sound of this type needs to be played")]
        public AudioClip[] pickupSounds = null;
        [Tooltip("Put Down sounds (optional). If more than one is specified, a random sound will be chosen each time a sound of this type needs to be played")]
        public AudioClip[] putBackSounds = null;
        [Tooltip("Physics sounds (optional). Played when object hits another object. If more than one is specified, a random sound will be chosen each time a sound of this type needs to be played")]
        public AudioClip[] impactSounds = null;

        // Allows for initial physics impacts to happen when scene starts, but not generate sound.
        private float impactSoundCountdown = 1.0f;
        // We always want *some* sound from our impacts, so make min half volume. Given that they are 3D sounds, half volume even at a distance is fine
        private float minImpactSoundVolume = 0.5f;
        private bool playImpactSounds = false;
        protected bool beingPutBack = false;
        protected bool pickedUp = false;

        [Header("Pickup and Put Back events")]
        [SerializeField, Tooltip("If specified, this event will fire when the object is picked up")]
        private FPEGenericEvent OnPickupEvent = null;
        [SerializeField, Tooltip("If ONETIME, the event will only fire the first time object is picked up. If REPEAT, event will fire every time the object is picked up")]
        private FPEGenericEvent.eRepeatMode pickupRepeatMode = FPEGenericEvent.eRepeatMode.ONETIME;
        private bool pickupEventFiredOnce = false;
        [SerializeField, Tooltip("If specified, this event will fire when the object is put back")]
        private FPEGenericEvent OnPutBackEvent = null;
        [SerializeField, Tooltip("If ONETIME, the event will only fire the first time object is put back. If REPEAT, event will fire every time the object is put back")]
        private FPEGenericEvent.eRepeatMode putbackRepeatMode = FPEGenericEvent.eRepeatMode.ONETIME;
        private bool putbackEventFiredOnce = false;

        public override void Awake()
        {

            base.Awake();

            interactionType = eInteractionType.PICKUP;
            gameObject.layer = LayerMask.NameToLayer("FPEPickupObjects");

            if (enableSounds)
            {

                if (enableSounds && !gameObject.GetComponent<AudioSource>())
                {
                    Debug.LogError("FPEInteractablePickupScript:: Pickup object '" + gameObject.name + "' has sounds enabled, but the Game Object is missing an AudioSource. Either add an AudioSource component, or uncheck the enableSounds check box.");
                }

                gameObject.GetComponent<AudioSource>().loop = false;
                gameObject.GetComponent<AudioSource>().playOnAwake = false;

                // If no impact sounds are specified, just use the generic one
                if (impactSounds.Length == 0)
                {
                    impactSounds = new AudioClip[1];
                    impactSounds[0] = Resources.Load("genericPhysicsImpact") as AudioClip;
                }

                if (pickupSounds.Length == 0)
                {
                    pickupSounds = new AudioClip[1];
                    pickupSounds[0] = Resources.Load("genericPickup") as AudioClip;
                }

                if (putBackSounds.Length == 0)
                {
                    putBackSounds = new AudioClip[1];
                    putBackSounds[0] = Resources.Load("genericPutBack") as AudioClip;
                }

            }

            if (autoGeneratePutBackObject)
            {
                generatePutBackPlace();
            }

        }

        public virtual void Update()
        {

            if (enableSounds)
            {

                if (!playImpactSounds)
                {

                    impactSoundCountdown -= Time.deltaTime;

                    if (impactSoundCountdown <= 0.0f)
                    {
                        playImpactSounds = true;
                    }

                }

                // Stay in being put back state until putback sound is done playing.
                if (beingPutBack)
                {

                    if (!gameObject.GetComponent<AudioSource>().isPlaying)
                    {
                        beingPutBack = false;
                    }

                }

            }

        }

        // Note: Pickup objects can only every be picked up if player's hands are empty
        public override bool interactionsAllowedWhenHoldingObject()
        {
            return false;
        }

        void OnCollisionEnter()
        {

            if (!gameObject.GetComponent<Rigidbody>().isKinematic && playImpactSounds && !beingPutBack && enableSounds)
            {

                // Curb physics sound volume based on how hard object hits something
                float impactVolume = Mathf.Max(minImpactSoundVolume, Mathf.Min(1.0f, (gameObject.GetComponent<Rigidbody>().velocity.magnitude / 5.0f)));

                if (gameObject.GetComponent<AudioSource>().isPlaying)
                {
                    gameObject.GetComponent<AudioSource>().Stop();
                }
                gameObject.GetComponent<AudioSource>().volume = impactVolume;
                gameObject.GetComponent<AudioSource>().clip = impactSounds[Random.Range(0, impactSounds.Length)];
                gameObject.GetComponent<AudioSource>().Play();

            }

        }

        /// <summary>
        /// Handles pickup and put back/drop logic. 
        /// parameter putback should be true if object is being put back, false if it is being dropped.
        /// If you wanted to extend this class and make a special object that triggers an event when
        /// picked up, you would handle that here. (e.g. Indiana Jones picking up the Golden Idol of Fertility)
        /// </summary>
        /// <param name="putback">If true, this call is considered a request to do put back logic. If false, it is considered pick up</param>
        public virtual void doPickupPutdown(bool putback)
        {

            if (pickedUp)
            {

                pickedUp = false;

                if (putback)
                {

                    if (enableSounds)
                    {

                        beingPutBack = true;

                        if (gameObject.GetComponent<AudioSource>().isPlaying)
                        {
                            gameObject.GetComponent<AudioSource>().Stop();
                        }
                        gameObject.GetComponent<AudioSource>().volume = 1.0f;
                        gameObject.GetComponent<AudioSource>().clip = putBackSounds[Random.Range(0, putBackSounds.Length)];
                        gameObject.GetComponent<AudioSource>().Play();

                    }

                    if (!putbackEventFiredOnce || putbackRepeatMode == FPEGenericEvent.eRepeatMode.REPEAT)
                    {

                        putbackEventFiredOnce = true;

                        if (OnPutBackEvent != null)
                        {
                            OnPutBackEvent.Invoke();
                        }

                    }

                }

            }
            else
            {

                base.interact();

                beingPutBack = false;
                pickedUp = true;

                if (enableSounds)
                {

                    if (gameObject.GetComponent<AudioSource>().isPlaying)
                    {
                        gameObject.GetComponent<AudioSource>().Stop();
                    }
                    gameObject.GetComponent<AudioSource>().volume = 1.0f;
                    gameObject.GetComponent<AudioSource>().clip = pickupSounds[Random.Range(0, pickupSounds.Length)];
                    gameObject.GetComponent<AudioSource>().Play();

                }

                if (!pickupEventFiredOnce || pickupRepeatMode == FPEGenericEvent.eRepeatMode.REPEAT)
                {

                    pickupEventFiredOnce = true;

                    if (OnPickupEvent != null)
                    {
                        OnPickupEvent.Invoke();
                    }

                }

            }

        }

        /// <summary>
        /// Ensures that if the object is marked as not currently picked up
        /// </summary>
        public void drop()
        {
            pickedUp = false;
        }

        // This function is called by FPEInteractionManager when the player starts examining an object
        public virtual void startExamination()
        {

        }

        // This function is called by FPEInteractionManager when the player stops examining an object
        public virtual void endExamination()
        {

        }

        public bool isCurrentlyPickedUp()
        {
            return pickedUp;
        }

        public void SetAsPickedUpForGameLoad()
        {
            pickedUp = true;
        }

        // Attempts to create a suitable put back object
        private void generatePutBackPlace()
        {

            GameObject putBackPlace = new GameObject(gameObject.name + "PutBackObject");
            putBackPlace.transform.position = gameObject.transform.position;
            putBackPlace.transform.rotation = gameObject.transform.rotation;
            putBackPlace.transform.localScale = gameObject.transform.localScale;

            // Try to make the best collider based on what the game object already has
            if (gameObject.GetComponent<MeshCollider>())
            {

                // NOTE: Mesh Colliders can behave oddly depending on the 3D toolset used to import the model. If you get 
                // odd-looking results, check rotations and offsets from Mesh to Mesh collider, import rotation/orientation
                // and scale. If that fails, there's nothing wrong with a simple Box Collider :)
                putBackPlace.AddComponent<MeshCollider>();
                putBackPlace.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshCollider>().sharedMesh;
                putBackPlace.GetComponent<MeshCollider>().convex = true;
                putBackPlace.GetComponent<MeshCollider>().isTrigger = true;

            }
            else if (gameObject.GetComponent<SphereCollider>())
            {

                putBackPlace.AddComponent<SphereCollider>();
                putBackPlace.GetComponent<SphereCollider>().radius = gameObject.GetComponent<SphereCollider>().radius;
                putBackPlace.GetComponent<SphereCollider>().center = gameObject.GetComponent<SphereCollider>().center;
                putBackPlace.GetComponent<SphereCollider>().isTrigger = true;

            }
            else if (gameObject.GetComponent<CapsuleCollider>())
            {

                putBackPlace.AddComponent<CapsuleCollider>();
                putBackPlace.GetComponent<CapsuleCollider>().height = gameObject.GetComponent<CapsuleCollider>().height;
                putBackPlace.GetComponent<CapsuleCollider>().radius = gameObject.GetComponent<CapsuleCollider>().radius;
                putBackPlace.GetComponent<CapsuleCollider>().center = gameObject.GetComponent<CapsuleCollider>().center;
                putBackPlace.GetComponent<CapsuleCollider>().direction = gameObject.GetComponent<CapsuleCollider>().direction;
                putBackPlace.GetComponent<CapsuleCollider>().isTrigger = true;

            }
            else if (gameObject.GetComponent<BoxCollider>())
            {

                putBackPlace.AddComponent<BoxCollider>();
                putBackPlace.GetComponent<BoxCollider>().size = gameObject.GetComponent<BoxCollider>().size;
                putBackPlace.GetComponent<BoxCollider>().center = gameObject.GetComponent<BoxCollider>().center;
                putBackPlace.GetComponent<BoxCollider>().isTrigger = true;

            }
            else {
                // If you want to extend to other collider types, you can do that here by adding extra cases. I recommend using one of the above types instead.
                Debug.LogWarning("FPEInteractablePickupScript:: Game Object '" + gameObject.name + "' is attempting to auto-generate a Put Back object for an unhandled Collider Type.");
            }

            // Lastly, add the putback script and assign pick up object id and put back distance
            putBackPlace.AddComponent<FPEPutBackScript>();
            putBackPlace.GetComponent<FPEPutBackScript>().assignMatchingForGameObject(gameObject);
            putBackPlace.GetComponent<FPEPutBackScript>().setInteractionDistance(interactionDistance);

        }

    }

}