using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Whilefun.FPEKit;

//
// FPEUniqueNameValidator
// This script contains logic to valid your scene for save/load name uniqueness for certain object types.
//
// Copyright 2017 While Fun Games
// http://whilefun.com
//
public class FPEUniqueNameValidator {

    [MenuItem("While Fun Games/Validate Scene for Saved Games", false, 202)]
    private static void NewMenuOption()
    {

        Debug.Log("FPEUniqueNameValidator:: Validating Names...");
        string errorString = ValidateSceneObjects();

        if (errorString == "")
        {
            Debug.Log("FPEUniqueNameValidator:: <color=green><b>PASS!</b></color>");
        }
        else
        {
            Debug.Log("FPEUniqueNameValidator:: <color=red><b>FAIL!</b></color> List of items with non-unique names:\n\n" + errorString + "\n\n");
        }

    }

    public static string ValidateSceneObjects()
    {

        string errorList = "";

        // Must validate name uniqueness of objects of types:
        // Event Triggers (FPEEventTrigger)
        // Activate Types (FPEInteractableActivateScript)
        // Attached Notes(FPEAttachedNote)
        // Audio Diaries(FPEInteractableAudioDiaryScript and FPEPassiveAudioDiary)
        // Doors(FPESimpleDoor)
        // Generic Saved Object(FPEGenericObjectSaveData)

        // Triggers //
        FPEEventTrigger[] allTriggers = GameObject.FindObjectsOfType<FPEEventTrigger>();
        for (int i = 0; i < allTriggers.Length; i++)
        {
            for (int j = i + 1; j < allTriggers.Length; j++)
            {
                if (allTriggers[i].gameObject.name == allTriggers[j].gameObject.name)
                {
                    Debug.Log("<b>Duplicate Found:</b> Type FPEEventTrigger named '" + allTriggers[i].gameObject.name + "'", allTriggers[j].gameObject);
                    errorList += "FPEEventTriggers with name '" + allTriggers[i].gameObject.name + "'\n";
                }
            }
        }

        // Activate //
        FPEInteractableActivateScript[] allActivates = GameObject.FindObjectsOfType<FPEInteractableActivateScript>();
        for (int i = 0; i < allActivates.Length; i++)
        {
            for (int j = i + 1; j < allActivates.Length; j++)
            {
                // Note for v2.2: If the Activate is a child of the SwingingPart of SlidingPart of a door or drawer, the dupe can be ignored.
                if (allActivates[i].gameObject.name == allActivates[j].gameObject.name)
                {
                    Debug.Log("<b>Duplicate Found:</b> Type FPEInteractableActivateScripts named '" + allActivates[i].gameObject.name + "'", allActivates[j].gameObject);
                    errorList += "FPEInteractableActivateScripts with name '" + allActivates[i].gameObject.name + "'. <b>NOTE: If these objects are door handles or drawer pulls, you can leave the names the same.</b>\n\n";
                }
            }
        }

        // Attached Notes //
        FPEAttachedNote[] allNotes = GameObject.FindObjectsOfType<FPEAttachedNote>();
        for (int i = 0; i < allNotes.Length; i++)
        {
            for (int j = i + 1; j < allNotes.Length; j++)
            {
                if (allNotes[i].gameObject.name == allNotes[j].gameObject.name)
                {
                    Debug.Log("<b>Duplicate Found:</b> Type FPEAttachedNote named '" + allNotes[i].gameObject.name + "'", allNotes[j].gameObject);
                    errorList += "FPEAttachedNote with name '" + allNotes[i].gameObject.name + "'\n";
                }
            }
        }

        // Audio Diaries: This is a unique case because names must be common for BOTH passive and active diaries//
        FPEPassiveAudioDiary[] allPassiveDiaries = GameObject.FindObjectsOfType<FPEPassiveAudioDiary>();
        FPEInteractableAudioDiaryScript[] allActiveDiaries = GameObject.FindObjectsOfType<FPEInteractableAudioDiaryScript>();

        // Combine the arrays
        List<GameObject> allCombinedDiaries = new List<GameObject>();
        for (int p = 0; p < allPassiveDiaries.Length; p++)
        {
            allCombinedDiaries.Add(allPassiveDiaries[p].gameObject);
        }
        for (int a = 0; a < allActiveDiaries.Length; a++)
        {
            allCombinedDiaries.Add(allActiveDiaries[a].gameObject);
        }

        GameObject[] allDiaries = allCombinedDiaries.ToArray();

        // Compare the combined list
        for (int i = 0; i < allDiaries.Length; i++)
        {
            for (int j = i + 1; j < allDiaries.Length; j++)
            {
                if (allDiaries[i].gameObject.name == allDiaries[j].gameObject.name)
                {
                    Debug.Log("<b>Duplicate Found:</b> Type FPEPassiveAudioDiary or FPEInteractableAudioDiaryScript named '" + allDiaries[i].gameObject.name + "'", allDiaries[j].gameObject);
                    errorList += "FPEPassiveAudioDiary or  FPEInteractableAudioDiaryScript with name '" + allDiaries[i].gameObject.name + "'\n";
                }
            }
        }

        // Journals //
        FPEInteractableJournalScript[] allJournals = GameObject.FindObjectsOfType<FPEInteractableJournalScript>();

        // Compare the combined list
        for (int i = 0; i < allJournals.Length; i++)
        {
            for (int j = i + 1; j < allJournals.Length; j++)
            {
                if (allJournals[i].gameObject.name == allJournals[j].gameObject.name)
                {
                    Debug.Log("<b>Duplicate Found:</b> Type FPEInteractableJournalScript named '" + allJournals[i].gameObject.name + "'", allJournals[j].gameObject);
                    errorList += "FPEInteractableJournalScript with name '" + allJournals[i].gameObject.name + "'\n";
                }
            }
        }

        // Doors //
        FPEDoor[] allDoors = GameObject.FindObjectsOfType<FPEDoor>();
        for (int i = 0; i < allDoors.Length; i++)
        {
            for (int j = i + 1; j < allDoors.Length; j++)
            {
                if (allDoors[i].gameObject.name == allDoors[j].gameObject.name)
                {
                    Debug.Log("<b>Duplicate Found:</b> Type FPEDoor named '" + allDoors[i].gameObject.name + "'", allDoors[j].gameObject);
                    errorList += "FPEDoor with name '" + allDoors[i].gameObject.name + "'\n";
                }
            }
        }

        // Drawers //
        FPEDrawer[] allDrawers = GameObject.FindObjectsOfType<FPEDrawer>();
        for (int i = 0; i < allDrawers.Length; i++)
        {
            for (int j = i + 1; j < allDrawers.Length; j++)
            {
                if (allDrawers[i].gameObject.name == allDrawers[j].gameObject.name)
                {
                    Debug.Log("<b>Duplicate Found:</b> Type FPEDrawer named '" + allDrawers[i].gameObject.name + "'", allDrawers[j].gameObject);
                    errorList += "FPEDrawer with name '" + allDrawers[i].gameObject.name + "'\n";
                }
            }
        }

        // Generic Saveable //
        FPEGenericSaveableGameObject[] allGenericSaveables = GameObject.FindObjectsOfType<FPEGenericSaveableGameObject>();
        for (int i = 0; i < allGenericSaveables.Length; i++)
        {
            for (int j = i + 1; j < allGenericSaveables.Length; j++)
            {
                if (allGenericSaveables[i].gameObject.name == allGenericSaveables[j].gameObject.name)
                {
                    Debug.Log("<b>Duplicate Found:</b> Type FPEGenericSaveableGameObject named '" + allGenericSaveables[i].gameObject.name + "'", allGenericSaveables[j].gameObject);
                    errorList += "FPEGenericSaveableGameObject with name '" + allGenericSaveables[i].gameObject.name + "'\n";
                }
            }
        }
        
        // YOUR CUSTOM TYPES VALIDATION HERE //

        return errorList;

    }

}
