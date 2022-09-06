using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillList : MonoBehaviour
{
    public float msgShowTime = 3f;
    private float showtime = 0f;
    public GameObject[] killMsg;
    public List<string> PlayerNameList;
    // Start is called before the first frame update
    void Start()
    {
        PlayerNameList = new List<string>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int num = 0;
        for (int i = 0; i < killMsg.Length; i++)
        {
            int cout = PlayerNameList.Count - 1;

            if (num == cout)
            {
                killMsg[i].SetActive(true);
                killMsg[i].transform.Find("PlayerName").GetComponent<TMP_Text>().text = PlayerNameList[i];
            }
            else
            {
                killMsg[i].SetActive(false);
            }
            num++;
        }

        showtime -= Time.deltaTime;
        if (showtime <= 0) {
            ClearKillMsg();
        }
    }

    public void SendKillMsg(string PlayerName)
    {
        //启动定时器
        showtime = msgShowTime;

        //发送死亡信息到列表里面
        if (PlayerNameList.Count < killMsg.Length)
        {
            //列表没满直接添加
            PlayerNameList.Add(PlayerName);
        }
        else
        {
            //删除第一条
            PlayerNameList.RemoveAt(0);
            //再次添加新的
            PlayerNameList.Add(PlayerName);
        }
    }

    void ClearKillMsg()
    {
        //定期清除
        if(PlayerNameList.Count > 0)
        {
            PlayerNameList.RemoveAt(0);
        }
    }
}
