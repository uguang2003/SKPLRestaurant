using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPEMenuToggle
    // A child class of FPEMenuButton, which acts as a "radio button" style toggle.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEMenuToggle : FPEMenuButton
    {

        [Header("Toggle Specific Stuff")]
        [SerializeField, Tooltip("If toggle is on, this image is shown on the toggle.")]
        private Sprite toggleOnImage = null;
        [SerializeField, Tooltip("If toggle is off, this image is shown on the toggle.")]
        private Sprite toggleOffImage = null;
        private Image myIcon = null;

        [Serializable]
        public class MenuToggleEvent : UnityEvent { }
        [SerializeField]
        private MenuToggleEvent ToggleOnEvent = null;
        [SerializeField]
        private MenuToggleEvent ToggleOffEvent = null;

        private bool toggledOn = false;

        protected override void Awake()
        {

            base.Awake();
            myIcon = transform.Find("ToggleIcon").GetComponent<Image>();

            if (!myIcon)
            {
                Debug.LogError("FPEMenuToggle:: Missing Image child component 'ToggleIcon'. Did you remove it from a prefab?");
            }

        }
        

        protected override void executeClick()
        {

            if (highlighted)
            {

                if (playSounds)
                {
                    myAudio.clip = buttonClick;
                    myAudio.Play();
                }

                if (toggledOn)
                {

                    if (ToggleOffEvent != null)
                    {
                        ToggleOffEvent.Invoke();
                    }

                    myIcon.overrideSprite = toggleOffImage;
                    toggledOn = false;

                }
                else
                {

                    if (ToggleOnEvent != null)
                    {
                        ToggleOnEvent.Invoke();
                    }

                    myIcon.overrideSprite = toggleOnImage;
                    toggledOn = true;

                }

            }

        }
        

        public override void enableButton()
        {

            myImage.color = regularColor;
            interactable = true;

        }

        public override void disableButton()
        {

            interactable = false;
            myImage.color = disabledColor;
            highlighted = false;

        }

        public void ForceToggleState(bool toggleOn)
        {

            if (toggleOn)
            {
                myIcon.overrideSprite = toggleOnImage;
                toggledOn = true;
            }
            else
            {
                myIcon.overrideSprite = toggleOffImage;
                toggledOn = false;
            }
           
        }

    }

}
