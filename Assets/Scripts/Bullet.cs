using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float fireSpeed = 100f;
    private Rigidbody rb;

    private void Start()
    {
        Destroy(gameObject, 6f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<Enemy>())
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage();
        }

        Destroy(gameObject);
    }
}
