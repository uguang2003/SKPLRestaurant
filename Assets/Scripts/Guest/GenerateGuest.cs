using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GenerateGuest : MonoBehaviour
{

    public GameObject guestPrefab;
    public GameObject guestPrefab2;
    public GameObject guestPrefab3;
    private GameObject guestStartLocation;
    //生成的方法传位置的编号，生成了就保存这个编号，下次生成的时候判断是否已经有人坐了
    public List<int> seatList = new List<int>();
    public List<int> beiZuoList = new List<int>();


    float time = 0;

    void Start()
    {
        guestStartLocation = GameObject.Find("GuestPosition");

        //设置座位的编号为{ 4, 5, 6, 7 }
        seatList.Add(4);
        seatList.Add(5);
        seatList.Add(6);
        seatList.Add(7);
    }

    void Update()
    {
        //每10秒生成一个客人，有4个座位，坐满了就不生成了       
        time += Time.deltaTime;
        if (time >= Random.Range(8f, 16f))
        {
            //随机将beiZuoList里没有的数据，从seatList里的一个位置放到beiZuoList里
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
