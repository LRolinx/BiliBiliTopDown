using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatTextManager : MonoBehaviour,IPooledObject
{

    /// <summary>
    /// 飘字时间
    /// </summary>
    float _floatTime = 0.5f;

    public void OnObjectSpawn()
    {
        Vector3 pos = new Vector3(transform.position.x - 1.5f, 0, transform.position.z);
        transform.position = pos;

        Invoke("hide", _floatTime);
    }

    /// <summary>
    /// 飘字
    /// </summary>
    /// <param name="str">文字</param>
    /// <param name="parent">挂点</param>
    public void FixedUpdate()
    {
        //Vector3 point = new Vector3(Screen.width / 2, 0, Screen.height / 2);
        //transform.LookAt(Camera.main.ScreenToWorldPoint(point));//字体和屏幕平行

        Quaternion q = Quaternion.identity;
        q.SetLookRotation(Camera.main.transform.forward, Camera.main.transform.up);
        transform.rotation = q;
    }

    private void hide()
    {
        gameObject.SetActive(false);
    }
    
}