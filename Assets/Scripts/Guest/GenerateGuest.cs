using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GenerateGuest : MonoBehaviour
{

    public GameObject guestPrefab;
    public GameObject guestPrefab2;
    public GameObject guestPrefab3;
    private GameObject guestStartLocation;
    //���ɵķ�����λ�õı�ţ������˾ͱ��������ţ��´����ɵ�ʱ���ж��Ƿ��Ѿ���������
    public List<int> seatList = new List<int>();
    public List<int> beiZuoList = new List<int>();


    float time = 0;

    void Start()
    {
        guestStartLocation = GameObject.Find("GuestPosition");

        //������λ�ı��Ϊ{ 4, 5, 6, 7 }
        seatList.Add(4);
        seatList.Add(5);
        seatList.Add(6);
        seatList.Add(7);
    }

    void Update()
    {
        //ÿ10������һ�����ˣ���4����λ�������˾Ͳ�������       
        time += Time.deltaTime;
        if (time >= Random.Range(8f, 16f))
        {
            //�����beiZuoList��û�е����ݣ���seatList���һ��λ�÷ŵ�beiZuoList��
            if (beiZuoList.Count < 4 && seatList.Count >= 0)
            {
                {
                    int random = Random.Range(0, seatList.Count);
                    beiZuoList.Add(seatList[random]);
                    GenerateGuests(seatList[random]);
                    seatList.RemoveAt(random);
                }
            }
            time = 0;
        }
    }
    public void GenerateGuests(int num)
    {
        GameObject guest = Instantiate(Random.Range(1, 3) == 1 ? guestPrefab2 : guestPrefab3, guestStartLocation.transform.position, Quaternion.identity);
        guest.transform.parent = guestStartLocation.transform;
        guest.GetComponent<GuestAI>().target = GameObject.Find("Chair 3 (" + num + ")").transform;
        guest.GetComponent<GuestAI>().num = num;
    }
}
