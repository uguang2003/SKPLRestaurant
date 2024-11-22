using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEPlayer
    // A simple class for keeping a referenceable singleton of the player controller, independent 
    // of the actual player controller script. For example, if using another player controller 
    // asset, you can still apply this script to that asset's player controller prefab, and reference 
    // FPEPlayer.Instance generically.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEPlayer : MonoBehaviour
    {

        private static FPEPlayer _instance;
        public static FPEPlayer Instance {
            get { return _instance; }
        }

        void Awake()
        {

            if (_instance != null)
            {
                Debug.LogWarning("FPEPlayer:: Duplicate instance of FPEPlayer, deleting second one.");
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }

        }

    }

}