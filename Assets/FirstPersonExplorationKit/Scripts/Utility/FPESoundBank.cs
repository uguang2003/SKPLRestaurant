using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPESoundBank
    // This simple class allows for you to create a sound bank
    [System.Serializable]
    public abstract class FPESoundBank : ScriptableObject
    {
        public abstract void Play(AudioSource source);
    }

}
