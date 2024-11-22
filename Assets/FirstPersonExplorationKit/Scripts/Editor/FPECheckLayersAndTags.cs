using UnityEngine;
using UnityEditor;
using Whilefun.FPEKit;

//
// FPECheckLayersAndTags
// This script contains logic to valid project layers
//
// Copyright 2019 While Fun Games
// http://whilefun.com
//
public class FPECheckLayersAndTags {


    [MenuItem("While Fun Games/Check Tags and Layers", false, 203)]
    private static void NewMenuOption()
    {

        Debug.Log("FPECheckLayersAndTags:: Checking layer validity...");
        FPETagAndLayerHelper.CheckLayerValidity(true);

    }

}
