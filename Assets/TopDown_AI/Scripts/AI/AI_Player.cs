using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AI_Player : MonoBehaviour, IPooledObject
{
    //Rigidbody myRigidBody;
    public float defaultproyectileSpeed = 0.2f;
    public float proyectileSpeed = 0.2f;
    public float defaultmoveSpeed = 8f;
    public GameObject playerChat;
    public GameObject gameManager;
    public AIPlayerInfo aiPlayerInfo = new AIPlayerInfo();
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    public Animator animator;
    //public GameObject proyectilePrefab;
    public Transform hitTestPivot, gunPivot;
    public Transform[] retreatPivot;
    public RectTransform UIcurrentBlood;
    public LayerMask hitTestLayer;
    public GameObject name;
    public GameObject bloob;


    //是否子弹穿透
    public bool isPenetrate;
    //是否吸血
    public bool isproyectilesuckBlood;
    //是否多弹道
    public bool ismultipleballistics;
    //是否启动快枪
    public bool isfastGun;




    Misc_Timer attackTimer = new Misc_Timer();
    Misc_Timer msgTimer = new Misc_Timer();
    //
    WeaponType currentWeapon = WeaponType.NULL;
    //最近的敌人目标
    GameObject targetEnemy;
    //默认血量
    public float defaultBlood;
    //当前血量
    public float currentBlood;

    //武器范围距离
    float weaponRange;
    //攻击间隔
    float attackTime = 0.4f;
    //是否正在后撤
    bool isRetreat = false;
    //上次后撤的动作
    int retreatindex = 0;
    //连续后撤次数
    int retreatNum = 0;

    ObjectPooler objectPooler;

    bool isfit = false;
    // Start is called before the first frame update
    public void OnObjectSpawn()
    {
        //对象池初始化
        currentBlood = defaultBlood;
        navMeshAgent.speed = defaultmoveSpeed;
        proyectileSpeed = defaultproyectileSpeed;
        SetWeapon(WeaponType.PISTOL);

    }

    private void Start()
    {
        //查找游戏管理
        gameManager = GameObject.Find("GameManager");
        objectPooler = ObjectPooler.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetEnemy == null)
        {
            //停止行动
            navMeshAgent.isStopped = true;
            //搜索最近目标
            SelectEnemy();
        }
        else
        {
            //已经有目标
            if (!targetEnemy.isStatic)
            {
                //目标被消灭
                SelectEnemy();
            }
            if (targetEnemy != null)
            {
                //把头转向目标
                float deltaY = targetEnemy.transform.position.z - transform.position.z;
                float deltaX = targetEnemy.transform.position.x - transform.position.x;
                float angleInDegrees = Mathf.Atan2(deltaY, deltaX) * 180 / Mathf.PI;
                Quaternion rotation = Quaternion.Euler(0, -angleInDegrees, 0);

                transform.rotation = rotation;

                //距离敌人射线
                RaycastHit diffhit;
                Physics.Raycast(transform.position, gunPivot.forward, out diffhit, Mathf.Infinity, hitTestLayer);

                if (diffhit.distance <= 8f && diffhit.collider != null && diffhit.collider.tag == "Enemy" && isRetreat == false || isfit == true)
                {
                    //距离敌人太近，往后退
                    //停止行动
                    navMeshAgent.isStopped = true;
                    isRetreat = true;
                    retreatNum += 1;


                    if (retreatNum >= 3)
                    {
                        //随机设置后撤点
                        retreatindex = Random.Range(0, retreatPivot.Length);
                        //重置连续后撤次数
                        retreatNum = 0;
                    };
                    navMeshAgent.isStopped = false;
                    navMeshAgent.SetDestination(retreatPivot[retreatindex].position);

                    //后撤多少秒
                    Invoke("CancelRetreat", 1f);
                }

                if (diffhit.collider != null && diffhit.distance <= weaponRange || isfit == true)
                {
                    //距离足够，开火
                    if (!isRetreat)
                    {
                        navMeshAgent.isStopped = true;
                    }

                    if (isfit == true && attackTimer.IsFinished() && !gameObject.isStatic || attackTimer.IsFinished() && diffhit.collider.tag == "Enemy" && !gameObject.isStatic)
                    {
                        Attack();
                    }

                }
                else
                {
                    navMeshAgent.isStopped = false;
                    //敌人没进到范围，移动
                    navMeshAgent.SetDestination(targetEnemy.transform.position);
                }
            }
        }


        //更新玩家血量
        if (currentBlood <= 0)
        {
            //触发死亡
            Death();
        }

        //更新UI
        UIcurrentBlood.sizeDelta = new Vector2(currentBlood, UIcurrentBlood.sizeDelta.y);

        //更新射击间隔时间
        attackTimer.UpdateTimer();
        //更新消息显示时间
        msgTimer.UpdateTimer();

        if (msgTimer.IsFinished())
        {
            playerChat.SetActive(false);
        }

        //将聊天显示和镜头平行
        Quaternion q = Quaternion.identity;
        q.SetLookRotation(Camera.main.transform.forward, Camera.main.transform.up);
        playerChat.transform.rotation = q;
        //将玩家名字显示和镜头平行
        name.transform.rotation = q;
        //将血条显示和镜头平行
        bloob.transform.rotation = q;
    }

    public void SelectEnemy()
    {
        //搜索敌人
        targetEnemy = null;
        GameObject[] closest = GameObject.FindGameObjectsWithTag("Enemy");
        if (closest.Length != 0)
        {
            var distance = Mathf.Infinity;

            var position = transform.position;

            foreach (GameObject go in closest)
            {
                if (!go.isStatic)
                {
                    var diff = (go.transform.position - position); //计算player与Enemy的向量距离差
                    var curDistance = diff.sqrMagnitude; //将向量距离平方(防止有负数产生)

                    if (curDistance < distance)
                    { //找出最近距离
                        targetEnemy = go; //更新最近距离敌人
                        distance = curDistance; //更新最近距离
                    }
                }
            }
        }
    }

    public void CancelRetreat()
    {
        //取消后撤
        isRetreat = false;
    }

    //攻击
    public void Attack()
    {
        switch (currentWeapon)
        {
            case WeaponType.KNIFE:
                Invoke("DoHitTest", 0.2f);
                break;
            case WeaponType.PISTOL:
                //决定单发还是多发
                for (int i = 0; i < (ismultipleballistics ? 3 : 1); i++)
                {
                    //GameObject birdshot = Instantiate(proyectilePrefab, weaponPivot.position, weaponPivot.rotation);
                    //birdshot.transform.Rotate(0, Random.Range(-15, 15), 0);

                    GameObject bullet = objectPooler.SpawnFromPool("Proyectile_Player", gunPivot.position, gunPivot.rotation);
                    bullet.transform.GetComponent<Proyectile_Simple>().launcher = gameObject;
                    bullet.transform.GetComponent<Proyectile_Simple>().speed = proyectileSpeed;
                    bullet.transform.GetComponent<Proyectile_Simple>().isPenetrate = isPenetrate;
                    bullet.transform.LookAt(gunPivot);
                    bullet.transform.Rotate(0, Random.Range(-7.5f, 7.5f), 0);
                }

                break;
        }
        //animator.SetBool("Attack", true);
        //CancelInvoke("AttackOver");
        //Invoke("AttackOver", attackTime);
        if (isfastGun)
        {
            attackTime = 0.1f;
        }

        attackTimer.StartTimer(attackTime);

    }

    void SetWeapon(WeaponType weaponType)
    {
        //设置武器
        if (weaponType != currentWeapon)
        {
            currentWeapon = weaponType;
            //animator.SetTrigger("WeaponChange");
            switch (weaponType)
            {
                case WeaponType.KNIFE:
                    weaponRange = 1.0f;
                    attackTime = 0.4f;
                    //animator.SetInteger("WeaponType", 0);
                    break;
                case WeaponType.PISTOL:
                    weaponRange = 12.0f;
                    attackTime = 0.5f;
                    //animator.SetInteger("WeaponType", 3);
                    break;
            }
        }
        //GameManager.SelectWeapon(weaponType);
    }

    void AttackOver()
    {
        //animator.SetBool("Attack", false);
    }

    public void ShowPlayerChat(string msg)
    {
        //显示玩家的聊天
        msgTimer.StartTimer(3f);
        playerChat.SetActive(true);
        playerChat.transform.Find("msg").GetComponent<TMP_Text>().text = msg;
    }

    public void Damage(GameObject launcher, float harm)
    {
        //受到伤害
        GameObject bloodChanges = objectPooler.SpawnFromPool("bloodChanges", transform.position, Quaternion.identity);
        bloodChanges.GetComponent<TMP_Text>().color = Color.white;
        bloodChanges.GetComponent<TMP_Text>().text = harm.ToString();


        currentBlood -= harm;

        //设置相机定位到自己
        if (currentBlood <= defaultBlood / 1.42)
        {
            //血只剩一半移动镜头
            gameManager.GetComponent<GameManager>().gameCamera.setGameObject(gameObject);
        }
    }

    #region Buff
    /// <summary>
    /// 加子弹速度
    /// </summary>
    /// <param name="num"></param>
    public void addProyectileSpeed(float num)
    {
        if (proyectileSpeed + num > 1f)
        {
            proyectileSpeed += num;
        }
        else
        {
            proyectileSpeed = 1f;
        }
    }

    /// <summary>
    /// 加移动速度
    /// </summary>
    /// <param name="num"></param>
    public void addMoveSpeed(float num)
    {
        if (navMeshAgent.speed + num < 20f)
        {
            navMeshAgent.speed += num;
        }
        else
        {
            navMeshAgent.speed = 20f;
        }
    }

    /// <summary>
    /// 修复血量
    /// </summary>
    /// <param name="num"></param>
    /// <param name="ispb"></param>
    public void AndBlood(float num, bool ispb = false)
    {
        if (currentBlood + num >= defaultBlood)
        {
            currentBlood = defaultBlood;
        }
        else
        {
            currentBlood += num;
        }

        //显示回血文字
        GameObject bloodChanges = objectPooler.SpawnFromPool("bloodChanges", transform.position, Quaternion.identity);
        bloodChanges.GetComponent<TMP_Text>().color = Color.green;

        bloodChanges.GetComponent<TMP_Text>().text = (ispb ? "吸血" : "恢复血量") + num.ToString();

    }

    /// <summary>
    /// 随机抽取一个buff
    /// </summary>
    public void RandomBuff()
    {

    }

    #endregion
    public void Death()
    {
        //死亡
        GameCamera.ToggleShake(0.3f);//晃动镜头
        gameObject.SetActive(false);
        gameManager.GetComponent<GameManager>().currentPlayerNum--;
        gameManager.GetComponent<KillList>().SendKillMsg(aiPlayerInfo.playerName);//发送被击杀信息

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            //已经和敌人靠在一起
            isfit = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            //已经脱离敌人
            isfit = false;
        }
    }


}

public class AIPlayerInfo
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int uid { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string playerName { get; set; }

    /// <summary>
    /// 击杀
    /// </summary>
    public int killNum { get; set; }

    /// <summary>
    /// 氪金
    /// </summary>
    public int kryptonGold { get; set; }
}
