using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AI_Enemy : MonoBehaviour, IPooledObject
{
    //Rigidbody myRigidBody;
    //public float moveSpeed = 10.0f;
    public GameObject gameManager;
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    public Animator animator;
    public GameObject proyectilePrefab;
    public Transform gunPivot;
    public LayerMask hitTestLayer;
    //默认血量
    public float defaultBlood = 100f;
    //当前血量
    public float currentBlood = 100f;
    Misc_Timer attackTimer = new Misc_Timer();
    //当前使用的武器
    WeaponType currentWeapon = WeaponType.KNIFE;
    //最近的敌人目标
    GameObject targetEnemy;

    /// <summary>
    /// 武器范围距离
    /// </summary>
    float weaponRange;
    /// <summary>
    /// 武器伤害
    /// </summary>
    float weapngDamage = 5;
    /// <summary>
    /// 武器伤害范围
    /// </summary>
    float weaponDamageRange = 0;
    /// <summary>
    /// 攻击间隔
    /// </summary>
    float attackTime = 0.4f;


    ObjectPooler objectPooler;

    // Start is called before the first frame update
    public void OnObjectSpawn()
    {
        currentBlood = defaultBlood;
        SetWeapon(WeaponType.KNIFE);
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

                //距离敌人射线
                RaycastHit diffhit;
                Physics.Raycast(transform.position, gunPivot.forward, out diffhit, Mathf.Infinity, hitTestLayer);

                //把头转向目标
                float deltaY = targetEnemy.transform.position.z - transform.position.z;
                float deltaX = targetEnemy.transform.position.x - transform.position.x;
                float angleInDegrees = Mathf.Atan2(deltaY, deltaX) * 180 / Mathf.PI;
                Quaternion rotation = Quaternion.Euler(0, -angleInDegrees, 0);

                transform.rotation = rotation;//物体向慢慢朝向目标

                if (diffhit.collider != null && diffhit.distance <= weaponRange)
                {
                    //距离足够，开火
                    if (attackTimer.IsFinished() && diffhit.collider.tag == "Player" && !gameObject.isStatic)
                    {
                        navMeshAgent.isStopped = true;
                        Attack(diffhit);
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


        //更新血量
        if (currentBlood <= 0)
        {
            //触发死亡
            Death();

        }

        //更新射击间隔时间
        attackTimer.UpdateTimer();


    }

    public void SelectEnemy()
    {
        //搜索敌人
        targetEnemy = null;
        GameObject[] closest = GameObject.FindGameObjectsWithTag("Player");
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

    //攻击
    public void Attack(RaycastHit diffhit)
    {
        switch (currentWeapon)
        {
            case WeaponType.KNIFE:
                if (!diffhit.collider.gameObject.isStatic)
                {
                    diffhit.transform.GetComponent<AI_Player>().Damage(gameObject, weapngDamage);
                }
                break;
            case WeaponType.TNT:
                if (!diffhit.collider.gameObject.isStatic)
                {
                    //显示特效
                    GameObject smallExp = objectPooler.SpawnFromPool("Small_Explositon", transform.position, transform.rotation);
                    GameCamera.ToggleShake(0.6f);//晃动镜头
                    //触发范围伤害
                    var collidedObj = Physics.OverlapSphere(transform.position, weaponDamageRange);
                    foreach (var obj in collidedObj)
                    {
                        if (obj.tag == "Player")
                        {
                            //给范围玩家施加伤害
                            obj.transform.GetComponent<AI_Player>().Damage(gameObject, weapngDamage);
                        }
                    }

                    Death();
                }
                break;

            case WeaponType.PISTOL:
                //GameCamera.ToggleShake(0.01f);
                GameObject bullet = objectPooler.SpawnFromPool("Proyectile_Player", gunPivot.position, gunPivot.rotation);

                bullet.transform.LookAt(gunPivot);
                bullet.transform.Rotate(0, Random.Range(-7.5f, 7.5f), 0);
                break;
        }

        animator.SetBool("Attack", true);
        CancelInvoke("AttackOver");
        Invoke("AttackOver", attackTime);
        attackTimer.StartTimer(attackTime);


    }

    /// <summary>
    /// 设置敌人类型
    /// </summary>
    /// <param name="enemyType"></param>

    /// <summary>
    /// 设置武器
    /// </summary>
    /// <param name="weaponType"></param>
    void SetWeapon(WeaponType weaponType)
    {
        //if (weaponType != currentWeapon)
        //{
        currentWeapon = weaponType;
        //animator.SetTrigger("WeaponChange");
        switch (weaponType)
        {
            case WeaponType.KNIFE:
                weaponRange = 1.2f;
                attackTime = 0.4f;
                weapngDamage = 5f;
                weaponDamageRange = 0;
                defaultBlood = 100;
                currentBlood = 100;
                navMeshAgent.speed = 10;

                animator.SetInteger("WeaponType", 0);
                break;
            case WeaponType.TNT:
                weaponRange = 1.5f;
                attackTime = 99f;
                weapngDamage = 20f;
                weaponDamageRange = 3f;
                defaultBlood = 50;
                currentBlood = 50;
                navMeshAgent.speed = 12;

                animator.SetInteger("WeaponType", 4);

                break;
            case WeaponType.PISTOL:
                weaponRange = 12.0f;
                attackTime = 0.5f;
                weapngDamage = 10f;
                weaponDamageRange = 0;
                defaultBlood = 100;
                currentBlood = 100;
                navMeshAgent.speed = 10;
                //animator.runtimeAnimatorController = animationList[0];
                //animator.SetInteger("WeaponType", 3);
                break;
        }
        //}
        //GameManager.SelectWeapon(weaponType);
    }

    void AttackOver()
    {
        animator.SetBool("Attack", false);
    }

    public void Damage(GameObject launcher, float harm)
    {
        GameObject bloodChanges = objectPooler.SpawnFromPool("bloodChanges", transform.position, Quaternion.identity);
        bloodChanges.GetComponent<TMP_Text>().color = Color.red;
        bloodChanges.GetComponent<TMP_Text>().text = harm.ToString();
        //扣血
        currentBlood -= harm;
    }

    public void Death()
    {
        //死亡
        attackTimer.StopTimer();
        gameObject.SetActive(false);
        gameManager.GetComponent<GameManager>().currentEnemyNum--;

    }

}
