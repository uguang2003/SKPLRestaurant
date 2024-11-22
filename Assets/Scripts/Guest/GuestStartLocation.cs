using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestStartLocation : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {

        Color c = Color.green;
        c.a = 0.5f;
        Gizmos.color = c;

        Gizmos.DrawSphere(transform.position, 0.75f);
        Gizmos.DrawWireSphere(transform.position, 0.75f);

    }

#endif
}
