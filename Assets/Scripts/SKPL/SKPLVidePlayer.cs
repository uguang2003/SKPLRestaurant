using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class SKPLVidePlayer : MonoBehaviour
{
    private VideoPlayer Vp;
    void Awake()
    {
        Vp = GetComponent<VideoPlayer>();
    }
    void Start()
    {
        //Vp.loopPointReached += VideoEnd;

        //Vp.Play();//������Ƶ
        //Vp.Pause();//��ͣ��Ƶ
        //Vp.Stop();//ֹͣ��Ƶ
        //Vp.playbackSpeed = 1;//�����ٶ�
    }
    /// <summary>
    /// ������Ƶ�Ƿ񲥷Ž���������ʱ����
    /// </summary>
    /// <param name="vp"></param>
    void VideoEnd(VideoPlayer vp)
    {
        Debug.Log("��Ƶ���Ž���");
        Vp.Play();//���²�����Ƶ
    }

}
