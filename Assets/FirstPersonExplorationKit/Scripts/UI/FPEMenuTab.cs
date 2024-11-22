using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPEMenuTab
    // A button-type object that can either be actuated by mouse clicks, or manually 
    // selected from other scripts (e.g. As required for gamepad navigation)
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEMenuTab : Selectable
    {

        [SerializeField]
        private Color regularColor = Color.white;
        [SerializeField]
        private Color highlightColor = Color.yellow;

        [SerializeField]
        private Sprite regularTabImage = null;
        [SerializeField]
        private Sprite selectedTabImage = null;

        private Image myImage = null;
        private Text myText = null;
        private bool highlighted = false;

        [Serializable]
        public class MenuTabEvent : UnityEvent { }
        [SerializeField]
        private MenuTabEvent OnClickEvent = null;

        private FPEMenuTab[] allTabs = null;

        protected override void Awake()
        {

            base.Awake();
            myImage = gameObject.GetComponent<Image>();
            myText = gameObject.GetComponentInChildren<Text>();

            if (!myImage || !myText)
            {
                Debug.LogError("FPEMenuTab:: Missing Image component, or Image or Text child component(s). Did you remove one or both of these from a menu tab?");
            }

            allTabs = gameObject.transform.parent.gameObject.GetComponentsInChildren<FPEMenuTab>();

        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            highlight();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            unhighlight();
        }

        // Note: We only need to implement OnPointerDown (and not OnSubmit) because 
        // our non-mouse interaction via gamepad uses bumper buttons to change tabs
        public override void OnPointerDown(PointerEventData eventData)
        {

            if (highlighted)
            {

                if (OnClickEvent != null)
                {
                    OnClickEvent.Invoke();
                }

            }

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
        // To be called only from FPEGameMenu class for forcing tab selection based on gamepad button presses
        /// </summary>
        public void ForceSelectTab()
        {
            executeSelect();
        }

        public void ForceUnhighlight()
        {
            myImage.overrideSprite = regularTabImage;
            unhighlight();
        }

        private void executeSelect()
        {

            for(int t = 0; t < allTabs.Length; t++)
            {
                allTabs[t].ForceUnhighlight();
            }

            myImage.overrideSprite = selectedTabImage;
            highlight();

        }

        private void highlight()
        {
            myImage.color = highlightColor;
            myText.color = highlightColor;
            highlighted = true;
        }

        private void unhighlight()
        {
            myImage.color = regularColor;
            myText.color = regularColor;
            highlighted = false;
        }
        
    }

}