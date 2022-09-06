using UnityEngine;
using System.Collections;

public class Proyectile_Simple : MonoBehaviour, IPooledObject
{
    public enum CollisionTarget { PLAYER, ENEMIES }
    public CollisionTarget collisionTarget;
    public float lifeTime = 3.0f;
    public float speed = 1.5f;
    public float harm = 5f;
    /// <summary>
    /// 发射者
    /// </summary>
    public GameObject launcher;
    [Tooltip("是否有穿透")]
    public bool isPenetrate = false;

    //bool hitTest = true;
    public void OnObjectSpawn()
    {

        //moving = true;
        //Destroy(gameObject, lifeTime);
        Invoke("DestroyProyectile", lifeTime);
    }


    void FixedUpdate()
    {
        transform.Translate(transform.forward * speed, Space.World);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collisionTarget == CollisionTarget.PLAYER && collision.gameObject.tag == "Player")
        {
            //击中玩家

            collision.gameObject.GetComponent<AI_Player>().Damage(launcher, harm);
            if (!isPenetrate)
            {
                DestroyProyectile();
            }
        }
        else if (collisionTarget == CollisionTarget.ENEMIES && collision.gameObject.tag == "Enemy")
        {
            //击中敌人
            collision.gameObject.GetComponent<AI_Enemy>().Damage(launcher, harm);
            if (launcher.GetComponent<AI_Player>().isproyectilesuckBlood && Random.Range(0, 100) % 100 < 2)
            {
                //带吸血
                launcher.GetComponent<AI_Player>().AndBlood(harm, true);
            }
            if (!isPenetrate)
            {
                DestroyProyectile();
            }
        }
        else if (collision.gameObject.tag == "Finish")
        { //This is to detect if the proyectile collides with the world, i used this tag because it is standard in Unity (To prevent asset importing issues)
            DestroyProyectile();
        }



    }
    /// <summary>
    /// 销毁
    /// </summary>
    void DestroyProyectile()
    {

        /*hitTest=false;
		gameObject.GetComponent<Rigidbody> ().isKinematic = true;
		gameObject.GetComponent<Collider> ().enabled = false;
		moving = false;*/
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

}

