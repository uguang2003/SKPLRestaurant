using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEEventSystem
    // This script simply creates a referenceable singleton for the core event system for the asset.
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEEventSystem : MonoBehaviour
    {

        private static FPEEventSystem _instance;
        public static FPEEventSystem Instance {
            get { return _instance; }
        }

        void Awake()
        {

            if (_instance != null)
            {
                Debug.LogWarning("FPEEventSystem:: Duplicate instance of FPEEventSystem, deleting second one.");
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