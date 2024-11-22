using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEAutomaticDoorTrigger
    // Allows for sliding doors to be automatically opened or closed
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [RequireComponent(typeof(BoxCollider))]
    public class FPEAutomaticDoorTrigger : MonoBehaviour
    {

        [SerializeField, Tooltip("The tag of the object(s) to listen for. When one of these objects enters the trigger, it will activate. Default is 'Player'")]
        private string targetTag = "Player";

        [SerializeField, Tooltip("If true, player standing inside trigger volume will send 'keep alive' to doors so they will not close until player leaves trigger. Default is true.")]
        private bool keepDoorsOpenIfPlayerPresent = true;

        [SerializeField, Tooltip("The set of Manual Sliding Doors to control with this trigger.")]
        private FPESlidingDoor[] doorsToRemotelyControl = null;
        

#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField, Tooltip("If true, Gizmos will be drawn in editor to aid with layout and testing")]
        private bool drawTriggerGizmos = true;
#endif

        private BoxCollider myCollider = null;
        private bool targetPresent = false;


        void Awake()
        {

            myCollider = gameObject.GetComponent<BoxCollider>();

            if (!myCollider)
            {
                myCollider = gameObject.AddComponent<BoxCollider>();
            }

            myCollider.isTrigger = true;
            myCollider.size = Vector3.one;

            if (doorsToRemotelyControl.Length == 0)
            {
                Debug.LogError("FPEAutomaticDoorTrigger:: Trigger '" + gameObject.name + "' has no doors listed in the Inspector. No doors will be opened or closed by this trigger!", gameObject);
            }

            gameObject.layer = LayerMask.NameToLayer("FPEIgnore");

        }

        void Start()
        {

        }

        void OnTriggerEnter(Collider other)
        {

            if (other.CompareTag(targetTag))
            {

                targetPresent = true;

                foreach (FPESlidingDoor d in doorsToRemotelyControl)
                {
                    d.RemotelyOpenDoor();
                }

            }

        }

        void OnTriggerStay(Collider other)
        {

            if (other.CompareTag(targetTag) && keepDoorsOpenIfPlayerPresent)
            {

                foreach (FPESlidingDoor d in doorsToRemotelyControl)
                {
                    d.RemotelyOpenDoor(!targetPresent);
                }

                targetPresent = true;

            }

        }

        private void OnTriggerExit(Collider other)
        {

            if (other.CompareTag(targetTag))
            {
                targetPresent = false;
            }

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

                if (targetPresent)
                {

                    c = Color.green;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);

                }
                else
                {

                    c = Color.red;
                    c.a = 0.5f;
                    Gizmos.color = c;
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);

                }

                Gizmos.DrawIcon(transform.position + (Vector3.up * 0.5f), "Whilefun/automaticDoorTrigger.png");

                Gizmos.matrix = oldGizmosMatrix;


                if (doorsToRemotelyControl != null)
                {
                    foreach (FPESlidingDoor d in doorsToRemotelyControl)
                    {
                        Gizmos.DrawLine(transform.position, d.transform.position);
                    }
                }

            }



        }

#endif

    }

}


