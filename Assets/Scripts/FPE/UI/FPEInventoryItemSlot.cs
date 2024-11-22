using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Whilefun.FPEKit
{

    //
    // FPEInventoryItemSlot
    // A button-like element which can be selected and clicked by various 
    // means. When clicked, it displays applicable actions.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEInventoryItemSlot : Selectable, ISubmitHandler
    {

        [SerializeField]
        private Color regularColor = Color.white;
        [SerializeField]
        private Color highlightColor = Color.yellow;
        [SerializeField]
        private Color disabledColor = Color.gray;

        // Appears before stacked quantity. E.g. 'x' would yield "x5" for 5 items
        private string stackablePrefixString = "x";

        private Image frameImage = null;
        private Image myImage = null;
        private Text myName = null;
        private Text myCount = null;
        private bool highlighted = false;

        // Internal stuff
        private int currentInventoryDataIndex = -1;
        public int CurrentInventoryDataIndex {
            get { return currentInventoryDataIndex; }
        }

        private FPEInventoryItemSlot[] allItemSlots;

        protected override void Awake()
        {

            base.Awake();
            frameImage = gameObject.GetComponent<Image>();
            myImage = gameObject.transform.Find("ItemImage").GetComponent<Image>();
            myName = gameObject.transform.Find("ItemName").GetComponent<Text>();
            myCount = gameObject.transform.Find("ItemCount").GetComponent<Text>();

            if (!frameImage || !myImage || !myName || !myCount)
            {
                Debug.LogError("FPEInventoryItemSlot '" + gameObject.name + "' is missing one of ItemImage(Image), ItemName(Text), or ItemCount(Text) child objects!");
            }

            allItemSlots = gameObject.transform.parent.gameObject.GetComponentsInChildren<FPEInventoryItemSlot>();

        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            executeSelect();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            frameImage.color = regularColor;
            myName.color = regularColor;
            highlighted = false;
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
            frameImage.color = regularColor;
            myName.color = regularColor;
            highlighted = false;
        }

        public void ForceUnhighlight()
        {
            frameImage.color = regularColor;
            myName.color = regularColor;
            highlighted = false;
        }

        public void enableSlot()
        {

            interactable = true;
            myImage.color = regularColor;
            myName.color = regularColor;
            highlighted = false;

        }

        public void disableSlot()
        {

            interactable = false;
            myImage.color = disabledColor;
            myName.color = disabledColor;
            highlighted = false;

        }

        /// <summary>
        /// Sets data for the inventory slot UI
        /// </summary>
        /// <param name="index">A "blind" index into the item data set that is used for message passing. Never manipulated by the slot itself.</param>
        /// <param name="data">The actual item data, to be read by slot so display parameters can be changed</param>
        public void setItemData(int index, FPEInventoryItemData data)
        {

            currentInventoryDataIndex = index;

            myImage.overrideSprite = data.ItemImage;
            myImage.enabled = true;
            
            myName.text = data.ItemName;
            myName.enabled = true;

            if (data.Stackable)
            {
                myCount.text = stackablePrefixString + data.Quantity;
                myCount.enabled = true;
            }
            else
            {
                myCount.enabled = false;
            }

        }

        public void clearItemData()
        {
            myImage.enabled = false;
            myName.enabled = false;
            myCount.enabled = false;
            currentInventoryDataIndex = -1;
        }

        public void setStackableItemQuantity(int quantity)
        {
            myCount.text = stackablePrefixString + quantity;
        }

        private void passItemDetailsToMenu()
        {

            if (currentInventoryDataIndex != -1)
            {
                FPEMenu.Instance.GetComponent<FPEGameMenu>().updateItemDataView(currentInventoryDataIndex);
            }
            else
            {
                FPEMenu.Instance.GetComponent<FPEGameMenu>().clearItemDataView();
            }

        }

        private void showMyActions()
        {

            if(currentInventoryDataIndex != -1)
            {
                FPEMenu.Instance.GetComponent<FPEGameMenu>().showActionsForItem(currentInventoryDataIndex);
            }

        }

        private void executeClick()
        {

            if (interactable && highlighted)
            {
                showMyActions();
            }

        }

        private void executeSelect()
        {

            if (interactable)
            {

                for(int s = 0; s < allItemSlots.Length; s++)
                {
                    allItemSlots[s].ForceUnhighlight();
                }

                frameImage.color = highlightColor;
                myName.color = highlightColor;
                highlighted = true;
                passItemDetailsToMenu();

            }

        }

    }

}