using UnityEngine.Events;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPEGenericEvent
    // This generic event is used across many Interactable types for arbitrary addition script execution
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [Serializable]
    public class FPEGenericEvent : UnityEvent
    {

        public enum eEventFireType
        {
            ONCE = 0,
            EVERYTIME = 1,
            TOGGLE = 2
        }

        public enum eRepeatMode
        {
            ONETIME = 0,
            REPEAT = 1
        }

    }

}
