using UnityEngine;
using System.Collections;

namespace Whilefun.FPEKit
{

    //
    // FPEPutBackScript
    // This script is for put back objects. In its simplist form, it's a 
    // trigger collider. If the collider is not set to be a trigger, this
    // will be toggled on Awake. The physics layer is also set to
    // be FPEPutBackObjects.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [RequireComponent(typeof(Collider))]
    public class FPEPutBackScript : MonoBehaviour
    {

        [Tooltip("If set in the inspector, this put back position will be tied to the assigned object. This allows for drag and drop assignment in the Scene editor.")]
        public GameObject myPickupObject = null;
        [Tooltip("The maximum straight-line distance from the player that an object can be put back. Strongly recommended that this be given the same value as the Pickup Object's interaction distance if manually creating Put Back object.")]
        public float interactionDistance = 2.0f;
        private string objectNameToMatch = "";

        void Awake()
        {

            if (!gameObject.GetComponent<Collider>().isTrigger)
            {
                gameObject.GetComponent<Collider>().isTrigger = true;
            }

            gameObject.layer = LayerMask.NameToLayer("FPEPutBackObjects");

            if (myPickupObject != null)
            {
                objectNameToMatch = generateMatchStringFromGameObject(myPickupObject);
            }

        }


        /// <summary>
        /// Checks for a match between this put back location and the provided game object.
        /// </summary>
        /// <param name="go">The GameObject to test for a match against</param>
        /// <returns>True if there is a match, false if there is not.</returns>
        public bool putBackMatchesGameObject(GameObject go)
        {
            return (objectNameToMatch == generateMatchStringFromGameObject(go));
        }

        /// <summary>
        /// To be called when generating a put back location dynamically for a game object.
        /// </summary>
        /// <param name="go">The GameObject to assign as a match for this put back location</param>
        public void assignMatchingForGameObject(GameObject go)
        {
            objectNameToMatch = generateMatchStringFromGameObject(go);
        }

        private string generateMatchStringFromGameObject(GameObject go)
        {
            return (go.name.Split(FPEObjectTypeLookup.PickupPrefabDelimiter)[0]);
        }

        public void setInteractionDistance(float distance)
        {
            interactionDistance = distance;
        }

        public float getInteractionDistance()
        {
            return interactionDistance;
        }

    }

}