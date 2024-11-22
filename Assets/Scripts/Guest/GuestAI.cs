using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuestAI : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform target;

    private GameObject guestStartLocation;
    public bool isGoGuestStartLocation = false;
    public GameObject GuestManager;
    public int num;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        guestStartLocation = GameObject.Find("GuestPosition");
        GuestManager = GameObject.Find("GuestManager");
    }
    void Update()
    {
        agent.SetDestination(target.position);
        GoGuestStartLocation();
    }

    void GoGuestStartLocation()
    {
        if (isGoGuestStartLocation)
        {
            agent.SetDestination(guestStartLocation.transform.position);
            if (Vector3.Distance(transform.position, guestStartLocation.transform.position) < 0.5f)
            {
                //截取出来的代码，将座位编号添加到seatList里，从beiZuoList里移除
                GuestManager.GetComponent<GenerateGuest>().seatList.Add(num);
                GuestManager.GetComponent<GenerateGuest>().beiZuoList.Remove(num);
                Destroy(gameObject);
            }
        }
    }
}
