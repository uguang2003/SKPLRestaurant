using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whilefun.FPEKit
{

    //
    // FPECore
    // This script allows for simplified handling of game start by ensuring all the required prefabs are created 
    // and placed in your scene when the game starts. It is recommended that the 'FPECore' object 
    // (e.g. FPECore.prebab) be placed in the 1st scene of your game (perhaps the main menu scene). If the first 
    // scene that contained FPECOre is reloaded, you will see a warning printed due to a duplicate instance. You 
    // can ignore this error or comment it out if you know you were loading your first scene again.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPECore : MonoBehaviour
    {

        private static FPECore _instance;
        public static FPECore Instance {
            get { return _instance; }
        }

        [SerializeField, Tooltip("")]
        private GameObject eventSystemPrefab = null;

        [SerializeField, Tooltip("")]
        private GameObject interactionManagerPrefab = null;

        [SerializeField, Tooltip("")]
        private GameObject HUDPrefab = null;

        [SerializeField, Tooltip("")]
        private GameObject playerPrefab = null;

        [SerializeField, Tooltip("")]
        private GameObject inputManagerPrefab = null;

        [SerializeField, Tooltip("")]
        private GameObject saveLoadManagerPrefab = null;

        [SerializeField, Tooltip("")]
        private GameObject menuPrefab = null;

        private bool initialized = false;
        
        void Awake()
        {

            if (_instance != null)
            {

                // This warning is very harmless, and can be ignored. It should indicate that you are returning to the 
                // first level in your game. If you get this warning in OTHER scenes, it means you have an FPECore
                // prefab in that scene, which you don't need. You can also delete the Debug.Log call if you want.
                Debug.Log("FPECore:: Duplicate FPECore, deleting duplicate instance. If you saw this message when loading a saved game into your 1st scene, ignore. Otherwise, it's still not a problem, but might indicate redundant FPECore in a secondary Scene file or something similar.");

                Destroy(this.gameObject);

            }
            else
            {

                _instance = this;
                DontDestroyOnLoad(this.transform);

                if (!initialized)
                {
                    initialized = true;
                    initialize();
                }

            }

        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void initialize()
        {

            if (!HUDPrefab || !eventSystemPrefab || !interactionManagerPrefab || !playerPrefab || !inputManagerPrefab || !saveLoadManagerPrefab || !menuPrefab)
            {
                Debug.LogError("FPECore:: Missing prefab for core component. Game will not function correctly. See Inspector for object '" + gameObject.name + "' to ensure all fields are populated correctly.");
            }

            Instantiate(eventSystemPrefab, null);

            Instantiate(HUDPrefab, null);
            Instantiate(interactionManagerPrefab, null);


            GameObject player = Instantiate(playerPrefab, this.transform);
            FPEPlayerStartLocation startLocation = GameObject.FindObjectOfType<FPEPlayerStartLocation>();

            if (startLocation != null)
            {

                player.transform.position = startLocation.gameObject.transform.position;
                Quaternion flatRotation = Quaternion.Euler(0.0f, startLocation.gameObject.transform.rotation.eulerAngles.y, 0.0f);
                player.transform.rotation = flatRotation;

            }
            else
            {

                Debug.LogWarning("FPECore:: No FPEPlayerStartLocation was found. Placing player at origin");
                player.transform.position = Vector3.zero;
                player.transform.rotation = Quaternion.identity;

            }

            Instantiate(inputManagerPrefab, null);
            Instantiate(saveLoadManagerPrefab, null);
            Instantiate(menuPrefab, null);

            FPEHUD.Instance.initialize();
            FPEInteractionManagerScript.Instance.initialize();

            // Lastly, load game options
            FPESaveLoadManager.Instance.LoadGameOptions();


            // Helper checks //

            // Check if we're in the 0th scene. If we are, and there is no FPEMainMenu object, this will cause issues with player controls. Print an error and exit play mode.
            if(GameObject.FindObjectOfType<FPEMainMenu>() == null && SceneManager.GetActiveScene().buildIndex == 0)
            {
                Debug.LogError("FPECore:: The scene '" + SceneManager.GetActiveScene().name + "' is at index 0, but index 0 is reserved for the Main Menu. This scene does not contain an FPEMainMenu object. You must add one or controls will not work as expected.");
            }

        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

            // If we're in the main menu scene, freeze the player
            if (SceneManager.GetActiveScene().buildIndex == FPESaveLoadManager.Instance.MainMenuSceneBuildIndex)
            {
                FPEInteractionManagerScript.Instance.suspendPlayerAndInteraction();
            }

        }
      
    }

}