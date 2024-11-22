using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Whilefun.FPEKit
{

    //
    // FPESaveLoadManager
    // This script handles basic saving and loading of core game 
    // data on WINDOWS PC PLATFORM ONLY. However, it can be used as
    // a model for making game save management on other platforms.
    //
    // Note: The use of a Binary Formatter for object serialization
    // can also be replaced with something like a JSON library in
    // order to create human-readable save files. No such library
    // was shipped with this asset for licensing and maintenance 
    // reasons.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPESaveLoadManager : MonoBehaviour
    {

        private static FPESaveLoadManager _instance;
        public static FPESaveLoadManager Instance {
            get { return _instance; }
        }

        [Header("UI Components")]
        [SerializeField, Tooltip("The parent game object that houses your save/load status. Should probably just be a Canvas child of this game object.")]
        private GameObject loadingUIParentCanvas = null;
        [SerializeField, Tooltip("The text component that will display the status string while a save or load operation is in progress.")]
        private Text statusText = null;
        [SerializeField, Tooltip("The progress indication spinner. It spins while save/load operation is in progress.")]
        private RectTransform progressSpinner = null;
        [SerializeField, Tooltip("The degrees per second the spinner should spin. Defaulty is 360.")]
        private float spinnerRotationRate = 360.0f;
        [SerializeField, Tooltip("If a save or load takes less time than this, the save/load UI remains displayed for the difference. May help address general UX gulf issues.")]
        private float minimumUIDisplayTime = 0.25f;

        // Operation result tracking
        private bool savingInProgress = false;
        private bool loadingLevelInProgress = false;
        private bool restoringDataInProgress = false;
        private float operationStartTime = 0.0f;
        private bool resetPlayerLook = false;
        private bool returningToMainMenu = false;

        // Scene tracking
        private readonly int invalidSceneIndexToken = -1;
        private int previousSceneIndex = -1;
        private int destinationSceneIndex = -1;

        [Header("Scene Management")]
        [SerializeField, Tooltip("This is the build index of the first 'real level' scene of your game. For example, if your Main Menu scene is at build index 0, and the starting level scene is at build index 1, make this value 1.")]
        private int firstLevelSceneBuildIndex = 1;
        public int FirstLevelSceneBuildIndex { get { return firstLevelSceneBuildIndex; } }

        [SerializeField, Tooltip("The build index of the scene that is considered to be your Main Menu (default is 0)")]
        private int mainMenuSceneBuildIndex = 0;
        public int MainMenuSceneBuildIndex { get { return mainMenuSceneBuildIndex; } }

#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField, Tooltip("If true, debug keys will be enabled in Unity Editor")]
        private bool editorDebugKeys = true;
#endif

        // States
        private enum eOperationStatus
        {
            IDLE = 0,
            SAVING_GAME = 1,
            LOADING_GAME = 2,
            CHANGING_SCENE = 3,
            CHANGING_SCENE_NOSAVE = 4,
            RETURN_TO_MAIN_MENU = 5,
            UX_WAIT = 6
        }
        private eOperationStatus currentOperationStatus = eOperationStatus.IDLE;

        private enum eChangeSceneStatus
        {
            IDLE = 0,
            SAVING = 1,
            LOADING_NEXT_SCENE = 2,
            RESTORING_SCENE_DATA = 3
        }
        private eChangeSceneStatus currentChangeSceneStatus = eChangeSceneStatus.IDLE;

        private enum eLoadGameStatus
        {
            IDLE = 0,
            LOADING_LAST_SCENE = 1,
            RESTORING_SCENE_DATA = 2
        }
        private eLoadGameStatus currentLoadGameStatus = eLoadGameStatus.IDLE;

        // File naming and paths
        private string baseSavePath = "";
        private string autoSavePath = "";
        private string fullSavePath = "";

        public const string autoSaveDirName = "auto";
        public const string fullSaveDirName = "full";
        public const string coreSaveFile = "core.dat";
        public const string playerLocationDataSaveFile = "player.dat";
        public const string inventoryDataSaveFile = "inventory.dat";
        public const string optionsDataSaveFile = "options.dat";
        public const string levelDataFilePrefix = "level_";
        public const string levelDataFilePostfix = ".dat";

        // Base paths
        private string optionsDataSaveFileFullPath = "";
        // Full save paths
        private string fullCoreSaveFileFullPath = "";
        private string fullPlayerLocationDataSaveFileFullPath = "";
        private string fullInventoryDataSaveFileFullPath = "";

        private FPESaveLoadLogic mySaveLoadLogic = null;
        private bool initialized = false;

        void Awake()
        {

            if (_instance != null)
            {
                Debug.LogWarning("AsyncTester:: Duplicate instance of AsyncTester, deleting second one.");
                Destroy(this.gameObject);
            }
            else
            {

                _instance = this;
                DontDestroyOnLoad(this.gameObject);

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

            if (!loadingUIParentCanvas || !statusText || !progressSpinner)
            {
                Debug.Log("FPESaveLoadManager:: UI components are missing or not assigned! (See Inspector for object '" + gameObject.name + "')", gameObject);
            }

            loadingUIParentCanvas.SetActive(false);
            baseSavePath = Application.persistentDataPath;
            autoSavePath = Application.persistentDataPath + "/" + autoSaveDirName;
            fullSavePath = Application.persistentDataPath + "/" + fullSaveDirName;

            // These don't change names at runtime

            // Base
            optionsDataSaveFileFullPath = baseSavePath + "/" + optionsDataSaveFile;
            // Full
            fullCoreSaveFileFullPath = fullSavePath + "/" + coreSaveFile;
            fullPlayerLocationDataSaveFileFullPath = fullSavePath + "/" + playerLocationDataSaveFile;
            fullInventoryDataSaveFileFullPath = fullSavePath + "/" + inventoryDataSaveFile;

            mySaveLoadLogic = gameObject.GetComponent<FPESaveLoadLogic>();

            if (!mySaveLoadLogic)
            {
                Debug.LogError("FPESaveLoadManager:: FPESaveLoadLogic component is missing from object '" + gameObject.name + "'. Did you modify the prefab?");
            }


#if UNITY_EDITOR
            if (editorDebugKeys)
            {
                Debug.Log("FPESaveLoadManager:: Debug keys active! 1: Quick Save 2: Quick Load 4: Delete Saved Game File. To disable, uncheck the 'Editor Debug Keys' checkbox in the inspector of object '" + gameObject.name+"'");
            }
#endif

        }


        void Update()
        {

            #region EDITOR_DEBUG_KEYS

#if UNITY_EDITOR

            if (editorDebugKeys)
            {

                // Debug Keys - to simulate menu button actions, etc. //
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    Debug.Log("Debug Key: Save");
                    SaveGame();
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {

                    if (SavedGameExists())
                    {
                        Debug.Log("Debug Key: Load");
                        LoadGame();
                    }
                    else
                    {
                        Debug.Log("Debug Key: Can't do Load, no saved game file was found");
                    }

                }

                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    Debug.Log("Debug Key: Start a New Game");
                    StartANewGame();
                }

            }
           
#endif
            #endregion

            // If any operation is happening, we keep spinning the progress spinner and keep processing until that operation is complete.
            if (currentOperationStatus != eOperationStatus.IDLE)
            {

                // Animate spinner
                progressSpinner.transform.Rotate(new Vector3(0f, 0f, -spinnerRotationRate * Time.deltaTime));

                // Process current state
                if (currentOperationStatus == eOperationStatus.SAVING_GAME)
                {

                    if (savingInProgress == false)
                    {
                        currentOperationStatus = eOperationStatus.UX_WAIT;
                    }

                }

                else if (currentOperationStatus == eOperationStatus.LOADING_GAME)
                {

                    if (currentLoadGameStatus == eLoadGameStatus.LOADING_LAST_SCENE)
                    {

                        if (loadingLevelInProgress == false)
                        {

                            currentLoadGameStatus = eLoadGameStatus.RESTORING_SCENE_DATA;
                            updateStatusMessage("恢复已保存的游戏数据...");
                            StartCoroutine(restoreDataForCurrentScene(true));

                        }

                    }

                    else if (currentLoadGameStatus == eLoadGameStatus.RESTORING_SCENE_DATA)
                    {

                        if (restoringDataInProgress == false)
                        {

                            currentLoadGameStatus = eLoadGameStatus.IDLE;
                            currentOperationStatus = eOperationStatus.UX_WAIT;

                        }

                    }

                }

                else if (currentOperationStatus == eOperationStatus.CHANGING_SCENE)
                {

                    if (currentChangeSceneStatus == eChangeSceneStatus.SAVING)
                    {

                        if (savingInProgress == false)
                        {

                            // When visiting a level for the first time, there won't be any auto-save data
                            // to restore once we load the new scene. As a result, we won't do the usual
                            // data restoration, which includes an operation to clean up lingering pickups
                            // and inventory items. So, if changing scene and auto-saving, we want to
                            // destroy any left over objects in the DontDestoryOnLoad scene.
                            mySaveLoadLogic.removeAllInventoryInWorld(false);
                            mySaveLoadLogic.removeAllPickupsInWorld(false);

                            currentChangeSceneStatus = eChangeSceneStatus.LOADING_NEXT_SCENE;
                            updateStatusMessage("加载场景" + destinationSceneIndex + "...");
                            StartCoroutine(loadNewScene(destinationSceneIndex));

                        }

                    }
                    else if (currentChangeSceneStatus == eChangeSceneStatus.LOADING_NEXT_SCENE)
                    {

                        if (loadingLevelInProgress == false)
                        {

                            currentChangeSceneStatus = eChangeSceneStatus.RESTORING_SCENE_DATA;
                            updateStatusMessage("恢复数据...");
                            StartCoroutine(restoreDataForCurrentScene(false));

                        }

                    }
                    else if (currentChangeSceneStatus == eChangeSceneStatus.RESTORING_SCENE_DATA)
                    {

                        if (restoringDataInProgress == false)
                        {

                            movePlayerToSuitableEntranceOrStartPosition();
                            currentOperationStatus = eOperationStatus.UX_WAIT;

                        }

                    }

                }
                else if (currentOperationStatus == eOperationStatus.CHANGING_SCENE_NOSAVE)
                {

                    if (currentChangeSceneStatus == eChangeSceneStatus.LOADING_NEXT_SCENE)
                    {

                        if (loadingLevelInProgress == false)
                        {

                            currentChangeSceneStatus = eChangeSceneStatus.RESTORING_SCENE_DATA;
                            updateStatusMessage("恢复数据...");
                            StartCoroutine(restoreDataForCurrentScene(false));

                        }

                    }
                    else if (currentChangeSceneStatus == eChangeSceneStatus.RESTORING_SCENE_DATA)
                    {

                        if (restoringDataInProgress == false)
                        {

                            movePlayerToSuitableEntranceOrStartPosition();
                            currentOperationStatus = eOperationStatus.UX_WAIT;

                        }

                    }

                }
                else if (currentOperationStatus == eOperationStatus.RETURN_TO_MAIN_MENU)
                {

                    if (currentChangeSceneStatus == eChangeSceneStatus.LOADING_NEXT_SCENE)
                    {

                        if (loadingLevelInProgress == false)
                        {
                            currentOperationStatus = eOperationStatus.UX_WAIT;
                        }

                    }

                }
                else if (currentOperationStatus == eOperationStatus.UX_WAIT)
                {

                    if ((Time.time - operationStartTime) > minimumUIDisplayTime)
                    {
                        endOperation();
                    }

                }

            }

        }


        /// <summary>
        /// Initializes save/load operation system for specified operation type.
        /// </summary>
        /// <param name="opType">The type of operation to initialize</param>
        private void initializeOperation(eOperationStatus opType)
        {

            savingInProgress = false;
            loadingLevelInProgress = false;
            restoringDataInProgress = false;
            returningToMainMenu = false;
            currentOperationStatus = opType;

            switch (opType)
            {

                case eOperationStatus.SAVING_GAME:
                    savingInProgress = true;
                    resetPlayerLook = false;
                    updateStatusMessage("保存游戏...");
                    break;

                case eOperationStatus.LOADING_GAME:
                    currentLoadGameStatus = eLoadGameStatus.LOADING_LAST_SCENE;
                    loadingLevelInProgress = true;
                    restoringDataInProgress = true;
                    resetPlayerLook = false;
                    // We also want to stop all diary playback if loading a saved game
                    FPEInteractionManagerScript.Instance.stopAllDiaryPlayback();
                    updateStatusMessage("加载场景...");
                    break;

                case eOperationStatus.CHANGING_SCENE:
                    currentChangeSceneStatus = eChangeSceneStatus.SAVING;
                    savingInProgress = true;
                    loadingLevelInProgress = true;
                    restoringDataInProgress = true;
                    resetPlayerLook = true;
                    updateStatusMessage("切换场景...");
                    break;

                case eOperationStatus.CHANGING_SCENE_NOSAVE:
                    currentChangeSceneStatus = eChangeSceneStatus.LOADING_NEXT_SCENE;
                    loadingLevelInProgress = true;
                    restoringDataInProgress = true;
                    resetPlayerLook = true;
                    updateStatusMessage("切换场景...");
                    break;

                case eOperationStatus.RETURN_TO_MAIN_MENU:
                    currentChangeSceneStatus = eChangeSceneStatus.LOADING_NEXT_SCENE;
                    loadingLevelInProgress = true;
                    returningToMainMenu = true;
                    updateStatusMessage("返回主菜单...");
                    break;

                default:
                    Debug.LogError("Bad operation type '" + opType + "'");
                    break;

            }

            progressSpinner.transform.rotation = Quaternion.identity;
            loadingUIParentCanvas.SetActive(true);

            // Lock player in place and suspend interactions
            FPEInteractionManagerScript.Instance.suspendPlayerAndInteraction();
            FPEInteractionManagerScript.Instance.gameObject.SetActive(false);

            operationStartTime = Time.time;

        }

        /// <summary>
        /// Ends current save operation, resumes play
        /// </summary>
        private void endOperation()
        {

            // Special case if we're returning to main menu
            if(returningToMainMenu)
            {

                loadingUIParentCanvas.SetActive(false);
                currentOperationStatus = eOperationStatus.IDLE;

                // We also need to do some small potential clean up to ensure
                // that any lingering pickups that were in the DoNotDestroyOnLoad 
                // scene are destroyed. 
                mySaveLoadLogic.removeAllPickupsInWorld(true);

                // If player was docked, crouched, etc., ensure they are no longer. This
                // covers the case where player starts new game, docks, quits to main menu
                // then clicks new game again.
                FPEInteractionManagerScript.Instance.resetPlayerOnReturnToMainMenu();

            }
            else
            {

                loadingUIParentCanvas.SetActive(false);
                currentOperationStatus = eOperationStatus.IDLE;
                // Unlock player in place and resume interactions
                FPEInteractionManagerScript.Instance.gameObject.SetActive(true);
                FPEInteractionManagerScript.Instance.resumePlayerAndInteraction(resetPlayerLook);

                FPEInputManager.Instance.FlushInputs();

            }

        }

        /// <summary>
        /// Tells Unity to load a scene file
        /// </summary>
        /// <param name="sceneIndex">The scene's build index to load</param>
        public void loadScene(int sceneIndex)
        {
            loadingLevelInProgress = true;
            SceneManager.LoadScene(sceneIndex);
        }

        /// <summary>
        /// Our listener that receives callback from Unity when the SceneManager is 
        /// finished loading the requested scene
        /// </summary>
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            loadingLevelInProgress = false;
        }

        //
        // These interfaces should be used by the UI to do things like 'New Game', 'Save Game', 'Load Game', etc.
        //
        #region PUBLIC_INTERFACES

        /// <summary>
        /// Performs full game save (scene, objects/states, player location, etc.)
        /// </summary>
        public void SaveGame()
        {
            initializeOperation(eOperationStatus.SAVING_GAME);
            StartCoroutine(saveDataForCurrentScene(true));
        }

        /// <summary>
        /// Loads last manually saved game. Assumes that a check to SavedGameExists() is made prior to trying to load. If this check is not made, an error may occur.
        /// </summary>
        public void LoadGame()
        {

            if (SavedGameExists())
            {

                initializeOperation(eOperationStatus.LOADING_GAME);
                int sceneToLoad = fetchLastSavedGameSceneIndex();
                StartCoroutine(loadNewScene(sceneToLoad));

            }
            else
            {
                Debug.LogError("LoadGame():: Call to SavedGameExists() failed. This function should not have been called. Did you blend debug keys and regular UI calls to Save/Load functions?");
            }

        }

        /// <summary>
        /// Changes the scene to the scene build index specified, and saves current scene's state. Should only really be called by Doorways.
        /// </summary>
        /// <param name="sceneIndex">The build index of the scene to change to</param>
        public void ChangeSceneToAndAutoSave(int sceneIndex)
        {

            initializeOperation(eOperationStatus.CHANGING_SCENE);
            previousSceneIndex = SceneManager.GetActiveScene().buildIndex;
            destinationSceneIndex = sceneIndex;
            StartCoroutine(saveDataForCurrentScene(false));

        }

        /// <summary>
        /// Changes the scene to the scene build index specified, and does not save current scene's state. Should only really be called by Doorways.
        /// </summary>
        /// <param name="sceneIndex">The build index of the scene to change to</param>
        public void ChangeSceneToNoSave(int sceneIndex)
        {

            initializeOperation(eOperationStatus.CHANGING_SCENE_NOSAVE);
            previousSceneIndex = SceneManager.GetActiveScene().buildIndex;
            destinationSceneIndex = sceneIndex;

            // Since we are not auto-saving, we should assume there will never be auto-save data
            // in our destination level either. As a result, our restore data call won't find 
            // any data and won't clean up old objects from the previous scene. As a result, we
            // need to do a manual object clean up for picks and inventory in case they are lingering
            // in the DontDestoryOnLoad scene.
            mySaveLoadLogic.removeAllInventoryInWorld(false);
            mySaveLoadLogic.removeAllPickupsInWorld(false);

            StartCoroutine(loadNewScene(sceneIndex));

        }

        /// <summary>
        /// Returns to the main menu scene, as defined by FPESaveLoadManager.MainMenuSceneBuildIndex
        /// </summary>
        public void ReturnToMainMenu()
        {

            initializeOperation(eOperationStatus.RETURN_TO_MAIN_MENU);
            previousSceneIndex = SceneManager.GetActiveScene().buildIndex;
            destinationSceneIndex = mainMenuSceneBuildIndex;
            StartCoroutine(loadNewScene(destinationSceneIndex));

        }

        /// <summary>
        /// Checks to see if there is a valid core save file present.
        /// </summary>
        /// <returns>True if a core save file exists and is considered valid to load. False if no such file exists.</returns>
        public bool SavedGameExists()
        {

            bool result = false;
            FPESceneSaveData loadedSceneData = null;
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fsCore = null;

            try
            {

                // If core file exists, read in contents and see if save game is valid
                if(File.Exists(fullCoreSaveFileFullPath))
                {

                    fsCore = new FileStream(fullCoreSaveFileFullPath, FileMode.Open);

                    loadedSceneData = (FPESceneSaveData)formatter.Deserialize(fsCore);

                    if (loadedSceneData.LastSavedSceneIndex != invalidSceneIndexToken)
                    {
                        result = true;
                    }
                    //else
                    //{
                    //    Debug.Log("FPESaveLoadManager.SavedGameExists():: Saved game core file at '" + fullCoreSaveFileFullPath + "' was invalid, scene index was '" + loadedSceneData.LastSavedSceneIndex + "'");
                    //}

                }
                // If it does not exist, that's fine, it will be created when player saves their game
                else
                {
                    result = false;
                }

            }
            catch (Exception e)
            {
                Debug.LogError("FPESaveLoadManager.SavedGameExists():: Exception occurred while trying to open '" + fullCoreSaveFileFullPath + "'. Reason: " + e.Message);
                result = false;
            }
            finally
            {

                if (fsCore != null)
                {
                    fsCore.Close();
                }

            }

            return result;

        }


        /// <summary>
        /// This function removes all level save files and resets the core save file back to the invalid token index. After calling this function, SavedGameExists() will return false.
        /// </summary>
        /// <returns>True if starting a new game worked. False if something went wrong (e.g. file permissions issue, etc.)</returns>
        public bool StartANewGame()
        {

            bool result = true;
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fsCore = null;
            FPESceneSaveData sceneData = new FPESceneSaveData(invalidSceneIndexToken);

            try
            {

                // On first play, directories will not exist
                if(Directory.Exists(fullSavePath) == false)
                {
                    Directory.CreateDirectory(fullSavePath);
                }
                if (Directory.Exists(autoSavePath) == false)
                {
                    Directory.CreateDirectory(autoSavePath);
                }

                // Reset Core file
                fsCore = new FileStream(fullCoreSaveFileFullPath, FileMode.Create);
                formatter.Serialize(fsCore, sceneData);

                // Delete player data and inventory data from both auto and full directories
                try
                {

                    // Full
                    if (File.Exists(fullPlayerLocationDataSaveFileFullPath))
                    {
                        File.Delete(fullPlayerLocationDataSaveFileFullPath);
                    }

                    if (File.Exists(fullInventoryDataSaveFileFullPath))
                    {
                        File.Delete(fullInventoryDataSaveFileFullPath);
                    }

                }
                catch (Exception e)
                {
                    //Debug.LogError("FPESaveLoadManager.StartANewGame():: Failed to delete either player save file ('" + fullPlayerLocationDataSaveFileFullPath + "'/'" + autoPlayerLocationDataSaveFileFullPath + "') or inventory save file ('" + fullInventoryDataSaveFileFullPath + "'/'" + autoInventoryDataSaveFileFullPath + "') (or some combination). Reason: " + e.Message);
                    Debug.LogError("FPESaveLoadManager.StartANewGame():: Failed to delete either player save file ('" + fullPlayerLocationDataSaveFileFullPath + "') or inventory save file ('" + fullInventoryDataSaveFileFullPath + "') (or some combination). Reason: " + e.Message);
                }
                finally
                {
                }

                // Delete all level save files in auto directory
                deleteAllLevelDataFromPath(autoSavePath);
                // Delete all level save files in full directory
                deleteAllLevelDataFromPath(fullSavePath);

                // Lastly, in the case that the player quit to the main menu then started a new game immediately, we need to purge the 
                // Inventory from the InventoryManager
                FPEInventoryManagerScript.Instance.clearInventoryItems();
                FPEInventoryManagerScript.Instance.clearAudioDiariesAndNotes();

            }
            catch (Exception e)
            {

                Debug.LogError("FPESaveLoadManager.StartANewGame():: Failed to create core save game file '" + fullCoreSaveFileFullPath + "'. Reason: " + e.Message);
                result = false;

            }
            finally
            {

                if (fsCore != null)
                {
                    fsCore.Close();
                }

            }

            return result;

        }

        /// <summary>
        /// Writes game options to options data file
        /// </summary>
        /// <returns>True if write was successful, false if write failed. On failure, and error message is also printed (likely file write permission issues, etc.)</returns>
        public bool SaveGameOptions()
        {

            bool result = true;
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fsOptions = null;
            FPEGameOptionsSaveData optionsData = mySaveLoadLogic.gatherGameOptionsSaveData();

            try
            {

                fsOptions = new FileStream(optionsDataSaveFileFullPath, FileMode.Create);
                formatter.Serialize(fsOptions, optionsData);

            }
            catch (Exception e)
            {

                Debug.LogError("FPESaveLoadManager:: Failed to save game file(s). Reason: " + e.Message);
                result = false;

            }
            finally
            {

                if (fsOptions != null)
                {
                    fsOptions.Close();
                }

            }

            return result;

        }

        /// <summary>
        /// Reads the game options from file, if file exists.
        /// </summary>
        /// <returns>True if load succeeded, false if it did not. If file did not exist, it is created and 
        /// a warning is printed. If it exists but can not be read (e.g. file permission issue), only
        /// an error is printed.</returns>
        public bool LoadGameOptions()
        {

            bool result = true;

            if (File.Exists(optionsDataSaveFileFullPath))
            {

                FPEGameOptionsSaveData optionsData = null;
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fsOptions = null;

                // Try to actually read in the data from disk
                try
                {

                    fsOptions = new FileStream(optionsDataSaveFileFullPath, FileMode.Open);
                    optionsData = (FPEGameOptionsSaveData)formatter.Deserialize(fsOptions);
                    mySaveLoadLogic.restoreGameOptionsData(optionsData);

                }
                catch (Exception e)
                {

                    Debug.LogError("FPESaveLoadManager:: Failed to load game options file '" + optionsDataSaveFileFullPath + "'. Reason: " + e.Message);
                    result = false;

                }
                finally
                {

                    if (fsOptions != null)
                    {
                        fsOptions.Close();
                    }

                }

            }
            else
            {

                Debug.LogWarning("FPESaveLoadManager:: Game options file '" + optionsDataSaveFileFullPath + "' does not exist. Attempting to save existing options so a reload can occur.");
                SaveGameOptions();
                result = false;

            }

            return result;

        }

        #endregion

        /// <summary>
        /// Applies specified string to the blocking UIs status string (e.g. "Restoring data...") while progress spinner is spinning
        /// </summary>
        /// <param name="msg">The string that should appear on the UI. Should be indicative of currently operation/phase (e.g. 'Reticulating Splines...', etc.)</param>
        private void updateStatusMessage(string msg)
        {
            statusText.text = msg;
        }

        /// <summary>
        /// Simply checks for and returns the scene index from the core save file, per the last manual save operation.
        /// </summary>
        /// <returns>The last manually saved scene index, or invalidSceneIndexToken if no such save was present.</returns>
        private int fetchLastSavedGameSceneIndex()
        {

            int lastSavedGameSceneIndex = invalidSceneIndexToken;
            FileStream fsCore = null;
            FPESceneSaveData loadedSceneData = null;
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {

                fsCore = new FileStream(fullCoreSaveFileFullPath, FileMode.Open);
                loadedSceneData = (FPESceneSaveData)formatter.Deserialize(fsCore);
                lastSavedGameSceneIndex = loadedSceneData.LastSavedSceneIndex;

            }
            catch (Exception e)
            {
                Debug.LogError("FPESaveLoadManager:: Failed to load core save file '"+ fullCoreSaveFileFullPath + "'. Reason: " + e.Message);
            }
            finally
            {

                if (fsCore != null)
                {
                    fsCore.Close();
                }

            }

            return lastSavedGameSceneIndex;

        }

        /// <summary>
        /// Moves the player to the currently loaded scene's most appropriate starting position. If a 
        /// new game (previous scene was not "real level" - less than firstLevelSceneBuildIndex), a 
        /// FPEPlayerStartLocation is searched for. If not, a Doorway Entrance is searched for. If 
        /// neither can be found, an error is printed and the player is placed at the world origin.
        /// </summary>
        private void movePlayerToSuitableEntranceOrStartPosition()
        {

            GameObject thePlayer = FPEPlayer.Instance.gameObject;

            // If previous scene was LESS than our first known real level, we assume this is a new game
            if (previousSceneIndex < firstLevelSceneBuildIndex)
            {

                FPEPlayerStartLocation startLocation = GameObject.FindObjectOfType<FPEPlayerStartLocation>();

                // Yield to start location if there is one present
                if (startLocation != null)
                {

                    thePlayer.transform.position = startLocation.gameObject.transform.position;
                    Quaternion flatRotation = Quaternion.Euler(0.0f, startLocation.gameObject.transform.rotation.eulerAngles.y, 0.0f);
                    thePlayer.transform.rotation = flatRotation;

                }
                else
                {

                    Debug.LogWarning("FPESaveLoadManager:: No FPEPlayerStartLocation was found in scene. Placing player at origin instead.");
                    thePlayer.transform.position = Vector3.zero;
                    thePlayer.transform.rotation = Quaternion.identity;

                }

            }
            // Otherwise, yield to the appropriate doorway.
            else
            {

                FPEDoorway[] doorways = GameObject.FindObjectsOfType<FPEDoorway>();
                bool foundDoorway = false;
                
                for (int d = 0; d < doorways.Length; d++)
                {

                    if (doorways[d].ConnectedSceneIndex == previousSceneIndex)
                    {

                        thePlayer.transform.position = doorways[d].DoorwayEntranceTransform.position;
                        thePlayer.transform.rotation = doorways[d].DoorwayEntranceTransform.rotation;
                        foundDoorway = true;
                        break;

                    }

                }

                if (foundDoorway == false)
                {

                    Debug.LogWarning("FPESaveLoadManager:: No FPEDoorway was found that matched connected scene '" + previousSceneIndex + "'. Placing player at origin instead.");
                    thePlayer.transform.position = Vector3.zero;
                    thePlayer.transform.rotation = Quaternion.identity;

                }

            }

        }

        private bool deleteAllLevelDataFromPath(string path)
        {

            bool result = true;

            // Delete all level save files in specified directory
            DirectoryInfo dirFull = new DirectoryInfo(path);

            foreach (FileInfo f in dirFull.GetFiles("*" + levelDataFilePrefix + "*" + levelDataFilePostfix))
            {

                try
                {
                    File.Delete(f.FullName);
                }
                catch (Exception e)
                {
                    Debug.LogError("FPESaveLoadManager.StartANewGame():: Failed to delete save file '" + f.FullName + "'. Reason: " + e.Message);
                    result = false;
                }
                finally
                {
                }

            }

            return result;

        }

        private bool copyAllLevelDataFromPathToPath(string sourcePath, string destPath)
        {

            bool result = true;

            // First, delete all level data from destinationPath
            if (deleteAllLevelDataFromPath(destPath) == false)
            {
                Debug.LogError("FPESaveLoadManager.copyAllLevelDataFromPathToPath():: Failed to delete existing level data from '" + destPath + "'");
                result = false;
            }
            else
            {

                // Then, copy over all level data files
                string[] levelDataFilesInFromPath = Directory.GetFiles(sourcePath, ("*" + levelDataFilePrefix + "*" + levelDataFilePostfix));

                foreach (string s in levelDataFilesInFromPath)
                {

                    string filename = s.Substring(sourcePath.Length + 1);

                    try
                    {
                        // Copy file from sourcePath to destPath
                        File.Copy(Path.Combine(sourcePath, filename), Path.Combine(destPath, filename));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("FPESaveLoadManager.copyAllLevelDataFromPathToPath():: Failed to Copy level data file '" + filename + "' (full path '" + s + "'). Reason: " + e.Message);
                        result = false;
                    }
                    finally
                    {
                    }

                }

            }

            return result;

        }
        
        #region COROUTINES

        /// <summary>
        /// Simply calls loadScene which tells Unity to load the specified scene
        /// </summary>
        /// <param name="sceneIndex">The scene build index of the scene to load</param>
        /// <returns>null</returns>
        private IEnumerator loadNewScene(int sceneIndex)
        {

            loadScene(sceneIndex);
            yield return null;

        }

        /// <summary>
        /// Saves all relevant game data per the scene and specified fullSave value.
        /// </summary>
        /// <param name="fullSave">If true, 'last manually saved scene' and player location data are also saved. If false, these are skipped and just the other scene object data is saved.</param>
        /// <returns>True if operation was a success. False if there was an error. In the case of an error, an exception likely occured, and an exception message will be printed.</returns>
        private IEnumerator saveDataForCurrentScene(bool fullSave)
        {

            float operationStartTime = Time.time;
            bool result = true;
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fsCore = null;
            FileStream fsPlayer = null;
            FileStream fsInventory = null;
            FileStream fsLevel = null;

            FPESceneSaveData sceneData = mySaveLoadLogic.gatherSceneData();
            FPEInventorySaveData inventoryData = mySaveLoadLogic.gatherInventorySaveData();
            FPEPlayerStateSaveData playerLocationData = mySaveLoadLogic.gatherPlayerData();
            FPEInventoryWorldSaveData[] worldInventoryData = mySaveLoadLogic.gatherInventoryInWorld();
            FPEPickupWorldSaveData[] worldPickupData = mySaveLoadLogic.gatherPickupsInWorld();
            FPETriggerSaveData[] triggerSaveData = mySaveLoadLogic.gatherTriggerData();
            FPEActivateSaveData[] activateSaveData = mySaveLoadLogic.gatherActivateTypeData();
            FPEAttachedNoteSaveData[] attachedNoteSaveData = mySaveLoadLogic.gatherAttachedNoteTypeData();
            FPEAudioDiaryPlayedStateSaveData[] playedDiarySaveData = mySaveLoadLogic.gatherAudioDiaryPlayedStateData();
            FPEJournalSaveData[] journalSaveData = mySaveLoadLogic.gatherJournalSaveData();
            FPEDoorSaveData[] doorSaveData = mySaveLoadLogic.gatherDoorTypeData();
            FPEGenericObjectSaveData[] genericSaveData = mySaveLoadLogic.gatherGenericSaveTypeData();
            FPEDrawerSaveData[] drawerSaveData = mySaveLoadLogic.gatherDrawerTypeData();
            
            //
            // Your additional Custom Save/Load logic for custom save data types goes here
            //
            
            // Try to write the gathered data to applicable save files on disk
            try
            {

                // We only want to save player location data on manual save operation. When "auto-saving" on change of scene via Doorway, we just want to save the non-player level data.
                if (fullSave)
                {

                    // We also only want to write to core save file on a manual save operation. Auto-saves on level change do not count as a "full save"
                    fsCore = new FileStream(fullCoreSaveFileFullPath, FileMode.Create);
                    formatter.Serialize(fsCore, sceneData);

                    fsPlayer = new FileStream(fullPlayerLocationDataSaveFileFullPath, FileMode.Create);
                    formatter.Serialize(fsPlayer, playerLocationData);

                    fsInventory = new FileStream(fullInventoryDataSaveFileFullPath, FileMode.Create);
                    formatter.Serialize(fsInventory, inventoryData);

                }

                fsLevel = new FileStream(autoSavePath + "/" + levelDataFilePrefix + sceneData.LastSavedSceneIndex + levelDataFilePostfix, FileMode.Create);

                formatter.Serialize(fsLevel, worldInventoryData);
                formatter.Serialize(fsLevel, worldPickupData);
                formatter.Serialize(fsLevel, triggerSaveData);
                formatter.Serialize(fsLevel, activateSaveData);
                formatter.Serialize(fsLevel, attachedNoteSaveData);
                formatter.Serialize(fsLevel, playedDiarySaveData);
                formatter.Serialize(fsLevel, journalSaveData);
                formatter.Serialize(fsLevel, doorSaveData);
                formatter.Serialize(fsLevel, genericSaveData);
                formatter.Serialize(fsLevel, drawerSaveData);

                // If performing a full save, we also want to flush all previous FULL save data from full directory, and replace it will the stuff we just saved to auto
                if (fullSave)
                {
                    copyAllLevelDataFromPathToPath(autoSavePath, fullSavePath);
                }

            }
            catch (Exception e)
            {

                Debug.LogError("FPESaveLoadManager:: Failed to save game file(s). Did you make a call the SaveGame() before ever calling StartANewGame()? Reason: " + e.Message);
                result = false;

            }
            finally
            {

                if (fsCore != null)
                {
                    fsCore.Close();
                }

                if (fsLevel != null)
                {
                    fsLevel.Close();
                }
                                
                if(fsPlayer != null)
                {
                    fsPlayer.Close();
                }

                if(fsInventory != null)
                {
                    fsInventory.Close();
                }

            }

            // Last thing is to clear the progress flag to indicate we're done
            savingInProgress = false;

            yield return result;

        }

        /// <summary>
        /// Restores saved level data to the currently loaded scene, if it exists.
        /// </summary>
        /// <param name="fullLoad">If true, player location and inventory data will be loaded, and player will be relocated in world space per that data.</param>
        /// <returns>True if operation was a success. False if there was an error. In the case of an error, an exception likely occured, and an exception message will be printed.</returns>
        private IEnumerator restoreDataForCurrentScene(bool fullLoad)
        {

            bool result = true;

            // If doing a full load, we want to copy all 'full' save level data files into the 'auto' directory, then restore from there.
            if (fullLoad)
            {
                copyAllLevelDataFromPathToPath(fullSavePath, autoSavePath);
            }

            string levelDataFilename = autoSavePath + "/" + levelDataFilePrefix + SceneManager.GetActiveScene().buildIndex + levelDataFilePostfix;

            if (File.Exists(levelDataFilename))
            {

                FPEPlayerStateSaveData loadedPlayerLocationData = null;
                FPEInventorySaveData loadedInventoryData = null;
                FPEInventoryWorldSaveData[] loadedWorldInventoryData = null;
                FPEPickupWorldSaveData[] loadedWorldPickupData = null;
                FPETriggerSaveData[] loadedTriggerData = null;
                FPEActivateSaveData[] loadedActivateData = null;
                FPEAttachedNoteSaveData[] loadedAttachedNoteData = null;
                FPEAudioDiaryPlayedStateSaveData[] loadedAudioDiaryPlayedData = null;
                FPEJournalSaveData[] loadedJournalData = null;
                FPEDoorSaveData[] loadedDoorData = null;
                FPEGenericObjectSaveData[] loadedGenericData = null;
                FPEDrawerSaveData[] loadedDrawerData = null;

                BinaryFormatter formatter = new BinaryFormatter();

                FileStream fsPlayer = null;
                FileStream fsInventory = null;
                FileStream fsLevel = null;

                // Try to actually read in the data from disk
                try
                {

                    fsLevel = new FileStream(levelDataFilename, FileMode.Open);
                    
                    // Note: Read the data in from file in the same order it was written to file
                    loadedWorldInventoryData = (FPEInventoryWorldSaveData[])formatter.Deserialize(fsLevel);
                    loadedWorldPickupData = (FPEPickupWorldSaveData[])formatter.Deserialize(fsLevel);
                    loadedTriggerData = (FPETriggerSaveData[])formatter.Deserialize(fsLevel);
                    loadedActivateData = (FPEActivateSaveData[])formatter.Deserialize(fsLevel);
                    loadedAttachedNoteData = (FPEAttachedNoteSaveData[])formatter.Deserialize(fsLevel);
                    loadedAudioDiaryPlayedData = (FPEAudioDiaryPlayedStateSaveData[])formatter.Deserialize(fsLevel);
                    loadedJournalData = (FPEJournalSaveData[])formatter.Deserialize(fsLevel);
                    loadedDoorData = (FPEDoorSaveData[])formatter.Deserialize(fsLevel);
                    loadedGenericData = (FPEGenericObjectSaveData[])formatter.Deserialize(fsLevel);
                    loadedDrawerData = (FPEDrawerSaveData[])formatter.Deserialize(fsLevel);

                    //
                    // Your additional Custom Save/Load logic for custom save data types goes here
                    //

                    // Triggers and Activate types
                    mySaveLoadLogic.restoreTriggerData(loadedTriggerData);
                    mySaveLoadLogic.restoreActivateData(loadedActivateData);

                    // Doors
                    mySaveLoadLogic.restoreDoorData(loadedDoorData);
                    // Drawers
                    mySaveLoadLogic.restoreDrawerData(loadedDrawerData);

                    // Generic Saveable Objects
                    mySaveLoadLogic.restoreGenericSaveTypeData(loadedGenericData);
                    
                    // Inventory type objects in the world
                    mySaveLoadLogic.removeAllInventoryInWorld(fullLoad);
                    mySaveLoadLogic.createWorldInventory(loadedWorldInventoryData);

                    // Pickup type objects in the world
                    mySaveLoadLogic.removeAllPickupsInWorld(fullLoad);
                    mySaveLoadLogic.createWorldPickups(loadedWorldPickupData);

                    // Attached Notes, Diaries, and Journals
                    // Note: We restore these last, because they can be attached to Inventory Items and Pickups or other interactables. If for example, we loaded these BEFORE restoring pickups,
                    // the associated pickups would get their attached note/diary data restored, then be deleted and replaced with a fresh copy of the prefab, thus erasing the restored note/diary 
                    // state. This way, we ensure the 'final' restored prefab of a pickup/inventory item is in place and loaded before we restore its attached note or diary status.
                    mySaveLoadLogic.restoreAttachedNoteData(loadedAttachedNoteData);
                    mySaveLoadLogic.restoreAudioDiaryPlaybackStateData(loadedAudioDiaryPlayedData);
                    mySaveLoadLogic.restoreJournalData(loadedJournalData);

                    // Only restore player location and move them if required. We do this last because the functions above may destroy held object, depending on fullLoad value.
                    if (fullLoad)
                    {

                        fsPlayer = new FileStream(fullPlayerLocationDataSaveFileFullPath, FileMode.Open);
                        loadedPlayerLocationData = (FPEPlayerStateSaveData)formatter.Deserialize(fsPlayer);
                        mySaveLoadLogic.relocatePlayer(loadedPlayerLocationData);

                        fsInventory = new FileStream(fullInventoryDataSaveFileFullPath, FileMode.Open);
                        loadedInventoryData = (FPEInventorySaveData)formatter.Deserialize(fsInventory);
                        mySaveLoadLogic.restoreInventorySaveData(loadedInventoryData);

                    }

                }
                catch (Exception e)
                {

                    Debug.LogError("FPESaveLoadManager:: Failed to load game file. Reason: " + e.Message);
                    result = false;

                }
                finally
                {

                    if (fsLevel != null)
                    {
                        fsLevel.Close();
                    }

                    if (fsPlayer != null)
                    {
                        fsPlayer.Close();
                    }

                    if(fsInventory != null)
                    {
                        fsInventory.Close();
                    }

                }

            }

            // Last thing is to clear the progress flag to indicate we're done
            restoringDataInProgress = false;

            yield return result;

        }

        #endregion

    }

}