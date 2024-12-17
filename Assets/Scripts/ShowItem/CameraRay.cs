using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Whilefun.FPEKit.FPEInteractionManagerScript;
using Whilefun.FPEKit;
using UnityEngine.AI;
using UnityEngine.Video;

public class CameraRay : MonoBehaviour
{
    Ray ray;
    RaycastHit hitInfo;

    public float screenW;
    public float screenH;
    public Vector2 screenV2;

    private GameObject FPEInterActionManager;
    private GameObject FPEDefaultHUDManager;
    private GameObject currentHeldObject;

    float time = 0;

    void Start()
    {
        screenW = Screen.width / 2;
        screenH = Screen.height / 2;
        screenV2 = new Vector2(screenW, screenH);

        FPEInterActionManager = GameObject.Find("FPEInteractionManager(Clone)");
        FPEDefaultHUDManager = GameObject.Find("FPEDefaultHUD(Clone)");
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

                    ShowManager.instance.SelectItemNum = hitInfo.collider.gameObject.GetComponent<ShowItem>().ItemNum;
                    ShowManager.instance.SelectItemName = hitInfo.collider.gameObject.GetComponent<ShowItem>().ItemName;
                    ShowManager.instance.SelectItemInfo = hitInfo.collider.gameObject.GetComponent<ShowItem>().ItemInfo;
                    ShowManager.instance.LookItem();
                    //GameManager.instance.GMLoadScene();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ray = Camera.main.ScreenPointToRay(screenV2);
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                if (Vector3.Distance(transform.position, hitInfo.transform.position) < 2.5f)
                {
                    //time += Time.deltaTime;
                    //if (time >= 0.1f)
                    //{
                        if (hitInfo.transform.gameObject.tag == "Guest")
                        {
                            currentHeldObject = FPEInterActionManager.GetComponent<FPEInteractionManagerScript>().currentHeldObject;
                            if (currentHeldObject && currentHeldObject.tag == "Apple" && hitInfo.transform.gameObject.GetComponent<GuestAI>().eat == 0)
                            {
                                //获取到的物体是Guest，获取GuestAI组件，设置isGoGuestStartLocation为true
                                hitInfo.transform.gameObject.GetComponent<GuestAI>().isGoGuestStartLocation = true;
                                FPEDefaultHUDManager.GetComponent<FPEDefaultHUD>().money += 20;
                                Destroy(currentHeldObject);
                            }
                            if (currentHeldObject && currentHeldObject.tag == "Burger" && hitInfo.transform.gameObject.GetComponent<GuestAI>().eat == 1)
                            {
                                //获取到的物体是Guest，获取GuestAI组件，设置isGoGuestStartLocation为true
                                hitInfo.transform.gameObject.GetComponent<GuestAI>().isGoGuestStartLocation = true;
                                FPEDefaultHUDManager.GetComponent<FPEDefaultHUD>().money += 30;
                                Destroy(currentHeldObject);
                            }
                        }

                        if (hitInfo.transform.gameObject.GetComponent<VideoPlayer>())
                        {
                            if (hitInfo.transform.gameObject.GetComponent<VideoPlayer>().isPaused)
                            {
                                hitInfo.transform.gameObject.GetComponent<VideoPlayer>().Play();
                            }
                            else
                            {
                                hitInfo.transform.gameObject.GetComponent<VideoPlayer>().Pause();
                            }
                        }
                        time = 0;
                    }
                //}
                
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
