using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPEMenuButton
    // This is a simple class to override basic Unity UI buttons. It has an image and 
    // some transition colors, and handles highlighting, focus, and events.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEMenuButton : Selectable, ISubmitHandler
    {

        [Header("Button colors")]
        [SerializeField]
        protected Color regularColor = Color.white;
        [SerializeField]
        protected Color highlightColor = Color.yellow;
        [SerializeField]
        protected Color disabledColor = Color.gray;

        [Header("Button sounds")]
        [SerializeField, Tooltip("If true, sounds will be played. If none are specified, default sounds will be used.")]
        protected bool playSounds = true;
        [SerializeField]
        protected AudioClip buttonSelect;
        [SerializeField]
        protected AudioClip buttonClick;

        protected AudioSource myAudio;
        protected Image myImage;
        protected bool highlighted = false;

        [Serializable]
        public class MenuTabEvent : UnityEvent { }
        [SerializeField]
        protected MenuTabEvent OnClickEvent;

        protected FPEMenuButton[] allSiblingButtons;

        protected override void Awake()
        {

            base.Awake();
            myImage = gameObject.GetComponent<Image>();

            if (!myImage)
            {
                Debug.LogError("FPEMenuButton:: Missing Image child component. Did you remove this from a menu button?");
            }

            if (playSounds)
            {

                myAudio = gameObject.GetComponent<AudioSource>();

                if (!myAudio)
                {
                    myAudio = gameObject.AddComponent<AudioSource>();
                }

                myAudio.loop = false;
                myAudio.playOnAwake = false;

                if (buttonSelect == null)
                {
                    buttonSelect = Resources.Load("defaultMenuSelect") as AudioClip;
                }

                if (buttonClick == null)
                {
                    buttonClick = Resources.Load("defaultMenuClick") as AudioClip;
                }

            }

            // Note: this button is not excluded because it doesn't really cost much to redundantly un-then-re-highlight one button
            allSiblingButtons = gameObject.transform.parent.gameObject.GetComponentsInChildren<FPEMenuButton>();

            // By default, we ignore transitions and just do hard custom color swaps
            transition = Transition.None;

        }

        public override void OnPointerEnter(PointerEventData eventData)
        {

            if (interactable)
            {
                executeSelect();
            }

        }

        public override void OnPointerExit(PointerEventData eventData)
        {

            if (interactable)
            {
                myImage.color = regularColor;
                highlighted = false;
            }

        }

		//
		// Note: Switching to OnPointerUp() instead of OnPointerDown() prevents accidental "Left Mouse Button" interactions 
		// from occurring when the player clicks a UI button to close some UI. For example, if the player is holding a 
		// Pickup, opens a UI, left clicks to close the UI using a close button, an OnPointerDown() event would make the 
		// player drop the Pickup they were holding.
		//
        //public override void OnPointerDown(PointerEventData eventData)
        public override void OnPointerUp(PointerEventData eventData)
        {
            executeClick();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            executeClick();
        }

        protected virtual void executeSelect()
        {

            // This is a workaround for the combo Gamepad/Mouse navigation sometimes 
            // causing double highlights. It's a byproduct of telling the event system
            // to select an object when the mouse is not over that object. When the mouse
            // moves over another object, no OnPointerExit event is fired, so the first
            // object remains highlighted which looks confusing.
            for(int b = 0; b < allSiblingButtons.Length; b++)
            {
                allSiblingButtons[b].ForceUnhighlight();
            }

            highlight();

        }

        protected virtual void executeClick()
        {

            if (highlighted)
            {

                if (playSounds)
                {
                    myAudio.clip = buttonClick;
                    myAudio.Play();
                }

                if (OnClickEvent != null)
                {
                    OnClickEvent.Invoke();
                }

            }

        }

        public void ForceUnhighlight()
        {

            if (interactable)
            {
                unhighlight();
            }

        }

        public void ForceHighlight()
        {

            if (interactable)
            {
                highlight();
            }

        }

        public override void OnSelect(BaseEventData eventData)
        {

            if (interactable)
            {
                base.OnSelect(eventData);
                executeSelect();
            }

        }

        public override void OnDeselect(BaseEventData eventData)
        {

            if (interactable)
            {
                base.OnDeselect(eventData);
                unhighlight();
            }

        }

        public virtual void enableButton()
        {

            myImage.color = regularColor;
            interactable = true;

        }

        public virtual void disableButton()
        {

            interactable = false;
            myImage.color = disabledColor;
            highlighted = false;

        }

        public void setButtonInteractionState(bool interactable)
        {

            if (interactable)
            {
                enableButton();
            }
            else
            {
                disableButton();
            }

        }

        protected virtual void highlight()
        {

            myImage.color = highlightColor;
            highlighted = true;

            if (playSounds)
            {
                myAudio.clip = buttonSelect;
                myAudio.Play();
            }

        }

        protected virtual void unhighlight()
        {

            myImage.color = regularColor;
            highlighted = false;

        }

    }

}
