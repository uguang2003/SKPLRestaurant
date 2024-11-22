using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Whilefun.FPEKit
{

    //
    // FPEMainMenu
    // This special UI class acts similarly to the FPEMenu (also FPEGameMenu and FPESimpleMenu), but is 
    // unique. It is designated strictly for use in the "Main Menu" scene of your game, and nowhere else.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEMainMenu : MonoBehaviour
    {

        private GameObject menuCanvas = null;
        private GameObject beginPanel = null;
        private FPEMenuButton newGameButton = null;
        private FPEMenuButton continueGameButton = null;
        private FPEMenuButton quitGameButton = null;
        private GameObject newGameConfirmationPanel = null;
        private FPEMenuButton[] newGameConfirmationButtons;
        //private Text errorText = null;

        void Start()
        {
            menuCanvas = transform.Find("MenuCanvas").gameObject;
            //errorText = menuCanvas.transform.Find("ErrorText").gameObject.GetComponent<Text>();

            //if (!menuCanvas || !errorText)
            //{
            //    Debug.LogError("FPEMainMenu:: Cannot find MenuCanvas or ErrorText! Did you rename or remove them?");
            //}
            beginPanel = GameObject.Find("BeginPanel");
            // Find buttons - will need to be updated if you add or remove buttons
            FPEMenuButton[] menuButtons = beginPanel.GetComponentsInChildren<FPEMenuButton>();

            for (int t = 0; t < menuButtons.Length; t++)
            {

                if (menuButtons[t].transform.name == "NewGameButton")
                {
                    newGameButton = menuButtons[t];
                }
                else if (menuButtons[t].transform.name == "ContinueGameButton")
                {
                    continueGameButton = menuButtons[t];
                }
                else if (menuButtons[t].transform.name == "QuitGameButton")
                {
                    quitGameButton = menuButtons[t];
                }

            }

            if (!newGameButton || !continueGameButton || !quitGameButton)
            {
                Debug.LogError("FPEMainMenu:: Cannot find one or more of the menu buttons! Did you rename or remove them?");
            }

            newGameConfirmationPanel = beginPanel.transform.Find("NewGameConfirmationPanel").gameObject;

            if (!newGameConfirmationPanel)
            {
                Debug.LogError("FPEMainMenu:: Cannot find one or more of the menu panels! Did you rename or remove them?");
            }

            newGameConfirmationPanel.SetActive(true);
            newGameConfirmationButtons = newGameConfirmationPanel.gameObject.GetComponentsInChildren<FPEMenuButton>();
            newGameConfirmationPanel.SetActive(false);

            refreshButtonStates();

        }

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                exitGame();
            }

        }

        private void refreshButtonStates()
        {

            newGameButton.enableButton();
            quitGameButton.enableButton();

            // Only allow for continue if saved game exists
            if (FPESaveLoadManager.Instance.SavedGameExists())
            {
                continueGameButton.enableButton();
                FPEEventSystem.Instance.GetComponent<EventSystem>().SetSelectedGameObject(continueGameButton.gameObject);
            }
            else
            {
                continueGameButton.disableButton();
                FPEEventSystem.Instance.GetComponent<EventSystem>().SetSelectedGameObject(newGameButton.gameObject);
            }

        }

        public void startNewGame()
        {

            if (FPESaveLoadManager.Instance.StartANewGame())
            {
                FPESaveLoadManager.Instance.ChangeSceneToNoSave(FPESaveLoadManager.Instance.FirstLevelSceneBuildIndex);
                //errorText.text = "";
            }
            else
            {
                //errorText.text = "Something went wrong. Do you have read/write permissions to '" + Application.persistentDataPath + "'?";
            }

        }

        public void confirmStartNewGame()
        {

            if (FPESaveLoadManager.Instance.SavedGameExists())
            {

                newGameConfirmationPanel.SetActive(true);
                // Highlight the 'no' button by default
                FPEEventSystem.Instance.GetComponent<EventSystem>().SetSelectedGameObject(newGameConfirmationButtons[newGameConfirmationButtons.Length - 1].gameObject);

            }
            else
            {
                startNewGame();
            }

        }

        public void hideNewGameConfirmationDialog()
        {

            newGameConfirmationPanel.SetActive(false);
            refreshButtonStates();

        }

        public void continueGame()
        {
            FPESaveLoadManager.Instance.LoadGame();
        }

        public void exitGame()
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

        }

    }

}