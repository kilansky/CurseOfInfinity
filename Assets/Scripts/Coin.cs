using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float spinSpeed = 100f;
    public Transform coinModel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            GameManager.Instance.CoinCollected();
            Destroy(gameObject);
        }
    }

    void Update()
    {
        coinModel.RotateAround(coinModel.position, coinModel.up, spinSpeed * Time.deltaTime);
    }
}
