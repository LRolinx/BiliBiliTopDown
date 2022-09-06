
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
            print("已连接上转发服务器");
            reconnectionsnumber = 0;
            socketisclose = false;
            //发送文件储存目录
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
            //Socket没关闭才可以接收
            Root data = JsonConvert.DeserializeObject<Root>(e.Data);
            if (data.cmd == "DANMU_MSG")
            {
                //弹幕信息
                Loom.QueueOnMainThread((param) =>
                {
                    transform.GetComponent<GameManager>().CreateAIPlayer(data);
                }, null);

            }
            else if (data.cmd == "SEND_GIFT")
            {
                //送礼
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
                print($"已断开转发服务器,尝试重连第{reconnectionsnumber}次...");
                //}
                //else
                //{
                //    print($"重连已超最大次数，取消重连");
                //}
            }
            else
            {
                print("Socket已关闭");
            }

        };
        ws.OnError += (sender, e) =>
        {
            print("Socket出现致命错误" + e.Exception.Message.ToString() + "/" + sender.ToString());
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
    /// 标识
    /// </summary>
    public string cmd { get; set; }
    /// <summary>
    /// 文本
    /// </summary>
    public string text { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public int uid { get; set; }

    /// <summary>
    /// 用户名
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
    /// 礼物名称
    /// </summary>
    public string giftName { get; set; }
    /// <summary>
    /// 礼物ID
    /// </summary>
    public int giftId { get; set; }
    /// <summary>
    /// 礼物数量
    /// </summary>
    public int num { get; set; }
    /// <summary>
    /// 礼物类型
    /// </summary>
    public int giftType { get; set; }
    /// <summary>
    /// 礼物价格
    /// </summary>
    public int price { get; set; }
}
