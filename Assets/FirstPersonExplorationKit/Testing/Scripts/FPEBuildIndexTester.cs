using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whilefun.FPEKit
{

    //
    // A very simple script to softly enforce build indexes if they are required. For example, if demoMainMenu 
    // needs to be at build index 0, this script can be placed in that scene to detect it. This is useful for
    // detecting issues in project upgrades, and stopping the editor before things break (with a nice friendly 
    // message)
    //
    public class FPEBuildIndexTester : MonoBehaviour
    {

        [SerializeField]
        private int buildIndexToCheck = -1;

        void Start()
        {

            if (SceneManager.GetActiveScene().buildIndex != buildIndexToCheck)
            {
                Debug.LogError("FPEBuildIndexTester:: This scene must be in Build Settings scene list at index '"+ buildIndexToCheck + "' in order to function correctly. But, it is not (buildIndex is '" + SceneManager.GetActiveScene().buildIndex + "', not the required index of '" + buildIndexToCheck + "')");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }

        }

    }

}