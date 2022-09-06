using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;

public enum WeaponType
{
    /// <summary>
    /// 无
    /// </summary>
    NULL,
    /// <summary>
    /// 刀
    /// </summary>
    KNIFE,
    /// <summary>
    /// TNT炸药
    /// </summary>
    TNT,
    /// <summary>
    /// 步枪
    /// </summary>
    RIFLE,
    SHOTGUN,
    /// <summary>
    /// 手枪
    /// </summary>
    PISTOL,
}
public class GameManager : MonoBehaviour
{
    public TMP_Text Text;
    public GameCamera gameCamera;
    //public Text scoreText, scoreTextBG;
    //public GameObject restartMessage, knifeSelector, gunSelector, endSection;
    public Transform node1;
    public Transform node2;
    public Transform node3;
    public Transform node4;
    int currentScore = 0;
    static GameManager myslf;
    public bool gameOver = false;
    public List<GameObject> monsterSpawn;

    ObjectPooler objectPooler;
    //已上场AIPlayer玩家
    private List<GameObject> surviveAIPlayer = new List<GameObject>();
    //预备AIPlayer玩家
    private List<Root> preparationAIPlayer = new List<Root>();
    //最大上场玩家 100
    private int maxPlayer = 100;
    //最大上场敌人
    private int maxEnemy = 50;
    //当前上场玩家
    public int currentPlayerNum = 0;
    //当前上场敌人
    public int currentEnemyNum = 0;

    void Awake()
    {
        myslf = this;
    }
    // Use this for initialization
    void Start()
    {
        objectPooler = ObjectPooler.Instance;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currentEnemyNum < maxEnemy && currentEnemyNum < currentPlayerNum)
        {
            //根据玩家情况生成敌人
            int index = new System.Random().Next(0, monsterSpawn.Count);
            GameObject ms = monsterSpawn[index];
            GameObject enemy = objectPooler.SpawnFromPool("Enemy", ms.transform.position, Quaternion.identity);
            //决定生成什么样的敌人
            //enemy.GetComponent<AI_Enemy>().
            currentEnemyNum++;
        }



        //该玩家还未创建
        if (currentPlayerNum < maxPlayer)
        {
            for (var i = 0; i < preparationAIPlayer.Count; i++)
            {
                currentPlayerNum++;
                var data = preparationAIPlayer[i];
                //位置
                float x = UnityEngine.Random.Range(node1.position.x, node3.position.x);
                float y = 0;
                float z = UnityEngine.Random.Range(node2.position.z, node4.position.z);

                GameObject gameObject = objectPooler.SpawnFromPool("AIPlayer", new Vector3(x, y, z), Quaternion.identity);
                gameObject.name = data.uid.ToString();
                //gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = LoadTexture(data.uid.ToString());
                //给对象写入指定uid和名字
                gameObject.transform.GetComponent<AI_Player>().aiPlayerInfo.uid = data.uid;
                gameObject.transform.GetComponent<AI_Player>().aiPlayerInfo.playerName = data.name;
                gameObject.transform.Find("PlayerName").GetComponent<TMP_Text>().text = data.name;
                surviveAIPlayer.Add(gameObject);


                //生成后将预备里的移除
                preparationAIPlayer.RemoveAt(i);
                print("预备生成->" + preparationAIPlayer.Count);
                break;
            }
        }


    }

    /// <summary>
    /// 创建AI玩家
    /// </summary>
    /// <param name="data"></param>
    public void CreateAIPlayer(Root data)
    {
        //gameObject.GetComponent<KillList>().SendKillMsg(data.text);
        //print(data.name + ":" + data.text);


        //生成AI玩家
        if (!surviveAIPlayer.Exists(t => t.GetComponent<AI_Player>().aiPlayerInfo.uid == data.uid))
        {
            //该玩家还未创建，加到预备生成玩家
            if (!preparationAIPlayer.Exists(t => t.uid == data.uid))
            {
                //上场玩家满了，丢到预备玩家里,并且预备队员里没有添加，有就不添加
                preparationAIPlayer.Add(data);
            }
        }
        else if (data.text.Length > 0)
        {
            //已经生成角色,显示玩家聊天内容
            GameObject gameObject = surviveAIPlayer.Find(t => t.transform.GetComponent<AI_Player>().aiPlayerInfo.uid == data.uid);
            gameObject.GetComponent<AI_Player>().ShowPlayerChat(data.text);

        }
    }

    /// <summary>
    /// 设置玩家Buff
    /// </summary>
    /// <param name="data"></param>
    public void SetPlayerBuff(Root data)
    {
        print(JsonConvert.SerializeObject(data));

        if (surviveAIPlayer.Exists(t => t.GetComponent<AI_Player>().aiPlayerInfo.uid == data.uid))
        {

            //该玩家已上场并没死亡
            GameObject gameObject = surviveAIPlayer.Find(t => t.transform.GetComponent<AI_Player>().aiPlayerInfo.uid == data.uid);

            if (data.giftName.Equals("辣条"))
            {
                gameObject.GetComponent<AI_Player>().AndBlood(50);
            }
            else if (data.giftName.Equals("小花花"))
            {
                gameObject.GetComponent<AI_Player>().addProyectileSpeed(0.01f);
            }
            else if (data.giftName.Equals("牛哇牛哇"))
            {
                gameObject.GetComponent<AI_Player>().addMoveSpeed(0.5f);
            }
            else if (data.giftName.Equals("打call"))
            {

            }
            else if (data.giftName.Equals("这个好诶"))
            {
                gameObject.GetComponent<AI_Player>().RandomBuff();
            }
            else if (data.giftName.Equals("舰长"))
            {
                //舰长 31037

            }

        }
        else
        {
            CreateAIPlayer(data);
        }
    }

    //Sprite LoadTexture(string uid)
    //{
    //    string savePath = Application.persistentDataPath + $"/{uid}.png"; // 使用文件流读取本地图片文件
    //    if (File.Exists(savePath))
    //    {
    //        FileStream fs = new FileStream(savePath, FileMode.Open);
    //        byte[] buffer = new byte[fs.Length];
    //        fs.Read(buffer, 0, buffer.Length);
    //        fs.Close();
    //        // 创建texture并设置图片格式，4通道32位，不使用mipmap
    //        Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB4444, false);
    //        //var iSLoad = texture.LoadImage(buffer);
    //        texture.Apply(); // 创建精灵，中心点默认（0, 0）
    //        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

    //        return sprite;

    //    }
    //    else
    //    {
    //        return defultHead;
    //    }
    //}


    void OnDrawGizmos()
    {
        //敌人生成点
        Gizmos.color = Color.red;
        for (var i = 0; i < monsterSpawn.Count; i++)
        {
            Gizmos.DrawSphere(monsterSpawn[i].transform.position, 0.5f);
        }


        //绘画生成AI玩家参考范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(node1.position, 0.25f);
        Gizmos.DrawSphere(node2.position, 0.25f);
        Gizmos.DrawSphere(node3.position, 0.25f);
        Gizmos.DrawSphere(node4.position, 0.25f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(node1.position, node2.position);
        Gizmos.DrawLine(node2.position, node3.position);
        Gizmos.DrawLine(node3.position, node4.position);
        Gizmos.DrawLine(node4.position, node1.position);
    }
}

