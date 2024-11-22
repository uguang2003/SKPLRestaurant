using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowUI : MonoBehaviour
{
    public static ShowUI instance { get; private set; }

    public Text ItemName;

    public Text ItemInfoTextObject;

    public Button BackButton;

    public Button ResetRotateButton;

    public float RotateSpeed;

    public GameObject ItemGroupGameObject;

    public List<GameObject> ItemList = new List<GameObject>();

    public Vector3 RawInputV3;
    //public Item ItemSelected;

    public void Start()
    {
        instance = this;
        BackButton.onClick.AddListener(BackToMuseum);
        ResetRotateButton.onClick.AddListener(OnInitRotate);
    }

    public void Update()
    {
        RawInputV3 = Input.mousePosition;
        ItemRotate();
    }

    //刷新物体介绍相关数据
    public void RefreshItemInfo()
    {
        if (ItemList[ShowManager.instance.SelectItemNum] != null)
        {
            ItemList[ShowManager.instance.SelectItemNum].SetActive(true);
            ItemName.text = ShowManager.instance.SelectItemName;
            ItemInfoTextObject.text = ShowManager.instance.SelectItemInfo;
        }
        else
        {
            Debug.LogWarning("ItemList[Index] doesn't exisit.");
        }
    }

    //返回至博物馆
    private void BackToMuseum()
    {
        if (ItemList[ShowManager.instance.SelectItemNum] != null)
        {
            ItemList[ShowManager.instance.SelectItemNum].SetActive(false);
        }
        ShowManager.instance.ToMuseum();
        Cursor.visible = false;
    }

    //物体旋转
    public void ItemRotate()
    {
        if (ItemGroupGameObject !=null)
        {
            float h = 0;
            float w = 0;
            if (Input.GetMouseButton(0))
            {
                h = -Input.GetAxis("Mouse X");
                w = -Input.GetAxis("Mouse Y");
            }
            ItemGroupGameObject.transform.Rotate(0, h * RotateSpeed, 0,Space.World);
            ItemGroupGameObject.transform.Rotate(-w * RotateSpeed, 0, 0,Space.World);

        }
    }

    public void OnInitRotate()
    {
        ItemGroupGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }
}
