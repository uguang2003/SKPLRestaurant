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

        //Vp.Play();//播放视频
        //Vp.Pause();//暂停视频
        //Vp.Stop();//停止视频
        //Vp.playbackSpeed = 1;//播放速度
    }
    /// <summary>
    /// 监听视频是否播放结束，结束时调用
    /// </summary>
    /// <param name="vp"></param>
    void VideoEnd(VideoPlayer vp)
    {
        Debug.Log("视频播放结束");
        Vp.Play();//重新播放视频
    }

}
