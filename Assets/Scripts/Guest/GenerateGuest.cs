using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GenerateGuest : MonoBehaviour
{

    public GameObject guestPrefab;
    public GameObject guestStartLocation;
    //���ɵķ�����λ�õı�ţ������˾ͱ��������ţ��´����ɵ�ʱ���ж��Ƿ��Ѿ���������
    public List<int> seatList = new List<int>();
    public List<int> beiZuoList = new List<int>();

    float time = 0;

    void Start()
    {
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
        GameObject guest = Instantiate(guestPrefab, guestStartLocation.transform.position, Quaternion.identity);
        guest.transform.parent = guestStartLocation.transform;
        guest.GetComponent<GuestAI>().target = GameObject.Find("Chair 3 (" + num + ")").transform;
        guest.GetComponent<GuestAI>().num = num;
    }
}
