using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateApple : MonoBehaviour
{

    public GameObject applePrefab;
    void Start()
    {
        
    }
    void Update()
    {
        
    }

    public void GenerateAnApple()
    {
        GameObject apple = Instantiate(applePrefab, transform.position, Quaternion.identity);
    }
}
