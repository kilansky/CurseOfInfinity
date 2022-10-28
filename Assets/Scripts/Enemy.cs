using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int health = 1;
    public float moveSpeed = 2f;
    private NavMeshAgent navMeshAgent;
    private Transform player;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerController>().transform;

        health = GameManager.Instance.baseEnemyHealth;
        IncreaseScaleFromHealth();

        navMeshAgent.speed = GameManager.Instance.baseEnemySpeed;
    }

    void Update()
    {
        navMeshAgent.destination = player.position;
    }

    //Set enemy scale based on health value
    private void IncreaseScaleFromHealth()
    {
        if(health > 1)
        {
            float scaleAmt = (0.35f * (health - 1)) + transform.localScale.x;
            transform.localScale = new Vector3(scaleAmt, scaleAmt, scaleAmt);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            // GameManager.Instance.GameOver(causesOfDeath.zombie);
        }
    }

    public void TakeDamage()
    {
        health -= 1;
        AudioManager.Instance.Play("ZombieHit");

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
