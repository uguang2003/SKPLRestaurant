using UnityEngine;
using System.Collections;

// A debug/input script for the demo. You can delete this.
public class DemoDebugHelperScript : MonoBehaviour {

	void Update(){
	
		if(Input.GetKeyDown(KeyCode.Escape)){
			Application.Quit();
		}

	}

}
