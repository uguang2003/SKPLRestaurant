using UnityEngine;
using UnityEngine.UI;

namespace Whilefun.FPEKit
{

    //
    // FPESimpleMenu
    // An extremely simple example of how the FPEMenu class can be extended in the most basic fashion possible.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPESimpleMenu : FPEMenu
    {

        private GameObject menuCanvas;

        public override void Awake()
        {

            base.Awake();

            menuCanvas = transform.Find("MenuCanvas").gameObject;

            if (!menuCanvas)
            {
                Debug.LogError("FPESimpleMenu:: Cannot find MenuCanvas child object! Menu will not function.");
            }

            menuCanvas.SetActive(false);

        }

        public override void Start()
        {
            base.Start();
        }

        public override void activateMenu()
        {

            if (!menuActive)
            {
                menuCanvas.SetActive(true);
                menuActive = true;
            }

        }

        public override void deactivateMenu()
        {

            if (menuActive)
            {
                menuCanvas.SetActive(false);
                menuActive = false;
            }

        }

        public void exitGameButtonPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    }


}