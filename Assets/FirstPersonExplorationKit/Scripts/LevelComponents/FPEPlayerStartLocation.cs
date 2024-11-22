using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEPlayerStartLocation
    // This script simply acts as a findable component for Start Locations 
    // levels. It also draws a gizmo for easy visual identification.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEPlayerStartLocation : MonoBehaviour
    {

#if UNITY_EDITOR

        void OnDrawGizmos()
        {

            Color c = Color.green;
            c.a = 0.5f;
            Gizmos.color = c;

            Gizmos.DrawSphere(transform.position, 0.75f);
            Gizmos.DrawWireSphere(transform.position, 0.75f);
            Gizmos.DrawIcon(transform.position, "Whilefun/playerStart.png", false);

        }

#endif

    }

}