using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Whilefun.FPEKit.FPEInteractionManagerScript;
using Whilefun.FPEKit;
using UnityEngine.AI;

public class CameraRay : MonoBehaviour
{
    Ray ray;
    RaycastHit hitInfo;

    public float screenW;
    public float screenH;
    public Vector2 screenV2;


    private GameObject FPEInterActionManager;
    private GameObject currentHeldObject;

    void Start()
    {
        screenW = Screen.width / 2;
        screenH = Screen.height / 2;
        screenV2 = new Vector2(screenW, screenH);

        FPEInterActionManager = GameObject.Find("FPEInteractionManager(Clone)");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ray = Camera.main.ScreenPointToRay(screenV2);
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                if (hitInfo.transform.gameObject.tag == "Item")
                {
                    Cursor.visible = true;
                    //GameManager.instance.GMLoadScene(hitInfo.transform.gameObject.GetComponent<Item>().SceneName);
                    //Debug.Log(hitInfo.collider.gameObject+" and "+ hitInfo.collider.gameObject.GetComponent<Item>());

                    //GameManager.instance.ItemSelected = hitInfo.collider.gameObject.GetComponent<Item>();
                    
                    Time.timeScale = 0.0f;
                    FPEInputManager.Instance.LookSensitivity = Vector2.zero;
                    setCursorVisibility(true);

                    GameManager.instance.SelectItemNum = hitInfo.collider.gameObject.GetComponent<Item>().ItemNum;
                    GameManager.instance.SelectItemName = hitInfo.collider.gameObject.GetComponent<Item>().ItemName;
                    GameManager.instance.SelectItemInfo = hitInfo.collider.gameObject.GetComponent<Item>().ItemInfo;
                    GameManager.instance.LookItem();
                    //GameManager.instance.GMLoadScene();
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            ray = Camera.main.ScreenPointToRay(screenV2);
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                if (hitInfo.transform.gameObject.tag == "Guest")
                {
                    currentHeldObject = FPEInterActionManager.GetComponent<FPEInteractionManagerScript>().currentHeldObject;
                    if (currentHeldObject && currentHeldObject.tag == "Apple")
                    {
                        //获取到的物体是Guest，获取GuestAI组件，设置isGoGuestStartLocation为true
                        hitInfo.transform.gameObject.GetComponent<GuestAI>().isGoGuestStartLocation = true;
                        Destroy(currentHeldObject);
                    }


                }
            }
        }
    }

    private void setCursorVisibility(bool visible)
    {
        Cursor.visible = visible;
        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

}
