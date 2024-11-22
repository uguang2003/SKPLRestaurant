using UnityEngine;
using UnityEditor;
using Whilefun.FPEKit;
using System;
using System.IO;

//
// FPEDeleteLocalSaveData
// This script removes local save game and options data from the persistentDataPath for easier data management in the editor
//
// Copyright 2022 While Fun Games
// http://whilefun.com
//
public class FPEDeleteLocalSaveData
{

    [MenuItem("While Fun Games/Delete Local Saved Game Data", false, 301)]
    private static void NewMenuOption()
    {

        string errorString = "";

        Debug.Log("FPEDeleteLocalSaveData:: Looking for Save Game Data to Erase...");

        errorString = removeSavedGameData();

        if (errorString == "")
        {
            Debug.Log("FPEDeleteLocalSaveData:: <color=green><b>COMPLETE!</b></color>");
        }
        else
        {
            Debug.Log("FPEDeleteLocalSaveData:: <color=red><b>FAILED!</b></color> Errors:\n\n" + errorString + "\n\n");
        }

        Debug.Log("FPEDeleteLocalSaveData:: DONE.");

    }


    //
    // Note: The following two functions basically taken straight from FPESaveLoadManager
    //
    private static string removeSavedGameData()
    {

        string result = "";

        string autoSavePath = Application.persistentDataPath + "/" + FPESaveLoadManager.autoSaveDirName;
        string fullSavePath = Application.persistentDataPath + "/" + FPESaveLoadManager.fullSaveDirName;

        // Delete all level save files in auto directory
        if (deleteAllLevelDataFromPath(autoSavePath) == false)
        {
            result += "Failed to delete files from autoSavePath!\n";
        }

        // Delete all level save files in full directory
        if (deleteAllLevelDataFromPath(fullSavePath) == false)
        {
            result += "Failed to delete files from fullSavePath!\n";
        }

        if (deleteOptionsData() == false)
        {
            result += "Failed to delete options data file!\n";
        }

        return result;

    }

    private static bool deleteAllLevelDataFromPath(string path)
    {

        bool result = true;

        // Delete all level save data files in specified directory, of all ".dat" types
        DirectoryInfo dirFull = new DirectoryInfo(path);

        foreach (FileInfo f in dirFull.GetFiles("*" + FPESaveLoadManager.levelDataFilePostfix))
        {

            try
            {
                File.Delete(f.FullName);
            }
            catch (Exception e)
            {

                Debug.LogError("FPESaveLoadManager.deleteAllLevelDataFromPath():: Failed to delete save file '" + f.FullName + "'. Reason: " + e.Message);
                result = false;

            }
            finally
            {
            }

        }

        return result;

    }

    private static bool deleteOptionsData()
    {

        bool result = true;

        // Delete the options.dat file
        string optionsDataSaveFileFullPath = Application.persistentDataPath + "/" + FPESaveLoadManager.optionsDataSaveFile;

        try
        {
            File.Delete(optionsDataSaveFileFullPath);
        }
        catch (Exception e)
        {

            Debug.LogError("FPEDeleteLocalSaveData.deleteOptionsData():: Failed to delete options data file '" + optionsDataSaveFileFullPath + "'. Reason: " + e.Message);
            result = false;

        }
        finally
        {
        }


        return result;

    }

}
