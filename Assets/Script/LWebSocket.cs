
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;

public class LWebSocket : MonoBehaviour
{
    public static string SocketUrl = "ws://127.0.0.1:888";
    private WebSocket ws = new WebSocket(SocketUrl);
    private int reconnectionsnumber = 0;
    private bool socketisclose = false;
    // Start is called before the first frame update
    void Start()
    {
        Loom.Initialize();

        ws.OnOpen += (sender, e) =>
        {
            print("��������ת��������");
            reconnectionsnumber = 0;
            socketisclose = false;
            //�����ļ�����Ŀ¼
            Root root = new Root();
            Loom.QueueOnMainThread((param) =>
            {

                root.cmd = "LWS_SaveDataPath";
                root.text = Application.persistentDataPath;

                ws.Send(JsonConvert.SerializeObject(root));
            }, null);

        };
        ws.OnMessage += (sender, e) =>
        {
            //Socketû�رղſ��Խ���
            Root data = JsonConvert.DeserializeObject<Root>(e.Data);
            if (data.cmd == "DANMU_MSG")
            {
                //��Ļ��Ϣ
                Loom.QueueOnMainThread((param) =>
                {
                    transform.GetComponent<GameManager>().CreateAIPlayer(data);
                }, null);

            }
            else if (data.cmd == "SEND_GIFT")
            {
                //����
                Loom.QueueOnMainThread((param) =>
                {
                    transform.GetComponent<GameManager>().SetPlayerBuff(data);
                }, null);

            }
        };
        ws.OnClose += (sender, e) =>
        {
            if (!socketisclose)
            {
                //if (reconnectionsnumber < 10)
                //{
                reconnectionsnumber += 1;
                ws.ConnectAsync();
                print($"�ѶϿ�ת��������,����������{reconnectionsnumber}��...");
                //}
                //else
                //{
                //    print($"�����ѳ���������ȡ������");
                //}
            }
            else
            {
                print("Socket�ѹر�");
            }

        };
        ws.OnError += (sender, e) =>
        {
            print("Socket������������" + e.Exception.Message.ToString() + "/" + sender.ToString());
            ws.ConnectAsync();
        };

        ws.ConnectAsync();
    }


    private void OnDestroy()
    {
        socketisclose = true;
        ws.CloseAsync();
    }
}




public class bilibiliImgRes
{
    public int code { get; set; }
    public bilibiliData data { get; set; }
}

public class bilibiliData
{
    public string uid { get; set; }

    public string name { get; set; }

    public int level { get; set; }
    public string sex { get; set; }
    public string description { get; set; }
    public string avatar { get; set; }

}
public class Root
{
    /// <summary>
    /// ��ʶ
    /// </summary>
    public string cmd { get; set; }
    /// <summary>
    /// �ı�
    /// </summary>
    public string text { get; set; }
    /// <summary>
    /// �û�ID
    /// </summary>
    public int uid { get; set; }

    /// <summary>
    /// �û���
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string avatar { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool isavatar { get; set; }
    /// <summary>
    /// ��������
    /// </summary>
    public string giftName { get; set; }
    /// <summary>
    /// ����ID
    /// </summary>
    public int giftId { get; set; }
    /// <summary>
    /// ��������
    /// </summary>
    public int num { get; set; }
    /// <summary>
    /// ��������
    /// </summary>
    public int giftType { get; set; }
    /// <summary>
    /// ����۸�
    /// </summary>
    public int price { get; set; }
}
