using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private List<Transform> HandTextList;
    public Transform HandText;
    Vector3 screenPos;

    public int eat;

    void Start()
    {
        eat = Random.Range(0, 2);
        //HandTextList为HandText下的所有子物体
        HandTextList = new List<Transform>();
        for (int i = 0; i < HandText.childCount; i++)
        {
            HandTextList.Add(HandText.GetChild(i));
        }
        HandTextList[eat].gameObject.SetActive(true);

        agent = GetComponent<NavMeshAgent>();
        guestStartLocation = GameObject.Find("GuestPosition");
        GuestManager = GameObject.Find("GuestManager(Clone)");
    }
    void Update()
    {
        agent.SetDestination(target.position);
        if (Vector3.Distance(transform.position, target.transform.position) < 0.7f)
        {
            transform.localScale = new Vector3(0.35f, 0.5f, 0.35f);
        }
        else
        { 
            transform.localScale = new Vector3(0.35f, 1f, 0.35f);
        }
        GoGuestStartLocation();

        if (HandText)
        {
            HandText.LookAt(GetSymmetryPoint(Camera.main.transform));
        }
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
            if (HandText != null)
            {
                Destroy(HandText.gameObject);
            }
        }
    }


    Vector3 GetSymmetryPoint(Transform LookTran)
    {
        return new Vector3(
            transform.position.x * 2 - LookTran.position.x,
            transform.position.y * 2 - LookTran.position.y + 1f,
            transform.position.z * 2 - LookTran.position.z);
    }

}
