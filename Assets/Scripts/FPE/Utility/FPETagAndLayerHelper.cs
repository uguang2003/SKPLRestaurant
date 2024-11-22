using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    public class FPETagAndLayerHelper : MonoBehaviour
    {

#if UNITY_EDITOR

        public static readonly string LAYER_PUT_BACK = "FPEPutBackObjects";

        void Start()
        {

            CheckLayerValidity();

        }

        public static void CheckLayerValidity(bool printWhenPass = false)
        {

            int putBackLayer = LayerMask.NameToLayer("FPEPutBackObjects");
            int pickupLayer = LayerMask.NameToLayer("FPEPickupObjects");
            int playerLayer = LayerMask.NameToLayer("FPEPlayer");
            int objectExaminationLayer = LayerMask.NameToLayer("FPEObjectExamination");
            int ignoreLayer = LayerMask.NameToLayer("FPEIgnore");

            if ((putBackLayer == -1) || (pickupLayer == -1) || (playerLayer == -1) || (objectExaminationLayer == -1) || (ignoreLayer == -1))
            {
                Debug.LogError("<color=red>Mandatory Layers are missing. Check the list of required layers here:\n<b> http://whilefun.com/fpedocs/sections/tagslayersphysics.html </b></color>\n\n");
            }
            else
            {
                if (printWhenPass)
                {
                    Debug.Log("<color=green><b>PASS: Layers seem okay!</b></color>");
                }
            }

        }

#endif

    }

}