using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Whilefun.FPEKit.FPEInteractionManagerScript;
using Whilefun.FPEKit;

public class GameManager : MonoBehaviour
{
    public static WaitForSeconds waitForMOneSecond = new WaitForSeconds(0.1f);

    public GameObject PlayerPerfab;

    public Vector3 PlayerV3;

    public static GameManager instance { get; private set; }

    public bool LoadedPlayerv3 = false;

    //被选择的物品的信息
    public int SelectItemNum;
    public string SelectItemName;
    public string SelectItemInfo;

    //public Item ItemSelected;

    private GameObject MuseumGroup;
    private GameObject FPEInteractionManager;
    private GameObject UIManager;
    private GameObject FPEInputManager;

    public GameObject LookItemGroup;

    //private static bool origional = true;


    //protected virtual void Awake()
    //{
    //    if (origional)
    //    {
    //        instance = this;
    //        origional = false;
    //        DontDestroyOnLoad(this.gameObject);
    //    }
    //    else
    //    {
    //        Destroy(this.gameObject);
    //    }
    //}

    private void Start()
    {
        Cursor.visible = false;
        instance = this;

        GameObject[] gameObjects = getDontDestroyOnLoadGameObjects();

        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i].name == "FPEDefaultHUD(Clone)")
            {
                UIManager = gameObjects[i];
            }
            if (gameObjects[i].name == "FPEInputManager(Clone)")
            {
                FPEInputManager = gameObjects[i];
            }
            if (gameObjects[i].name == "FPEInteractionManager(Clone)")
            {
                FPEInteractionManager = gameObjects[i];
            }
            if (gameObjects[i].name == "FPECore")
            {
                MuseumGroup = gameObjects[i];
            }
        }


        //UIManager = GameObject.Find("FPEDefaultHUD(Clone)");
        //FPEInputManager = GameObject.Find("FPEInputManager(Clone)");
        ////FPEInteractionManager = GameObject.Find("FPEInteractionManager(Clone)");
        //MuseumGroup = GameObject.Find("FPEPlayerController(Clone)");
    }

    private void Update()
    {
        if (LookItemGroup.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ToMuseum();
        }
    }

    //前往检视界面
    public void LookItem()
    {
        LookItemGroup.SetActive(true);
        MuseumGroup.SetActive(false);
        UIManager.SetActive(false);
        FPEInputManager.SetActive(false);
        //FPEInteractionManager.SetActive(false);
        ItemUI.instance.RefreshItemInfo();
    }

    //返回至博物馆
    public void ToMuseum()
    {
        LookItemGroup.SetActive(false);
        MuseumGroup.SetActive(true);
        //FPEInteractionManager.SetActive(true);
        UIManager.SetActive(true);
        FPEInputManager.SetActive(true);

        Time.timeScale = 1.0f;
        setCursorVisibility(false);
       
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


    private GameObject[] getDontDestroyOnLoadGameObjects()
    {
        var allGameObjects = new List<GameObject>();
        allGameObjects.AddRange(FindObjectsOfType<GameObject>());
        //移除所有场景包含的对象
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            var objs = scene.GetRootGameObjects();
            for (var j = 0; j < objs.Length; j++)
            {
                allGameObjects.Remove(objs[j]);
            }
        }
        //移除父级不为null的对象
        int k = allGameObjects.Count;
        while (--k >= 0)
        {
            if (allGameObjects[k].transform.parent != null)
            {
                allGameObjects.RemoveAt(k);
            }
        }
        return allGameObjects.ToArray();
    }


}
