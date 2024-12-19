using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionUI : MonoBehaviour
{
    private GameObject selectionPanel;
    string[] ss = { "1.汉堡第一步放什么?#A.下层面包|B.芝士|C.番茄|D.肉饼#A",
        "2.汉堡第二步放什么?#A.下层面包|B.芝士|C.番茄|D.肉饼#B", 
        "3.汉堡第三步放什么?#A.下层面包|B.芝士|C.番茄|D.肉饼#C", 
        "4.汉堡第四步放什么?#A.下层面包|B.芝士|C.番茄|D.肉饼#D",
        "5.汉堡第五步放什么?#A.下层面包|B.芝士|C.番茄|D.生菜#D" };
    string[] 题目 = new string[5];
    string[] 选项 = new string[5];
    string[] 答案 = new string[5];
    int indexQuestion = 0;
    Toggle[] toggles = new Toggle[4];
    Dictionary<int, string> dic = new Dictionary<int, string>();//存单个Toggle的选择答案
    List<string> answers = new List<string>();//存所有的选择过的答案
    string answer = "";
    int score = 0;

    public GameObject nextQuestion;
    public GameObject endQuestion;

    void Start()
    {
        Button btn = nextQuestion.GetComponent<Button>();
        btn.onClick.AddListener(() => { this.NextQuestion(); });

        endQuestion.GetComponent<Button>().onClick.AddListener(() => {             
            //答题结束，关闭界面
            ShowManager.instance.QuesionToGame();
        });

        dic.Add(0,"A");
        dic.Add(1,"B");
        dic.Add(2,"C");
        dic.Add(3,"D");
        selectionPanel = transform.Find("ShowSelection").gameObject;
        for (int i = 0; i < ss.Length; i++)
        {
            string[] sss = ss[i].Split('#'); //sss就拿到了每一道题的 题目 选项 答案
            题目[i] = sss[0];
            选项[i] = sss[1];
            答案[i] = sss[2];
        }
        //设置第一题
        selectionPanel.transform.Find("题目").GetComponent<Text>().text = 题目[indexQuestion];
        for (int i = 0; i < 4; i++)
        {
            selectionPanel.transform.GetChild(2).GetChild(i).GetChild(1).GetComponent<Text>().text = 选项[i].Split('|')[i];
        }
        for (int i = 0; i < 4; i++)
        {
            toggles[i] = selectionPanel.transform.GetChild(2).GetChild(i).GetComponent<Toggle>();
        }

        this.transform.parent.gameObject.SetActive(false);
    }
  
    //获取答案---先选哪个选项，之后根据哪个数值传递到字典的K值，通过字典的K值找到存放的ABCD。
    public void Getanswere() {
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
            {
                answer += dic[i];
            }

        }
       
        //点击下一题就清空上一题的选项
        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].isOn = false;
            //selectionPanel.transform.GetChild(3).GetComponent<Button>().interactable = false;
        }
       
    }
    //下一题
    public void NextQuestion() {
        Getanswere();
       
        //没有选择就不允许进行下一次点击
        if (string.IsNullOrEmpty(answer))
        {
            return;
        }
        //先将答案存起来
        answers.Add(answer);
        //再清空，才能保证下一次没有选择选项就不能点击下一题
        answer = "";
        indexQuestion++;
        if (indexQuestion==ss.Length)
        {
            //统计得分
            for (int i = 0; i < 5; i++)
            {
                if (answers[i] == 答案[i])
                {
                    score += 20;
                }
            }

            selectionPanel.transform.GetChild(0).gameObject.SetActive(false);
            selectionPanel.transform.GetChild(1).gameObject.SetActive(false);
            selectionPanel.transform.GetChild(2).gameObject.SetActive(false);
            selectionPanel.transform.GetChild(3).gameObject.SetActive(false);
            selectionPanel.transform.GetChild(4).gameObject.SetActive(true);
            selectionPanel.transform.GetChild(5).gameObject.SetActive(true);
            selectionPanel.transform.GetChild(4).GetComponent<Text>().text += score;
            return;
        }
        else if (indexQuestion==ss.Length-1)
        {
            selectionPanel.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = "答题完毕";
        }
        selectionPanel.transform.Find("题目").GetComponent<Text>().text = 题目[indexQuestion];
        for (int i = 0; i < 4; i++)
        {
            selectionPanel.transform.GetChild(2).GetChild(i).GetChild(1).GetComponent<Text>().text = 选项[i].Split('|')[i];
        }
    }
    public void ShowSelectPanel(bool IsActive) {
        selectionPanel.transform.GetChild(4).GetComponent<Text>().text = "你的得分是：";
        indexQuestion = 0;
        score = 0;
        answer = "";
        answers.Clear();
        selectionPanel.transform.GetChild(0).gameObject.SetActive(true);
        selectionPanel.transform.GetChild(1).gameObject.SetActive(true);
        selectionPanel.transform.GetChild(2).gameObject.SetActive(true);
        selectionPanel.transform.GetChild(3).gameObject.SetActive(true);
        selectionPanel.transform.GetChild(4).gameObject.SetActive(false);
        selectionPanel.transform.GetChild(5).gameObject.SetActive(false);
        selectionPanel.transform.Find("题目").GetComponent<Text>().text = 题目[indexQuestion];
        for (int i = 0; i < 4; i++)
        {
            selectionPanel.transform.GetChild(2).GetChild(i).GetChild(1).GetComponent<Text>().text = 选项[i].Split('|')[i];
        }
        selectionPanel.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = "下一题";
        selectionPanel.SetActive(IsActive); 
    }

    void Update()
    {

    }
}
