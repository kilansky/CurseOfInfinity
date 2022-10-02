using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public ParticleSystem waterParticles;

    private CapsuleCollider capCollider;

    private void Start()
    {
        capCollider = GetComponent<CapsuleCollider>();
    }

    public void DisableWaterFountain()
    {
        waterParticles.Stop();
        capCollider.enabled = false;
    }

    public void EnableWaterFountain()
    {
        waterParticles.Play();
        capCollider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            if(GameManager.Instance.playerNeedsWater)
            {
                AudioManager.Instance.Play("WaterDrank");
                GameManager.Instance.WaterDrank();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            if (GameManager.Instance.playerNeedsWater)
                GameManager.Instance.WaterDrank();
        }
    }
}
