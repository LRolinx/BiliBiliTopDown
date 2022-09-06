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


    //�Ƿ��ӵ���͸
    public bool isPenetrate;
    //�Ƿ���Ѫ
    public bool isproyectilesuckBlood;
    //�Ƿ�൯��
    public bool ismultipleballistics;
    //�Ƿ�������ǹ
    public bool isfastGun;




    Misc_Timer attackTimer = new Misc_Timer();
    Misc_Timer msgTimer = new Misc_Timer();
    //
    WeaponType currentWeapon = WeaponType.NULL;
    //����ĵ���Ŀ��
    GameObject targetEnemy;
    //Ĭ��Ѫ��
    public float defaultBlood;
    //��ǰѪ��
    public float currentBlood;

    //������Χ����
    float weaponRange;
    //�������
    float attackTime = 0.4f;
    //�Ƿ����ں�
    bool isRetreat = false;
    //�ϴκ󳷵Ķ���
    int retreatindex = 0;
    //�����󳷴���
    int retreatNum = 0;

    ObjectPooler objectPooler;

    bool isfit = false;
    // Start is called before the first frame update
    public void OnObjectSpawn()
    {
        //����س�ʼ��
        currentBlood = defaultBlood;
        navMeshAgent.speed = defaultmoveSpeed;
        proyectileSpeed = defaultproyectileSpeed;
        SetWeapon(WeaponType.PISTOL);

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
                //��ͷת��Ŀ��
                float deltaY = targetEnemy.transform.position.z - transform.position.z;
                float deltaX = targetEnemy.transform.position.x - transform.position.x;
                float angleInDegrees = Mathf.Atan2(deltaY, deltaX) * 180 / Mathf.PI;
                Quaternion rotation = Quaternion.Euler(0, -angleInDegrees, 0);

                transform.rotation = rotation;

                //�����������
                RaycastHit diffhit;
                Physics.Raycast(transform.position, gunPivot.forward, out diffhit, Mathf.Infinity, hitTestLayer);

                if (diffhit.distance <= 8f && diffhit.collider != null && diffhit.collider.tag == "Enemy" && isRetreat == false || isfit == true)
                {
                    //�������̫����������
                    //ֹͣ�ж�
                    navMeshAgent.isStopped = true;
                    isRetreat = true;
                    retreatNum += 1;


                    if (retreatNum >= 3)
                    {
                        //������ú󳷵�
                        retreatindex = Random.Range(0, retreatPivot.Length);
                        //���������󳷴���
                        retreatNum = 0;
                    };
                    navMeshAgent.isStopped = false;
                    navMeshAgent.SetDestination(retreatPivot[retreatindex].position);

                    //�󳷶�����
                    Invoke("CancelRetreat", 1f);
                }

                if (diffhit.collider != null && diffhit.distance <= weaponRange || isfit == true)
                {
                    //�����㹻������
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
                    //����û������Χ���ƶ�
                    navMeshAgent.SetDestination(targetEnemy.transform.position);
                }
            }
        }


        //�������Ѫ��
        if (currentBlood <= 0)
        {
            //��������
            Death();
        }

        //����UI
        UIcurrentBlood.sizeDelta = new Vector2(currentBlood, UIcurrentBlood.sizeDelta.y);

        //����������ʱ��
        attackTimer.UpdateTimer();
        //������Ϣ��ʾʱ��
        msgTimer.UpdateTimer();

        if (msgTimer.IsFinished())
        {
            playerChat.SetActive(false);
        }

        //��������ʾ�;�ͷƽ��
        Quaternion q = Quaternion.identity;
        q.SetLookRotation(Camera.main.transform.forward, Camera.main.transform.up);
        playerChat.transform.rotation = q;
        //�����������ʾ�;�ͷƽ��
        name.transform.rotation = q;
        //��Ѫ����ʾ�;�ͷƽ��
        bloob.transform.rotation = q;
    }

    public void SelectEnemy()
    {
        //��������
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

    public void CancelRetreat()
    {
        //ȡ����
        isRetreat = false;
    }

    //����
    public void Attack()
    {
        switch (currentWeapon)
        {
            case WeaponType.KNIFE:
                Invoke("DoHitTest", 0.2f);
                break;
            case WeaponType.PISTOL:
                //�����������Ƕ෢
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
        //��������
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
        //��ʾ��ҵ�����
        msgTimer.StartTimer(3f);
        playerChat.SetActive(true);
        playerChat.transform.Find("msg").GetComponent<TMP_Text>().text = msg;
    }

    public void Damage(GameObject launcher, float harm)
    {
        //�ܵ��˺�
        GameObject bloodChanges = objectPooler.SpawnFromPool("bloodChanges", transform.position, Quaternion.identity);
        bloodChanges.GetComponent<TMP_Text>().color = Color.white;
        bloodChanges.GetComponent<TMP_Text>().text = harm.ToString();


        currentBlood -= harm;

        //���������λ���Լ�
        if (currentBlood <= defaultBlood / 1.42)
        {
            //Ѫֻʣһ���ƶ���ͷ
            gameManager.GetComponent<GameManager>().gameCamera.setGameObject(gameObject);
        }
    }

    #region Buff
    /// <summary>
    /// ���ӵ��ٶ�
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
    /// ���ƶ��ٶ�
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
    /// �޸�Ѫ��
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

        //��ʾ��Ѫ����
        GameObject bloodChanges = objectPooler.SpawnFromPool("bloodChanges", transform.position, Quaternion.identity);
        bloodChanges.GetComponent<TMP_Text>().color = Color.green;

        bloodChanges.GetComponent<TMP_Text>().text = (ispb ? "��Ѫ" : "�ָ�Ѫ��") + num.ToString();

    }

    /// <summary>
    /// �����ȡһ��buff
    /// </summary>
    public void RandomBuff()
    {

    }

    #endregion
    public void Death()
    {
        //����
        GameCamera.ToggleShake(0.3f);//�ζ���ͷ
        gameObject.SetActive(false);
        gameManager.GetComponent<GameManager>().currentPlayerNum--;
        gameManager.GetComponent<KillList>().SendKillMsg(aiPlayerInfo.playerName);//���ͱ���ɱ��Ϣ

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            //�Ѿ��͵��˿���һ��
            isfit = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            //�Ѿ��������
            isfit = false;
        }
    }


}

public class AIPlayerInfo
{
    /// <summary>
    /// �û�ID
    /// </summary>
    public int uid { get; set; }

    /// <summary>
    /// �û���
    /// </summary>
    public string playerName { get; set; }

    /// <summary>
    /// ��ɱ
    /// </summary>
    public int killNum { get; set; }

    /// <summary>
    /// 봽�
    /// </summary>
    public int kryptonGold { get; set; }
}
