using UnityEngine;
using UnityEngine.EventSystems;

namespace Whilefun.FPEKit
{

    //
    // FPEMenu
    // This is the base class for all Menus. For your chosen menu to function 
    // correctly, simply ensure that any FPEMenu-derived script is attached to 
    // an appropriate GameObject in your scene.
    //
    // Note: For example use in extending this for your own menu, please see 
    // FPESimpleMenu class
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public abstract class FPEMenu : MonoBehaviour
    {

        private static FPEMenu _instance;
        public static FPEMenu Instance {
            get { return _instance; }
        }

        protected bool menuActive = false;
        protected EventSystem myEventSystem = null;

        public virtual void Awake()
        {

            if (_instance != null)
            {
                Debug.LogWarning("FPEMenu:: Duplicate instance of FPEMenu called '"+ _instance.gameObject.name + "', deleting.");
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }

            

        }


        public virtual void Start()
        {

            menuActive = false;

            myEventSystem = FPEEventSystem.Instance.gameObject.GetComponent<EventSystem>();
            if (!myEventSystem)
            {
                Debug.LogError("FPEMenu:: There is no FPEEventSystem in the scene!");
            }

        }

        public virtual void Update()
        {

        }

        public virtual void activateMenu()
        {
            Debug.LogWarning("FPEMenu.activateMenu() - looks like you forgot to override in a child class.");
        }

        public virtual void deactivateMenu()
        {
            Debug.LogWarning("FPEMenu.activateMenu() - looks like you forgot to override in a child class.");
        }

    }

}