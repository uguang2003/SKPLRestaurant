using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Whilefun.FPEKit
{

    //
    // FPEAudioDiaryEntrySlot
    // A button-like element to allow player to select an audio diary to
    // play. Also contains an icon indicative of audio content.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEAudioDiaryEntrySlot : Selectable, ISubmitHandler
    {

        [SerializeField]
        private Color regularColor = Color.white;
        [SerializeField]
        private Color highlightColor = Color.yellow;
        [SerializeField]
        private Color disabledColor = Color.gray;

        private Image frameImage = null;
        private Image iconImage = null;
        private Text myTitle = null;
        private bool highlighted = false;

        private FPEAudioDiaryEntrySlot[] allDiarySlots;

        private int currentAudioDiaryIndex = -1;
        public int CurrentAudioDiaryIndex {
            get { return currentAudioDiaryIndex; }
        }

        protected override void Awake()
        {

            base.Awake();
            frameImage = gameObject.GetComponent<Image>();
            iconImage = gameObject.transform.Find("DiaryIcon").GetComponent<Image>();
            myTitle = gameObject.transform.Find("DiaryTitle").GetComponent<Text>();

            if (!frameImage || !iconImage || !myTitle)
            {
                Debug.LogError("FPEAudioDiaryEntrySlot '" + gameObject.name + "' is missing main image or one of DiaryIcon (Image), DiaryTitle (Text) child objects!");
            }

            allDiarySlots = gameObject.transform.parent.gameObject.GetComponentsInChildren<FPEAudioDiaryEntrySlot>();

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

        public void ForceUnhighlight()
        {
            unhighlight();
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
            iconImage.color = regularColor;
            myTitle.color = regularColor;
            highlighted = false;
        }

        public void disableSlot()
        {
            interactable = false;
            iconImage.color = disabledColor;
            myTitle.color = disabledColor;
            highlighted = false;
        }

        private void playDiary()
        {
            FPEMenu.Instance.GetComponent<FPEGameMenu>().performReplayAudioDiaryAction(currentAudioDiaryIndex);
        }

        private void executeClick()
        {

            if (interactable && highlighted)
            {
                playDiary();
            }

        }

        private void executeSelect()
        {

            if (interactable)
            {

                for (int s = 0; s < allDiarySlots.Length; s++)
                {
                    allDiarySlots[s].ForceUnhighlight();
                }

                frameImage.color = highlightColor;
                iconImage.color = highlightColor;
                myTitle.color = highlightColor;
                highlighted = true;

            }

        }

        public void setAudioData(int index, string title)
        {

            currentAudioDiaryIndex = index;
            myTitle.text = title;
            myTitle.enabled = true;
            iconImage.enabled = true;

        }

        public void clearAudioData()
        {

            currentAudioDiaryIndex = -1;
            myTitle.text = "";
            myTitle.enabled = false;
            iconImage.enabled = false;

        }

    }

}