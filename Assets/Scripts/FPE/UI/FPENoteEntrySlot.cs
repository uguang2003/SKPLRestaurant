using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using UnityEngine.EventSystems;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPENoteEntrySlot
    // A special type of button that remembers if it was selected. For indicating last selected note slot.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPENoteEntrySlot : Selectable, ISubmitHandler
    {

        [SerializeField]
        private Color regularColor = Color.white;
        [SerializeField]
        private Color highlightColor = Color.yellow;
        [SerializeField]
        private Color disabledColor = Color.gray;

        [SerializeField]
        private Sprite regularImage = null;
        [SerializeField]
        private Sprite activatedImage = null;

        private Image frameImage = null;
        private Image iconImage = null;
        private Text myTitle = null;
        private bool highlighted = false;

        private FPENoteEntrySlot[] allNoteSlots = null;

        private int currentNoteIndex = -1;
        public int CurrentNoteIndex {
            get { return currentNoteIndex; }
        }

        protected override void Awake()
        {

            base.Awake();
            frameImage = gameObject.GetComponent<Image>();
            iconImage = gameObject.transform.Find("NoteIcon").GetComponent<Image>();
            myTitle = gameObject.transform.Find("NoteTitle").GetComponent<Text>();

            if (!frameImage || !iconImage || !myTitle)
            {
                Debug.LogError("FPENoteEntrySlot '" + gameObject.name + "' is missing main image or one of NoteIcon (Image), NoteTitle (Text) child objects!");
            }

            allNoteSlots = gameObject.transform.parent.gameObject.GetComponentsInChildren<FPENoteEntrySlot>();

            transition = Transition.None;

        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            executeSelect();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            unhighlight();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            executeClick();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            executeClick();
        }

        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            executeSelect();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            unhighlight();
        }

        /// <summary>
        /// Used strictly when restoring prior menu state, and you need to make the slot look selected and active
        /// </summary>
        public void ForceActivateSlot()
        {
            highlight();
            executeClick();
        }

        public void ForceDeactivateSlot()
        {
            unhighlight();
            frameImage.overrideSprite = regularImage;
        }

        public void ForceUnhighlight()
        {
            unhighlight();
        }

        public void ForceHighlight()
        {
            highlight();
        }

        private void highlight()
        {
            frameImage.color = highlightColor;
            iconImage.color = highlightColor;
            myTitle.color = highlightColor;
            highlighted = true;
        }

        private void unhighlight()
        {
            frameImage.color = regularColor;
            iconImage.color = regularColor;
            myTitle.color = regularColor;
            highlighted = false;
        }

        public void enableSlot()
        {
            interactable = true;
            unhighlight();
        }

        public void disableSlot()
        {
            interactable = false;
            iconImage.color = disabledColor;
            myTitle.color = disabledColor;
            highlighted = false;
        }

        private void displayNote()
        {

            if (currentNoteIndex != -1)
            {
                FPEMenu.Instance.GetComponent<FPEGameMenu>().displayNote(currentNoteIndex);
            }

        }

        private void executeClick()
        {

            if (interactable && highlighted && currentNoteIndex != -1)
            {

                for (int s = 0; s < allNoteSlots.Length; s++)
                {
                    allNoteSlots[s].ForceDeactivateSlot();
                }

                highlight();
                frameImage.overrideSprite = activatedImage;
                displayNote();

            }

        }

        private void executeSelect()
        {

            if (interactable)
            {

                for (int s = 0; s < allNoteSlots.Length; s++)
                {
                    allNoteSlots[s].ForceUnhighlight();
                }

                highlight();

            }

        }

        public void setNoteData(int index, string title)
        {

            currentNoteIndex = index;
            myTitle.text = title;
            myTitle.enabled = true;
            iconImage.enabled = true;

        }

        public void clearNoteData()
        {

            currentNoteIndex = -1;
            myTitle.text = "";
            myTitle.enabled = false;
            iconImage.enabled = false;

        }

    }

}