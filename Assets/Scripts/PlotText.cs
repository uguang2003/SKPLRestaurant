using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class PlotText : MonoBehaviour
{
    public float speedTime = 0.1f;//打字间隔时间

    float timer;//计时器时间
    Text TextCompnt;//Text文字组件
    int wordNumber;//文字数量
    int nNumber;//下一行开始的位置
    bool isStart;//是否开始打字

    string wordContent;//----文字内容

    public GameObject plotPanel;

    void Awake()
    {
        TextCompnt = this.GetComponent<Text>();//从当前物体获取到Text组件
        isStart = true;//bool值的默认值是false  所以这里要重置为true
        wordContent = "你是一个没人要的孩子，直到有一天，\n你吃到了一个汉堡，从此展开了你的故事...";
    }

    void FixedUpdate()
    {
        StartTyping();
    }
    void StartTyping()
    {
        if (isStart)
        {
            timer += Time.deltaTime;//简单的计时器
            if (timer >= speedTime)//如果计时器时间>打字间隔时间
            {
                //设置文字显示
                TextCompnt.CrossFadeAlpha(1, 0, false);

                timer = 0;//重置
                wordNumber++;//文字数量+1

                //Substring() 官方文档解释：从此实例检索子字符串。 子字符串从指定的字符位置开始且具有指定的长度。
                TextCompnt.text = wordContent.Substring((nNumber), wordNumber - nNumber);//数字数量的起始字符位置（从零开始）
                if (wordNumber >= wordContent.Length)//数字数量=文字的长度
                {
                    isStart = false;//停止打字
                    //设置文字渐隐
                    TextCompnt.CrossFadeAlpha(0, 1.5f, false);
                    timer -= 1.5f;
                    //延迟1.5秒关闭plotPanel
                    Invoke("ClosePlot", 1.5f);
                }
                else if (wordContent.Substring((wordNumber), 1) == "\n")
                {
                    nNumber = wordNumber + 1;
                    Thread.Sleep(1000);
                    //设置文字渐隐
                    //TextCompnt.CrossFadeAlpha(0, 1.5f, false);
                    //timer -= 1.5f;
                }
            }
        }
    }

    public void ClosePlot()
    {
        plotPanel.SetActive(false);
    }
}
