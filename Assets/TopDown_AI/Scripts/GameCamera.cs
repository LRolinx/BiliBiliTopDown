using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{
    public Transform targetCamera;
    public Camera cameras;
    public int minSize = 8;
    public int maxSize = 18;
    Vector3 offset;
    static GameCamera myslf;
    Misc_Timer shakeTimer = new Misc_Timer();
    GameObject currentTrackedObject;
    ObjectPooler objectPooler;
    Misc_Timer attackTimer = new Misc_Timer();
    void Awake()
    {
        myslf = this;
    }

    // Use this for initialization
    void Start()
    {
        //currentTrackedObject = trackedObject;

    }

    private void Update()
    {
        attackTimer.UpdateTimer();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //	offset = offsetObject.position - trackedObject.position;
        //targetCamera.position = Vector3.Lerp(targetCamera.position, currentTrackedObject.position, 0.05f) + offset;
        FindPlayer();
        shakeTimer.UpdateTimer();
        if (shakeTimer.IsActive())
            UpdateShake();


        //if (Input.GetKeyDown(KeyCode.LeftShift))
        //{
        //    currentTrackedObject = trackedObjectZoom;
        //}
        //if (Input.GetKeyUp(KeyCode.LeftShift))
        //{
        //    currentTrackedObject = trackedObject;
        //}

    }

    void FindPlayer()
    {
        if (attackTimer.IsFinished() && currentTrackedObject == null || attackTimer.IsFinished() && !currentTrackedObject.isStatic)
        {
            //被观察的对象已经停用
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");

            if (gameObjects.Length != 0)
            {
                var distance = Mathf.Infinity;

                var position = transform.position;

                foreach (GameObject go in gameObjects)
                {
                    if (!go.isStatic)
                    {
                        var diff = (go.transform.position - position); //计算player与Enemy的向量距离差
                        var curDistance = diff.sqrMagnitude; //将向量距离平方(防止有负数产生)

                        if (curDistance < distance)
                        { //找出最近距离
                            currentTrackedObject = go; //更新最近距离敌人

                            targetCamera.position = Vector3.Lerp(targetCamera.position, currentTrackedObject.transform.position, 0.03f) + offset;
                            distance = curDistance; //更新最近距离
                        }
                    }
                }
            }
        }
        else
        {
            //观察对象还没被停用
            targetCamera.position = Vector3.Lerp(targetCamera.position, currentTrackedObject.transform.position, 0.05f) + offset;
        }
    }

    public void setGameObject(GameObject gameObject)
    {
        //设置相机指定指定对象
        if (attackTimer.IsFinished())
        {
            //血少一半再显示
            currentTrackedObject = gameObject;
            //定个时
            attackTimer.StartTimer(3f);
        }

    }

    float shakeDelay = 0.03f, lastShakeTime = float.MinValue;
    void UpdateShake()
    {
        //摇晃镜头
        if (lastShakeTime + shakeDelay < Time.time)
        {
            Vector3 shakePosition = Vector3.zero;
            shakePosition.x += Random.Range(-0.5f, 0.5f);
            shakePosition.y += Random.Range(-0.5f, 0.5f);
            targetCamera.transform.Translate(shakePosition);
            //targetCamera.transform.localPosition = shakePosition+targetCamera.transform.localPosition;
            lastShakeTime = Time.time;
        }
    }
    //Vector3 camLocalPos;
    //bool shakeActive;
    public static void ToggleShake(float shakeTime)
    {
        myslf.shakeTimer.StartTimer(shakeTime);
        //	myslf.shakeActive = toggleValue;
        //if (!toggleValue) {
        //	myslf.targetCamera.transform.localPosition=myslf.camLocalPos;
        //}
    }
}
