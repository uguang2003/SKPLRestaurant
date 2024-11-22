using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEEventTrigger
    // This class allows for arbitrary scripts and other component functions to be executed 
    // when the player or other target enters a trigger volume
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [RequireComponent(typeof(BoxCollider))]
    public class FPEEventTrigger : MonoBehaviour
    {

        [SerializeField, Tooltip("The tag of the object(s) to listen for. When one of these objects enters the trigger, it will activate. Default is 'Player'")]
        private string targetTag = "Player";

        [SerializeField, Tooltip("If true, trigger will be enabled on Start")]
        private bool armOnStart = false;

        [SerializeField, Tooltip("List of other triggers to arm when this trigger is activated")]
        private FPEEventTrigger[] triggersToArmOnActivation = null;
        [SerializeField, Tooltip("List of other triggers to disarm when this trigger is activated")]
        private FPEEventTrigger[] triggersToDisarmOnActivation = null;

        [SerializeField, Tooltip("If specified, this event will fire when the trigger is armed.")]
        private FPEGenericEvent ArmEvent = null;
        [SerializeField, Tooltip("If specified, this event will fire when the trigger is disarmed")]
        private FPEGenericEvent DisarmEvent = null;
        [SerializeField, Tooltip("If specified, this event will fire when a target object enters the trigger.")]
        private FPEGenericEvent ActivationEvent = null;
        [SerializeField, Tooltip("If true, the trigger will disarm itself when activated. If false, it will not, and every subsequent activation will repeat arming/disarming other triggers, and fire the activation event.")]
        private bool disarmOnActivation = true;

#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField, Tooltip("If true, Gizmos will be drawn in editor to aid with layout and testing")]
        private bool drawTriggerGizmos = true;
#endif

        private BoxCollider myCollider = null;

        private bool armed = false;
        private bool tripped = false;
        

        void Awake()
        {

            myCollider = gameObject.GetComponent<BoxCollider>();

            if (!myCollider)
            {
                myCollider = gameObject.AddComponent<BoxCollider>();
            }

            myCollider.isTrigger = true;
            myCollider.size = Vector3.one;

        }

        void Start()
        {

            if (armOnStart)
            {
                armTheTrigger();
            }

        }

        void OnTriggerEnter(Collider other)
        {

            if (other.CompareTag(targetTag) && armed)
            {
                activateTrigger();
            }

        }

        private void activateTrigger()
        {

            for (int t1 = 0; t1 < triggersToArmOnActivation.Length; t1++)
            {
                if (triggersToArmOnActivation[t1])
                {
                    triggersToArmOnActivation[t1].armTheTrigger();
                }
            }

            for (int t2 = 0; t2 < triggersToDisarmOnActivation.Length; t2++)
            {
                if (triggersToDisarmOnActivation[t2])
                {
                    triggersToDisarmOnActivation[t2].disarmTheTrigger();
                }
            }

            if(ActivationEvent != null)
            {
                ActivationEvent.Invoke();
                tripped = true;
            }

            if (disarmOnActivation)
            {
                disarmTheTrigger();
            }

        }

        public void armTheTrigger()
        {

            armed = true;

            if(ArmEvent != null)
            {
                ArmEvent.Invoke();
            }

        }

        public void disarmTheTrigger()
        {

            armed = false;

            if (DisarmEvent != null)
            {
                DisarmEvent.Invoke();
            }

        }

        /// <summary>
        /// Resets trigger to clean 'never tripped' state
        /// </summary>
        public void resetTrigger()
        {
            tripped = false;
        }

        public FPETriggerSaveData getSaveData()
        {
            return new FPETriggerSaveData(gameObject.name, armed, tripped);
        }

        public void restoreSaveGameData(FPETriggerSaveData data)
        {

            armed = data.Armed;
            tripped = data.Tripped;

        }

#if UNITY_EDITOR

        void OnDrawGizmos()
        {

            if (drawTriggerGizmos)
            {

                Color c = Color.red;

                Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

                Gizmos.matrix *= cubeTransform;
                
                

                if (armed)
                {

                    c = Color.green;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
                    Gizmos.DrawIcon(transform.position + (Vector3.up * 0.5f), "Whilefun/triggerArmed.png");

                }
                else
                {

                    c = Color.red;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
                    Gizmos.DrawIcon(transform.position + (Vector3.up * 0.5f), "Whilefun/triggerNotArmed.png");

                }

                Gizmos.matrix = oldGizmosMatrix;

                if (triggersToArmOnActivation != null)
                {

                    c = Color.green;
                    c.a = 0.5f;
                    Gizmos.color = c;

                    for (int t1 = 0; t1 < triggersToArmOnActivation.Length; t1++)
                    {
                        if (triggersToArmOnActivation[t1])
                        {
                            Gizmos.DrawLine(transform.position, triggersToArmOnActivation[t1].transform.position);
                        }
                    }

                }

                if (triggersToDisarmOnActivation != null)
                {

                    c = Color.red;
                    c.a = 0.5f;
                    Gizmos.color = c;

                    for (int t2 = 0; t2 < triggersToDisarmOnActivation.Length; t2++)
                    {
                        if (triggersToDisarmOnActivation[t2])
                        {
                            Gizmos.DrawLine(transform.position, triggersToDisarmOnActivation[t2].transform.position);
                        }
                    }

                }

            }

        }

#endif

    }

}