using UnityEngine;
using UnityEngine.UI;

namespace Whilefun.FPEKit
{

    public class FPEInteractableDebugger : MonoBehaviour
    {

        private FPEInteractionManagerScript interactionManager;
        private Text myDebugText;

        string[] currentInteractionData = null;

        public bool drawInteractDebug = true;

        void Start()
        {

            interactionManager = FPEInteractionManagerScript.Instance;
            myDebugText = gameObject.GetComponentInChildren<Text>();

            if (!myDebugText)
            {
                Debug.Log("FirstPersonInteractableDebugger cannot find myDebugText child object!");
            }

        }

        void Update()
        {

            if(Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.I))
            {

                FPEInteractableInventoryItemScript[] allInventory = GameObject.FindObjectsOfType<FPEInteractableInventoryItemScript>();
                Debug.Log("Debug: Putting '"+allInventory.Length+"' found inventory items into inventory!");

                for(int i = 0; i < allInventory.Length; i++)
                {
                    FPEInteractionManagerScript.Instance.putObjectIntoInventory(allInventory[i]);
                }

            }

            currentInteractionData = interactionManager.FetchCurrentInteractionDebugData();

            if (drawInteractDebug)
            {

                string holdingString = (currentInteractionData[4] != "") ? currentInteractionData[4] : "Nothing";
                string lookingAtString = "";

                if (currentInteractionData[0] != "")
                {
                    lookingAtString = currentInteractionData[0];
                }
                else
                {

                    if (currentInteractionData[2] != "")
                    {
                        lookingAtString = "(Put Back)";
                    }
                    else
                    {
                        lookingAtString = "Nothing";
                    }

                }

                myDebugText.text = "LookingAt: [" + lookingAtString + ", type=" + currentInteractionData[1] + "]" + "\n" + "Holding: [" + holdingString + ", type=" + currentInteractionData[5] + "]";
                myDebugText.enabled = true;

            }
            else
            {

                if (myDebugText != null)
                {
                    myDebugText.text = "";
                    myDebugText.enabled = false;
                }

            }

        }

    }

}