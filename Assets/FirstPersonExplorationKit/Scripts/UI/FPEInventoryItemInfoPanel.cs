using UnityEngine;
using UnityEngine.UI;

namespace Whilefun.FPEKit
{

    //
    // FPEInventoryItemInfoPanel
    // A basic panel with no interactivity, used to display title, image, and 
    // description of a selected inventory item.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInventoryItemInfoPanel : MonoBehaviour
    {

        private Text myTitle = null;
        private Image myImage = null;
        private Text myDescription = null;

        void Awake()
        {

            myTitle = gameObject.transform.Find("ItemTitle").GetComponent<Text>();
            myImage = gameObject.transform.Find("ItemImage").GetComponent<Image>();
            myDescription = gameObject.transform.Find("ItemDescription").GetComponent<Text>();

            if (!myTitle || !myImage || !myDescription)
            {
                Debug.LogError("FPEInventoryItemInfoPanel '" + gameObject.name + "' is missing one of ItemTitle(Text), ItemImage(Image), or ItemDescription(Text) child objects!");
            }

        }

        public void setItemDetails(FPEInventoryItemData data)
        {

            myTitle.text = data.ItemName;
            myTitle.enabled = true;

            myImage.overrideSprite = data.ItemImage;
            myImage.enabled = true;

            myDescription.text = data.ItemDescription;
            myDescription.enabled = true;

        }

        public void clearItemDetails()
        {

            myTitle.enabled = false;
            myImage.enabled = false;
            myDescription.enabled = false;

        }

    }

}
