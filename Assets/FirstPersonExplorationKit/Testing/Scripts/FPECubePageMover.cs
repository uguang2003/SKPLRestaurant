using UnityEngine;
using Whilefun.FPEKit;

public class FPECubePageMover : MonoBehaviour {
	
	void Update () {


        if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_MENU_NEXT_PAGE)){
            transform.Translate(new Vector3(0.5f, 0f, 0f));
        }

        if (FPEInputManager.Instance.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_MENU_PREVIOUS_PAGE)){
            transform.Translate(new Vector3(-0.5f, 0f, 0f));
        }


    }

}
