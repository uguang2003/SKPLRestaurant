using UnityEngine;
using UnityEngine.UI;

namespace Whilefun.FPEKit
{

    //
    // FPEUIHint
    // This class allows for generic screen hints to be drawn and changed based on context
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEUIHint : MonoBehaviour
    {

        private Text hintText = null;
        private Image[] myIcons = null;
        private bool hidingHint = false;

        void Awake()
        {

            myIcons = gameObject.GetComponentsInChildren<Image>();

            if(myIcons.Length < 1)
            {
                Debug.LogError("FPEUIHint:: There are no child objects with component Image. Did you change the prefab?");
            }

            hintText = transform.Find("HintText").gameObject.GetComponent<Text>();

            if (!hintText)
            {
                Debug.LogError("FPEUIHint:: Cannot find child object 'HintText'(Text). Did you remove or rename it?");
            }

        }

        /// <summary>
        /// Sets visibility and text content of the hint. If hintMessage is blank, hint is 
        /// disabled. If not, hint is shown with specified text. However, if hint has been made 
        /// invisible through the setHintVisibility function, it will remain hidden regardless 
        /// of calls to this function.
        /// </summary>
        /// <param name="hintMessage">The string to apply to the hint</param>
        public void setHint(string hintMessage)
        {

            if (hintMessage == "")
            {

                for (int i = 0; i < myIcons.Length; i++)
                {
                    myIcons[i].enabled = false;
                }

                hintText.enabled = false;

            }
            else
            {

                hintText.text = hintMessage;

                // Only turn text back on if it was not manually hidden from view.
                if (!hidingHint)
                {

                    for (int i = 0; i < myIcons.Length; i++)
                    {
                        myIcons[i].enabled = true;
                    }
                    
                    hintText.enabled = true;

                }

            }

        }

        /// <summary>
        /// Allows for hints to be hidden from view entirely
        /// </summary>
        /// <param name="visible">If false, hints are hidden. If true, they are unhidden</param>
        public void setHintVisibility(bool visible)
        {

            for (int i = 0; i < myIcons.Length; i++)
            {
                myIcons[i].enabled = visible;
            }

            hintText.enabled = visible;
            hidingHint = !visible;

        }
       
        
    }

}