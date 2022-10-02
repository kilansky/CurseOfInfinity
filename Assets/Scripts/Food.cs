using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public bool isOrange;

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerController>())
        {
            GameManager.Instance.FoodEaten(isOrange);
            Destroy(gameObject);
        }
    }
}
