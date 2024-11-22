using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Whilefun.FPEKit
{

    //
    // FPETextButton
    // A child class of FPEMenuButton, which is basically the same but has a text 
    // child element in addition to the base button image.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPETextButton : FPEMenuButton
    {
        
        private Text myText;

        protected override void Awake()
        {

            base.Awake();
            myText = gameObject.GetComponentInChildren<Text>();

            if (!myText)
            {
                Debug.LogError("FPETextButton:: Missing Text child component. Did you remove it from a prefab?");
            }

        }

        public override void OnPointerExit(PointerEventData eventData)
        {

            if (interactable)
            {
                myImage.color = regularColor;
                myText.color = regularColor;
                highlighted = false;
            }

        }

        public override void enableButton()
        {

            base.enableButton();
            myText.color = regularColor;

        }

        public override void disableButton()
        {

            base.disableButton();
            myText.color = disabledColor;

        }
        

        protected override void highlight()
        {

            base.highlight();
            myText.color = highlightColor;
            
        }

        protected override void unhighlight()
        {

            base.unhighlight();
            myText.color = regularColor;

        }

    }

}
