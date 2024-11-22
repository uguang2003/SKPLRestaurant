using UnityEngine;
using UnityEditor;
using Whilefun.FPEKit;

//
// FPESaveGameErrorChecker
// This script combines outputs from other editor scripts to output a single report on potential save game errors in the given scene.
//
// Copyright 2018 While Fun Games
// http://whilefun.com
//
public class FPESaveGameErrorChecker
{

    [MenuItem("While Fun Games/Check for Save Game Errors", false, 100)]
    private static void NewMenuOption()
    {

        string errorString = "";

        Debug.Log("FPESaveGameErrorChecker:: Looking for Save Game Errors...");

        errorString = FPEFindBrokenInteractions.FindInteractionIssues();

        if (errorString == "")
        {
            Debug.Log("FPEFindBrokenInteractions:: <color=green><b>PASS!</b></color>");
        }
        else
        {
            Debug.Log("FPEFindBrokenInteractions:: <color=red><b>FAIL!</b></color> Interaction Issues:\n\n" + errorString + "\n\n");
        }

        errorString = FPEUniqueNameValidator.ValidateSceneObjects();

        if (errorString == "")
        {
            Debug.Log("FPEUniqueNameValidator:: <color=green><b>PASS!</b></color>");
        }
        else
        {
            Debug.Log("FPEUniqueNameValidator:: <color=red><b>FAIL!</b></color> List of items with non-unique names:\n\n" + errorString + "\n\n");
        }

        Debug.Log("FPESaveGameErrorChecker:: DONE.");


    }

}
