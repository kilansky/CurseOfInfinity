using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Skull : SingletonPattern<Skull>
{
    public GameObject pickupCanvas;

    [HideInInspector] public bool canPickUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            pickupCanvas.SetActive(true);
            canPickUp = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            pickupCanvas.SetActive(false);
            canPickUp = false;
        }
    }
}
