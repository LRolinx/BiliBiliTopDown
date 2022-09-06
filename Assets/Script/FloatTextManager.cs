using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatTextManager : MonoBehaviour,IPooledObject
{

    /// <summary>
    /// Ʈ��ʱ��
    /// </summary>
    float _floatTime = 0.5f;

    public void OnObjectSpawn()
    {
        Vector3 pos = new Vector3(transform.position.x - 1.5f, 0, transform.position.z);
        transform.position = pos;

        Invoke("hide", _floatTime);
    }

    /// <summary>
    /// Ʈ��
    /// </summary>
    /// <param name="str">����</param>
    /// <param name="parent">�ҵ�</param>
    public void FixedUpdate()
    {
        //Vector3 point = new Vector3(Screen.width / 2, 0, Screen.height / 2);
        //transform.LookAt(Camera.main.ScreenToWorldPoint(point));//�������Ļƽ��

        Quaternion q = Quaternion.identity;
        q.SetLookRotation(Camera.main.transform.forward, Camera.main.transform.up);
        transform.rotation = q;
    }

    private void hide()
    {
        gameObject.SetActive(false);
    }
    
}