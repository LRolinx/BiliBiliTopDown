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
    //Ĭ��Ѫ��
    public float defaultBlood = 100f;
    //��ǰѪ��
    public float currentBlood = 100f;
    Misc_Timer attackTimer = new Misc_Timer();
    //��ǰʹ�õ�����
    WeaponType currentWeapon = WeaponType.KNIFE;
    //����ĵ���Ŀ��
    GameObject targetEnemy;

    /// <summary>
    /// ������Χ����
    /// </summary>
    float weaponRange;
    /// <summary>
    /// �����˺�
    /// </summary>
    float weapngDamage = 5;
    /// <summary>
    /// �����˺���Χ
    /// </summary>
    float weaponDamageRange = 0;
    /// <summary>
    /// �������
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
        //������Ϸ����
        gameManager = GameObject.Find("GameManager");
        objectPooler = ObjectPooler.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetEnemy == null)
        {
            //ֹͣ�ж�
            navMeshAgent.isStopped = true;
            //�������Ŀ��
            SelectEnemy();
        }
        else
        {
            //�Ѿ���Ŀ��
            if (!targetEnemy.isStatic)
            {
                //Ŀ�걻����
                SelectEnemy();
            }
            if (targetEnemy != null)
            {

                //�����������
                RaycastHit diffhit;
                Physics.Raycast(transform.position, gunPivot.forward, out diffhit, Mathf.Infinity, hitTestLayer);

                //��ͷת��Ŀ��
                float deltaY = targetEnemy.transform.position.z - transform.position.z;
                float deltaX = targetEnemy.transform.position.x - transform.position.x;
                float angleInDegrees = Mathf.Atan2(deltaY, deltaX) * 180 / Mathf.PI;
                Quaternion rotation = Quaternion.Euler(0, -angleInDegrees, 0);

                transform.rotation = rotation;//��������������Ŀ��

                if (diffhit.collider != null && diffhit.distance <= weaponRange)
                {
                    //�����㹻������
                    if (attackTimer.IsFinished() && diffhit.collider.tag == "Player" && !gameObject.isStatic)
                    {
                        navMeshAgent.isStopped = true;
                        Attack(diffhit);
                    }

                }
                else
                {
                    navMeshAgent.isStopped = false;
                    //����û������Χ���ƶ�
                    navMeshAgent.SetDestination(targetEnemy.transform.position);
                }
            }
        }


        //����Ѫ��
        if (currentBlood <= 0)
        {
            //��������
            Death();

        }

        //����������ʱ��
        attackTimer.UpdateTimer();


    }

    public void SelectEnemy()
    {
        //��������
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
                    var diff = (go.transform.position - position); //����player��Enemy�����������
                    var curDistance = diff.sqrMagnitude; //����������ƽ��(��ֹ�и�������)

                    if (curDistance < distance)
                    { //�ҳ��������
                        targetEnemy = go; //��������������
                        distance = curDistance; //�����������
                    }
                }
            }
        }
    }

    //����
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
                    //��ʾ��Ч
                    GameObject smallExp = objectPooler.SpawnFromPool("Small_Explositon", transform.position, transform.rotation);
                    GameCamera.ToggleShake(0.6f);//�ζ���ͷ
                    //������Χ�˺�
                    var collidedObj = Physics.OverlapSphere(transform.position, weaponDamageRange);
                    foreach (var obj in collidedObj)
                    {
                        if (obj.tag == "Player")
                        {
                            //����Χ���ʩ���˺�
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
    /// ���õ�������
    /// </summary>
    /// <param name="enemyType"></param>

    /// <summary>
    /// ��������
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
        //��Ѫ
        currentBlood -= harm;
    }

    public void Death()
    {
        //����
        attackTimer.StopTimer();
        gameObject.SetActive(false);
        gameManager.GetComponent<GameManager>().currentEnemyNum--;

    }

}
