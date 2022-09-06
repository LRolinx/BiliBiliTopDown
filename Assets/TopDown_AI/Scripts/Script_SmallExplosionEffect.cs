using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_SmallExplosionEffect : MonoBehaviour, IPooledObject
{
    public float lifeTime = 3.0f;
    public void OnObjectSpawn()
    {

        //moving = true;
        //Destroy(gameObject, lifeTime);
        Invoke("DestroyProyectile", lifeTime);
    }

    /// <summary>
    /// Ïú»Ù
    /// </summary>
    void DestroyProyectile()
    {
        gameObject.SetActive(false);
    }
}
