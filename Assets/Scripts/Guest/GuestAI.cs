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

    public Transform HandText;//���ֶ�Ӧ3D������
    Vector3 screenPos;

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
                //��ȡ�����Ĵ��룬����λ�����ӵ�seatList���beiZuoList���Ƴ�
                GuestManager.GetComponent<GenerateGuest>().seatList.Add(num);
                GuestManager.GetComponent<GenerateGuest>().beiZuoList.Remove(num);
                Destroy(gameObject);
            }
            if (HandText)
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
