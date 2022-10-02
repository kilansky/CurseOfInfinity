using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firefly : MonoBehaviour
{
    public float wingFlapRate = 0.25f;
    public GameObject wingsOpen;
    public GameObject wingsClosed;

    private bool wingsOpened = true;

    void Start()
    {
        transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
        InvokeRepeating("FlapWings", 0, wingFlapRate);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * Time.deltaTime);
    }

    void FlapWings()
    {
        if(wingsOpened)
        {
            wingsClosed.SetActive(true);
            wingsOpen.SetActive(false);
        }
        else
        {
            wingsClosed.SetActive(false);
            wingsOpen.SetActive(true);
        }

        wingsOpened = !wingsOpened;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            GameManager.Instance.FireflyCollected();
            Destroy(gameObject);
        }
    }
}
