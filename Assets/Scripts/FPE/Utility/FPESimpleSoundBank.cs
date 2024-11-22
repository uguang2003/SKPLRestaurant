using UnityEngine;
using Random = UnityEngine.Random;

namespace Whilefun.FPEKit
{

    // FPESimpleSoundBank
    // This class provides a simple means to play a set of sounds with some randomization.
    [CreateAssetMenu(menuName = "While Fun Games/Simple Sound Bank")]
    [System.Serializable]
    public class FPESimpleSoundBank : FPESoundBank
    {

        public AudioClip[] clips;

        [FPEMinMaxRange(0.0f, 1.0f)]
        public FPEMinMaxRange volume;

        [FPEMinMaxRange(0.1f, 2.0f)]
        public FPEMinMaxRange pitch;

        public override void Play(AudioSource source)
        {

            if (clips.Length > 0)
            {

                source.clip = clips[Random.Range(0, clips.Length)];
                source.volume = Random.Range(volume.minValue, volume.maxValue);
                source.pitch = Random.Range(pitch.minValue, pitch.maxValue);
                source.Play();

            }

        }

    }

}
