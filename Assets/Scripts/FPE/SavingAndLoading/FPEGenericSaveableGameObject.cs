using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEGenericSaveableGameObject
    // This class for extending MonoBehaviours that you want to be saveable automatically through 
    // FPESaveLoadManager. If your class extends this class, it must implement the save and load 
    // functions below, and will automatically get picked up in the save and load functions.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public abstract class FPEGenericSaveableGameObject : MonoBehaviour, FPEGenericSaveableInterface
    {

        public abstract FPEGenericObjectSaveData getSaveGameData();
        public abstract void restoreSaveGameData(FPEGenericObjectSaveData data);

    }

}