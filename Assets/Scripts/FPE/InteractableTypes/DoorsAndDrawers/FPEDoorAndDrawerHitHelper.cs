using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEDoorAndDrawerHitHelper
    // This script helps FPEDoor and FPEDrawer deal with the hitting stuff when it is opening or closing.
    //
    // To use, place on the moving parts of the door per the setup guide documentation.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [RequireComponent(typeof(Collider))]
    public class FPEDoorAndDrawerHitHelper : MonoBehaviour
    {

        private bool hasHitSomething = false;
        public bool HasHitSomething {
            get { return hasHitSomething; }
        }

        int examinationIgnoreLayer = 0;

#if UNITY_EDITOR
        private GameObject blockingObject = null;
#endif

        private void Awake()
        {

            gameObject.GetComponent<Collider>().isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("FPEIgnore");
            examinationIgnoreLayer = LayerMask.NameToLayer("FPEObjectExamination");

        }

        private void OnTriggerStay(Collider other)
        {

            // We ignore hitting things in the player's hand because that is annoying most of the time, and only really 
            // looks funny if the player is holding a reaaaally long object and poking that object through a door's path 
            // when they try to open or close the door.
            if (hasHitSomething == false && other.gameObject.layer != examinationIgnoreLayer)
            {
                hasHitSomething = true;
#if UNITY_EDITOR
                blockingObject = other.gameObject;
#endif
            }

        }

        public void ResetHits()
        {
            hasHitSomething = false;
#if UNITY_EDITOR
            blockingObject = null;
#endif
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {

            Color c = Color.yellow;

            if (hasHitSomething)
            {
                c = Color.red;    
            }

            Gizmos.color = c;
            c.a = 0.5f;

            Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

            Gizmos.matrix *= cubeTransform;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = oldGizmosMatrix;

            if (blockingObject != null)
            {
                Gizmos.DrawSphere(blockingObject.transform.position, 0.1f);
                Gizmos.DrawLine(transform.position, blockingObject.transform.position);
            }

        }

#endif

    }

}