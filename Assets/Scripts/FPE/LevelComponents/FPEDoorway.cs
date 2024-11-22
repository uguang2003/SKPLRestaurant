using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEDoorway
    // This class acts as a level doorway with an exit trigger volume and entrance. When 
    // the player enters the trigger volume, the specified scene is loaded. When a player 
    // goes from one scene to another, they are placed at the doorway entrance that 
    // corresponds to the specified connectedScene. Auto-saving of scene player is 
    // leaving will also take place if desired.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [RequireComponent(typeof(BoxCollider))]
    public class FPEDoorway : MonoBehaviour
    {

        [SerializeField, Tooltip("The scene build index of the connected scene. When Player enters the door trigger, this scene is loaded. When player returns to this scene from the connected scene, player will be placed at the doorway transform.")]
        private int connectedSceneIndex = 0;
        public int ConnectedSceneIndex { get { return connectedSceneIndex; } }

        [SerializeField, Tooltip("The transform that will act as the 'Entrance' for the player. When the player uses this door to ENTER a level, they are placed at the entrance transform.")]
        private Transform doorwayEntranceTransform = null;
        public Transform DoorwayEntranceTransform { get { return doorwayEntranceTransform; } }

        [SerializeField, Tooltip("If true, current level's state will be saved before changing to next scene. If false, current level's state will not be saved. It is strongly recommended that ALL doors in your game do this consistently.")]
        private bool autoSaveOnExit = true;

        private BoxCollider myCollider = null;

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

        void OnTriggerEnter(Collider other)
        {

            // Only react to player if player is not currently suspended (easier testing, prevents 
            // instant/infinite save-load loops if door entrance was placed too close to door exit trigger)
            if (other.CompareTag("Player") && !FPEInteractionManagerScript.Instance.PlayerSuspendedForSaveLoad)
            {

                myCollider.enabled = false;

                if (autoSaveOnExit)
                {
                    FPESaveLoadManager.Instance.ChangeSceneToAndAutoSave(connectedSceneIndex);
                }
                else
                {
                    FPESaveLoadManager.Instance.ChangeSceneToNoSave(connectedSceneIndex);
                }

            }

        }
        
#if UNITY_EDITOR

        void OnDrawGizmos()
        {

            Color c = Color.yellow;
            c.a = 0.5f;
            Gizmos.color = c;

            Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

            Gizmos.matrix *= cubeTransform;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = oldGizmosMatrix;

            Gizmos.DrawIcon(transform.position, "Whilefun/doorwayExit.png", false);

            if(doorwayEntranceTransform != null)
            {

                c = Color.red;
                c.a = 0.5f;
                Gizmos.color = c;

                Gizmos.DrawSphere(doorwayEntranceTransform.position, 0.75f);
                Gizmos.DrawWireSphere(doorwayEntranceTransform.position, 0.75f);
                Gizmos.DrawIcon(doorwayEntranceTransform.position, "Whilefun/doorwayEntrance.png", false);

            }

        }

#endif

    }

}