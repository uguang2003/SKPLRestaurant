using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEDrawerContentsGrabber
    // This script is used to grab objects when the are put into a drawer.
    //
    // To use, put a correctly-sized (see demo drawers) trigger into the drawer. When objects hit the trigger, they will be childed to the drawer.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    public class FPEDrawerContentsGrabber : MonoBehaviour
    {

        private BoxCollider myBoxCollider = null;

        private void Awake()
        {

            gameObject.layer = LayerMask.NameToLayer("FPEIgnore");

            myBoxCollider = gameObject.GetComponent<BoxCollider>();
            myBoxCollider.isTrigger = true;

            if(transform.parent.name != "SlidingPart")
            {
                Debug.LogError("FPEDrawerContentsGrabber:: Grabber '" + gameObject.name + "' does not seem to be a child of an FPEDrawer object's 'SlidingPart'. Drawer grabber probably won't work the way you intended.", gameObject);
            }

            if(transform.localScale != Vector3.one)
            {
                Debug.LogError("FPEDrawerContentsGrabber:: Grabber '" + gameObject.name + "' has scale " + transform.localScale + " rather than (1,1,1). This may causes child objects to become distorted or scaled improperly.", gameObject);
            }

        }

        private void OnTriggerEnter(Collider other)
        {

            // We want to check that the object that hit us has no parent before we make the drawer the parent. This will avoid weird cases from breaking things.
            // Also want to make sure we don't grab the player if they somehow touch the trigger :)
            if (other.transform.parent == null && other.gameObject.GetComponent<FPEPlayer>() == null)
            {
                other.transform.parent = this.transform;
            }

        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {

            if (myBoxCollider != null)
            {

                Color c = Color.magenta;
                c.a = 0.5f;
                Gizmos.color = c;

                Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

                Gizmos.matrix *= cubeTransform;
                Gizmos.DrawCube(Vector3.zero, myBoxCollider.size);
                Gizmos.matrix = oldGizmosMatrix;

            }

        }

#endif


    }

}