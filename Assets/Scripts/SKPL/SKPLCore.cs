using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SKPLCore : MonoBehaviour
{
    public GameObject showManager;
    public GameObject guestManager;

    public GameObject showGroupUI;
    public GameObject questionGroupUI;

    void Start()
    {
        initialize();
    }

    void Update()
    {

    }

    private void initialize()
    {
        Instantiate(showManager, null);
        Instantiate(guestManager, null);
        Instantiate(showGroupUI, null);
        Instantiate(questionGroupUI, null);
    }

}
