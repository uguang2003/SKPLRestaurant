using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBurger : MonoBehaviour
{

    public GameObject burgerPrefab;
    void Start()
    {
        
    }
    void Update()
    {
        
    }

    public void GenerateABurger()
    {
        GameObject burger = Instantiate(burgerPrefab, transform.position, Quaternion.identity);
    }
}
