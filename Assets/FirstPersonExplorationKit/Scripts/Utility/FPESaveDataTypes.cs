using UnityEngine;
using System;

namespace Whilefun.FPEKit
{

    //
    // This file contains all the Serializable classes that are created 
    // from game objects and written to and read from save files. If you create 
    // your own custom save data type, it should go here.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //

    #region BASIC_TYPES

    // Generic Vector2
    [Serializable]
    public class FPEVector2
    {

        protected float[] data;

        public FPEVector2(Vector2 v)
        {
            data = new float[2];
            data[0] = v.x;
            data[1] = v.y;
        }

        public Vector2 getVector2()
        {
            return new Vector2(data[0], data[1]);
        }

        public override string ToString()
        {
            return "FPEVector2:(" + data[0] + "," + data[1] + ")";
        }

    }

    // Generic Vector3
    [Serializable]
    public class FPEVector3
    {

        protected float[] data;

        public FPEVector3(Vector3 v)
        {
            data = new float[3];
            data[0] = v.x;
            data[1] = v.y;
            data[2] = v.z;
        }

        public Vector3 getVector3()
        {
            return new Vector3(data[0], data[1], data[2]);
        }

        public override string ToString()
        {
            return "FPEVector3:(" + data[0] + "," + data[1] + "," + data[2] + ")";
        }

    }

    // Quaternion
    [Serializable]
    public class FPEQuaternion
    {

        protected float[] data;

        public FPEQuaternion(Quaternion q)
        {

            data = new float[4];
            data[0] = q.x;
            data[1] = q.y;
            data[2] = q.z;
            data[3] = q.w;

        }

        public Quaternion getQuaternion()
        {
            return new Quaternion(data[0], data[1], data[2], data[3]);
        }

        public override string ToString()
        {
            return "FPEQUaternion:(" + data[0] + "," + data[1] + "," + data[2] + "," + data[3] + ")";
        }

    }

    // Generic objects Tranform position/rotation. Can be extended for saving Transform based objects that have additional data.
    [Serializable]
    public class FPETransform
    {

        protected float[] positionFloats;
        protected float[] rotationFloats;

        public FPETransform(Vector3 pos, Quaternion rot)
        {

            positionFloats = new float[3];
            positionFloats[0] = pos.x;
            positionFloats[1] = pos.y;
            positionFloats[2] = pos.z;

            rotationFloats = new float[4];
            rotationFloats[0] = rot.x;
            rotationFloats[1] = rot.y;
            rotationFloats[2] = rot.z;
            rotationFloats[3] = rot.w;

        }

        public Vector3 getPosition()
        {
            return new Vector3(positionFloats[0], positionFloats[1], positionFloats[2]);
        }

        public Quaternion getRotation()
        {
            return new Quaternion(rotationFloats[0], rotationFloats[1], rotationFloats[2], rotationFloats[3]);
        }

        public string getPositionRotationString()
        {
            return "FPETransform: Pos(" + positionFloats[0] + "," + positionFloats[1] + "," + positionFloats[2] + "), Rot(" + rotationFloats[0] + "," + rotationFloats[1] + "," + rotationFloats[2] + "," + rotationFloats[2] + ")";
        }

    }

    #endregion

    #region COMPLEX_TYPES

    // Inventory objects in the world
    [Serializable]
    public class FPEInventoryWorldSaveData : FPETransform
    {

        private FPEInventoryManagerScript.eInventoryItems invType;
        public FPEInventoryManagerScript.eInventoryItems InventoryItemType {
            get { return invType; }
        }

        public FPEInventoryWorldSaveData(Vector3 pos, Quaternion rot, FPEInventoryManagerScript.eInventoryItems type) : base(pos, rot)
        {
            invType = type;
        }

        public override string ToString()
        {

            string result = "InventoryWorldObject: TYPE='" + invType + "',";
            result += "POS=(" + positionFloats[0] + "," + positionFloats[1] + "," + positionFloats[2] + "),";
            result += "ROT=(" + rotationFloats[0] + "," + rotationFloats[1] + "," + rotationFloats[2] + ", " + rotationFloats[3] + ")";
            return result;

        }

    }
    
    // Pickup objects in the world
    [Serializable]
    public class FPEPickupWorldSaveData : FPETransform
    {

        private string prefabName;
        public string PrefabName {
            get { return prefabName; }
        }

        public FPEPickupWorldSaveData(Vector3 pos, Quaternion rot, string name) : base(pos, rot)
        {
            prefabName = name;
        }

        public override string ToString()
        {

            string result = "PickupWorldObject: NAME='" + prefabName + "',";
            result += "POS=(" + positionFloats[0] + "," + positionFloats[1] + "," + positionFloats[2] + "),";
            result += "ROT=(" + rotationFloats[0] + "," + rotationFloats[1] + "," + rotationFloats[2] + ", " + rotationFloats[3] + ")";
            return result;

        }

    }

    // Player position data
    [Serializable]
    public class FPEPlayerStateSaveData
    {

        // Position/Look
        private FPETransform playerTransform;
        private FPEVector3 playerFocus;
        // Ability states
        private bool crouching;
        public bool Crouching { get { return crouching; } }

        // Dock data: Note we can't access the save menu while not IDLE, so we don't have to save docking state.
        private bool docked;
        public bool Docked { get { return docked; } }
        private string curDockName;
        public string DockName { get { return curDockName; } }
        private FPEVector2 maxAngles;
        public Vector3 MaxAngles { get { return maxAngles.getVector2(); } }
        private FPEVector3 tarFocalPos;
        public Vector3 TargetFocalPos { get { return tarFocalPos.getVector3(); } }
        private FPEVector3 tarDockPos;
        public Vector3 TargetDockPos { get { return tarDockPos.getVector3(); } }
        private FPEVector3 prevFocalPos;
        public Vector3 PreviousFocalPos { get { return prevFocalPos.getVector3(); } }
        private FPEVector3 prevWorldPos;
        public Vector3 PreviousWorldPos { get { return prevWorldPos.getVector3(); } }

        public FPEPlayerStateSaveData(Transform player, Vector3 focalPoint, bool isCrouching, bool playerDocked, string dockName, Vector2 targetMaxAngles, Vector3 targetFocalPoint, Vector3 targetDockPosition, Vector3 previousFocalPoint, Vector3 previousWorldPosition)
        {

            playerTransform = new FPETransform(player.position, player.rotation);
            playerFocus = new FPEVector3(focalPoint);
            crouching = isCrouching;
            docked = playerDocked;
            curDockName = dockName;
            maxAngles = new FPEVector2(targetMaxAngles);
            tarFocalPos = new FPEVector3(targetFocalPoint);
            tarDockPos = new FPEVector3(targetDockPosition);
            prevFocalPos = new FPEVector3(previousFocalPoint);
            prevWorldPos = new FPEVector3(previousWorldPosition);

        }

        public Vector3 playerPosition()
        {
            return playerTransform.getPosition();
        }

        public Quaternion playerRotation()
        {
            return playerTransform.getRotation();
        }

        public Vector3 playerLookAt()
        {
            return playerFocus.getVector3();
        }

        public override string ToString()
        {

            string result = "PlayerLocationData:\n";
            result += "PLAYER: " + playerTransform.getPositionRotationString() + "\n";
            result += "FOCUS: " + playerFocus.ToString() + "\n";
            result += "CROUCHING: " + crouching + "\n";
            result += "DOCKED: " + docked + "\n";
            result += "MAXANGLES: " + maxAngles.ToString() + "\n";
            result += "TARFOCUS: " + tarFocalPos.ToString() + "\n";
            result += "TARDOCK: " + tarDockPos.ToString() + "\n";
            result += "PREVFOCUS: " + prevFocalPos.ToString() + "\n";
            result += "PREVWORLDPOS: " + prevWorldPos.ToString() + "\n";
            return result;

        }

    }

    // Trigger Type objects
    [Serializable]
    public class FPETriggerSaveData
    {

        private string objName;
        public string ObjectName { get { return objName; } }
        private bool armed;
        public bool Armed { get { return armed; } }
        private bool wasTripped;
        public bool Tripped { get { return wasTripped; } }

        public FPETriggerSaveData(string name, bool isArmed, bool tripped)
        {
            objName = name;
            armed = isArmed;
            wasTripped = tripped;
        }

        public override string ToString()
        {
            return "TRIGGER: '" + objName + "' (armed='" + armed + "',tripped='" + wasTripped + "')";
        }

    }

    // Activate Type Objects
    [Serializable]
    public class FPEActivateSaveData
    {

        private string objName;
        public string ObjectName { get { return objName; } }
        private bool firedOnce;
        public bool FiredOnce { get { return firedOnce; } }
        private bool toggleCurrentlyOn;
        public bool ToggleCurrentlyOn { get { return toggleCurrentlyOn; } }
        private string interactString;
        public string InteractionString { get { return interactString; } }

        public FPEActivateSaveData(string objectName, bool eventFiredOnce, bool toggleOn, string interactionString)
        {

            objName = objectName;
            firedOnce = eventFiredOnce;
            toggleCurrentlyOn = toggleOn;
            interactString = interactionString;

        }

        public override string ToString()
        {
            return "ACTIV.: '" + objName + "' (" + firedOnce + "," + toggleCurrentlyOn + ","+interactString+")";
        }

    }

    // Attached Note Type Objects
    [Serializable]
    public class FPEAttachedNoteSaveData
    {

        private string objName;
        public string ObjectName { get { return objName; } }
        private bool collected;
        public bool Collected { get { return collected; } }

        public FPEAttachedNoteSaveData(string objectName, bool hasBeenCollected)
        {

            objName = objectName;
            collected = hasBeenCollected;

        }

        public override string ToString()
        {
            return "FPEAttachedNoteSaveData: '" + objName + "' (" + collected + ")";
        }

    }

    // Audio Diary Type Objects
    [Serializable]
    public class FPEAudioDiaryPlayedStateSaveData
    {

        private string objName;
        public string ObjectName { get { return objName; } }
        private bool played;
        public bool HasBeenPlayed { get { return played; } }

        public FPEAudioDiaryPlayedStateSaveData(string objectName, bool hasBeenPlayed)
        {

            objName = objectName;
            played = hasBeenPlayed;

        }

        public override string ToString()
        {
            return "FPEAudioDiaryPlayedStateSaveData: '" + objName + "' (" + played + ")";
        }

    }

    // Journal Type Objects
    [Serializable]
    public class FPEJournalSaveData
    {

        private string objName;
        public string ObjectName { get { return objName; } }
        private bool read;
        public bool HasBeenRead { get { return read; } }

        public FPEJournalSaveData(string objectName, bool hasBeenRead)
        {
            objName = objectName;
            read = hasBeenRead;
        }

        public override string ToString()
        {
            return "FPEJournalSaveData: '"+objName+"' ('"+read+"')";
        }

    }

    // Scene Data
    [Serializable]
    public class FPESceneSaveData
    {

        private int lastSavedSceneIndex;
        public int LastSavedSceneIndex { get { return lastSavedSceneIndex; } }

        public FPESceneSaveData(int index)
        {
            lastSavedSceneIndex = index;
        }

        public override string ToString()
        {
            return "FPESceneSaveData: lastSavedSceneIndex='" + lastSavedSceneIndex + "'";
        }

    }

    #endregion

    #region INVENTORY_DATA_TYPES

    [Serializable]
    public class FPEHeldObjectSaveData
    {

        private bool heldSomething;
        public bool HeldSomething { get { return heldSomething; } }
        private string puPrefabName;
        public string PickupPrefabName { get { return puPrefabName; } }
        private FPEInventoryManagerScript.eInventoryItems invItemType;
        public FPEInventoryManagerScript.eInventoryItems InventoryItemType { get { return invItemType; } }
        private FPEQuaternion localRot;
        public Quaternion LocalRotation {
            get { return localRot.getQuaternion(); }
        }

        /// <summary>
        /// Used to house player's current held object data. If the player was not holding anything, just make first param false and the rest will be ignored.
        /// </summary>
        /// <param name="playerWasHoldingSomething">If player was holding something, pass true. If not, pass false</param>
        /// <param name="pickupPrefabName">Name of the Pickup type object's prefab in the case the player was holding a Pickup type object</param>
        /// <param name="inventoryItemType">Inventory Item Type, in the case the player was holding an InventoryItem type</param>
        /// <param name="localRotation">The last local rotation of the held object when it was in the player's hand</param>
        public FPEHeldObjectSaveData(bool playerWasHoldingSomething, string pickupPrefabName, FPEInventoryManagerScript.eInventoryItems inventoryItemType, Quaternion localRotation)
        {

            heldSomething = playerWasHoldingSomething;
            puPrefabName = pickupPrefabName;
            invItemType = inventoryItemType;
            localRot = new FPEQuaternion(localRotation);

        }

        public override string ToString()
        {

            string result = "PEHeldObjectSaveData: ";

            if (heldSomething)
            {
                result += "puPrefabName '" + puPrefabName + "', invItemType(as int) '" + invItemType + "', localRot '" + localRot.ToString() + "'";
            }
            else
            {
                result += "[was not holding anything]";
            }

            return result;

        }

    }

    // Inventory Items (in inventory, not in world)
    [Serializable]
    public class FPEInventoryItemSaveData
    {

        private FPEInventoryManagerScript.eInventoryItems invType;
        public FPEInventoryManagerScript.eInventoryItems InventoryItemType {
            get { return invType; }
        }

        public FPEInventoryItemSaveData(FPEInventoryManagerScript.eInventoryItems type)
        {
            invType = type;
        }

        public override string ToString()
        {
            return "FPEInventoryItemSaveData: '" + invType + "'";
        }

    }

    // Audio Diaries
    [Serializable]
    public class FPEAudioDiaryEntrySaveData
    {

        private string title;
        private string clipName;
        private bool showTitle;

        public FPEAudioDiaryEntrySaveData(string diaryTitle, string audioClipName, bool showDiaryTitle)
        {
            title = diaryTitle;
            clipName = audioClipName;
            showTitle = showDiaryTitle;
        }

        /// <summary>
        /// Attempts to create and return an Audio Diary Entry from the saved data.
        /// </summary>
        /// <returns>An FPEAudioDiaryEntry from the saved data, or null if the saved data was not valid or recoverable (e.g. missing audio files)</returns>
        public FPEAudioDiaryEntry getAudioDiaryEntry()
        {

            AudioClip clip = Resources.Load(FPEObjectTypeLookup.AudioDiaryResourcePath + clipName) as AudioClip;

            if(clip != null)
            {
                return new FPEAudioDiaryEntry(title, clip, showTitle);
            }
            else
            {
                Debug.LogError("FPEAudioDiaryEntry: Could not locate Audio Clip '"+ clipName + "' inside the Resources '"+ FPEObjectTypeLookup.AudioDiaryResourcePath + "' sub folder. Audio Diary with title '"+ title + "' will not play correctly. Returning null.");
                return null;
            }

        }

        public override string ToString()
        {
            return "FPEAudioDiarySaveData: title '"+title+"', clipName '"+ clipName + "'";
        }

    }

    // Note Entries
    // Potential optimization: Depending on requirements, you could create ScriptableObject(s) or other class file that houses 
    // all the note titles and bodies, which get referenced by a note ID and are retrieved via look up table. This would make 
    // cases where there are a ton of long notes cut down on save game file size. As it stands, even having dozens of notes with
    // hundreds of words, the save file size will remain reasonable.
    [Serializable]
    public class FPENoteSaveData
    {

        private string title;
        private string body;

        public FPENoteSaveData(string noteTitle, string noteBody)
        {
            title = noteTitle;
            body = noteBody;
        }

        public FPENoteEntry getNoteEntry()
        {

            return new FPENoteEntry(title, body);

        }

        public override string ToString()
        {
            return "FPENoteSaveData: title '"+ title + "', bodyPreview '"+ body.Substring(0, Mathf.Min(20, body.Length))+ "'";
        }

    }

    // Inventory Data
    [Serializable]
    public class FPEInventorySaveData
    {

        private FPEHeldObjectSaveData heldObjData;
        public FPEHeldObjectSaveData HeldObjectData { get { return heldObjData; } }
        private FPEInventoryItemSaveData[] invItems;
        public FPEInventoryItemSaveData[] InventoryItemData { get { return invItems; } }
        FPEAudioDiaryEntrySaveData[] audioDiaryData;
        public FPEAudioDiaryEntrySaveData[] AudioDiaryData { get { return audioDiaryData; } }
        FPENoteSaveData[] noteData;
        public FPENoteSaveData[] NoteData { get { return noteData; } }

        /// <summary>
        /// Houses a representation of player's currently held object, inventory items, audio diaries, and note entries from their inventory
        /// </summary>
        /// <param name="heldObjectData">Object data for held object</param>
        /// <param name="inventoryItems">Inventory item data</param>
        /// <param name="audioDiaries">Audio diary data</param>
        /// <param name="notes">note entry data</param>
        public FPEInventorySaveData(FPEHeldObjectSaveData heldObjectData, FPEInventoryItemSaveData[] inventoryItems, FPEAudioDiaryEntrySaveData[] audioDiaries, FPENoteSaveData[] notes)
        {

            heldObjData = heldObjectData;
            invItems = inventoryItems;
            audioDiaryData = audioDiaries;
            noteData = notes;

        }

        public override string ToString()
        {

            string result = "FPEInventorySaveData:\n";
            result += heldObjData.ToString() + "\n";
            result += "InvItems data size '"+ invItems.Length + "'\n";
            result += "AudioDiaries data size '" + audioDiaryData.Length + "'\n";
            result += "Notes data size '" + noteData.Length + "'\n";

            return result;

        }

    }

    #endregion

    #region OPTIONS_DATA

    [Serializable]
    public class FPEGameOptionsSaveData
    {

        private float sensitivity;
        public float LookSensitivity { get { return sensitivity; } }
        private bool smoothing;
        public bool LookSmoothing { get { return smoothing; } }
        private bool gamepad;
        public bool UseGamepad { get { return gamepad; } }
        private bool flipYAxisMouseOnly;
        public bool FlipYAxisMouseOnly { get { return flipYAxisMouseOnly; } }
        private bool flipYAxisGamepadOnly;
        public bool FlipYAxisGamepadOnly { get { return flipYAxisGamepadOnly; } }


        public FPEGameOptionsSaveData(float lookSensitivity, bool lookSmoothing, bool useGamepad, bool flipMouseY, bool flipGamepadY)
        {

            sensitivity = lookSensitivity;
            smoothing = lookSmoothing;
            gamepad = useGamepad;
            flipYAxisMouseOnly = flipMouseY;
            flipYAxisGamepadOnly = flipGamepadY;

        }

        public override string ToString()
        {
            return "FPEGameOptionsSaveData: sens='" + sensitivity + "',smooth='" + smoothing + "',useGpad='" + gamepad + "',flipMY='" + flipYAxisMouseOnly + "',flipGY='" + flipYAxisGamepadOnly + "'";
        }

    }

    #endregion

    #region DOORS

    //
    // NOTE: This class changed as of version 2.2 to accomodate a broader set of door types. 
    //
    // Changes:
    //
    // v2.2: Added customDoorVector variable to account for some swinging door types
    //       Added isInternallyLocked variable to account for internal locking mechanisms such as deadbolts and door handles
    //       Added isExternallyLocked variable to account for external locking mechanisms such as security systems and magnetic locks
    // 
    [Serializable]
    public class FPEDoorSaveData
    {

        private string objName;
        public string ObjectName { get { return objName; } }
        private FPEDoor.eDoorState state;
        public FPEDoor.eDoorState DoorState { get { return state; } }
        private string doorHandleString;
        public string DoorHandleString { get { return doorHandleString; } }
        // For storing custom swing-based angles, positions, or other data for partial door operations
        private FPEVector3 customDoorVector;
        public Vector3 CustomDoorVector { get { return customDoorVector.getVector3(); } }
        private bool isInternallyLocked;
        public bool IsInternallyLocked { get { return isInternallyLocked; } }
        private bool isExternallyLocked;
        public bool IsExternallyLocked { get { return isExternallyLocked; } }


        public FPEDoorSaveData(string objectName, FPEDoor.eDoorState doorState, string handleString, Vector3 customVector, bool internallyLocked, bool externallyLocked)
        {

            objName = objectName;
            state = doorState;
            doorHandleString = handleString;
            customDoorVector = new FPEVector3(customVector);
            isInternallyLocked = internallyLocked;
            isExternallyLocked = externallyLocked;

        }

        public override string ToString()
        {
            return "FPEDoorSaveData: name='" + objName + "',state='" + state + "',handleString='" + doorHandleString + "',customDoorVector='" + customDoorVector.ToString() + "',isInternallyLocked='" + isInternallyLocked + "'" + "',isExternallyLocked='" + isExternallyLocked + "'";
        }

    }

    #endregion

    #region DRAWERS

    //
    // NOTE: This class was implemented as of version 2.2 to accomodate drawers
    //
    [Serializable]
    public class FPEDrawerSaveData
    {

        private string objName;
        public string ObjectName { get { return objName; } }
        private FPEDrawer.eDrawerState state;
        public FPEDrawer.eDrawerState DrawerState { get { return state; } }
        private string drawerPullString;
        public string DrawerPullString { get { return drawerPullString; } }
        // For storing custom positions, or other data for partial drawer operations
        private FPEVector3 customDrawerVector;
        public Vector3 CustomDrawerVector { get { return customDrawerVector.getVector3(); } }
        private bool isInternallyLocked;
        public bool IsInternallyLocked { get { return isInternallyLocked; } }
        private bool isExternallyLocked;
        public bool IsExternallyLocked { get { return isExternallyLocked; } }

        public FPEDrawerSaveData(string objectName, FPEDrawer.eDrawerState drawerState, string handleString, Vector3 customVector, bool internallyLocked, bool externallyLocked)
        {

            objName = objectName;
            state = drawerState;
            drawerPullString = handleString;
            customDrawerVector = new FPEVector3(customVector);
            isInternallyLocked = internallyLocked;
            isExternallyLocked = externallyLocked;

        }

        public override string ToString()
        {
            return "FPEDrawerSaveData: name='" + objName + "',state='" + state + "',drawerPullString='" + drawerPullString + "',customDrawerVector='" + customDrawerVector.ToString() + "',isInternallyLocked='" + isInternallyLocked + "'" + "',isExternallyLocked='" + isExternallyLocked + "'";
        }

    }

    #endregion

    //
    // This generic type contains 1 float, 1 int, and 1 bool, along with an object name
    // This can be used for saving generic object states, but should not be used for 
    // everything. This can also be used as a basis for creating your own custom type.
    //
    #region GENERIC_TYPE

    [Serializable]
    public class FPEGenericObjectSaveData
    {

        private string objName;
        public string SavedName { get { return objName; } }
        private int objInt;
        public int SavedInt { get { return objInt; } }
        private float objFloat;
        public float SavedFloat { get { return objFloat; } }
        private bool objBool;
        public bool SavedBool { get { return objBool; } }

        public FPEGenericObjectSaveData(string objectName, int objectInt, float objectFloat, bool objectBool)
        {

            objName = objectName;
            objInt = objectInt;
            objFloat = objectFloat;
            objBool = objectBool;

        }

        public override string ToString()
        {
            return "FPEGenericObjectSaveData: name='" + objName + "',int=" + objInt + ",float=" + objFloat + ",bool='" + objBool + "'";
        }

    }

    #endregion

    #region CUSTOM_TYPES

    // TODO: Your custom types here

    #endregion

}