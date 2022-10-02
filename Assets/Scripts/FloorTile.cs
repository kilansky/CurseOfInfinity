using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FloorTile : MonoBehaviour
{
    [Header("Spawning")]
    public Transform spawnPoint;
    public bool canSpawnObject = true;

    [Header("Warning")]
    public GameObject warningEffect;
    public float warningTime = 1f;

    [Header("Tile Lowering")]
    public GameObject navMeshBlockerPrefab;
    public float tileLowerSpeed = 1f;
    public float lowerDistance = 10f;
    public bool canLower = true;

    private bool playerBlockingSpawn = false;
    private bool enemyBlockingSpawn = false;
    private bool tileLowering = false;

    public void RemoveTile()
    {
        gameObject.layer = LayerMask.NameToLayer("RemovedTile");
        transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("RemovedTile");
        TileManager.Instance.RemoveFloorTile(this);
        tileLowering = true;
        canSpawnObject = false;
        warningEffect.SetActive(true);
        StartCoroutine(TileWarning());
    }

    IEnumerator TileWarning()
    {
        float timeElapsed = 0f;

        while (timeElapsed < warningTime)
        {
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(LowerTile());
    }

    IEnumerator LowerTile()
    {
        Instantiate(navMeshBlockerPrefab, spawnPoint.position, Quaternion.identity);

        while (lowerDistance > 0)
        {
            lowerDistance += transform.position.y - (tileLowerSpeed * Time.deltaTime);
            transform.position = transform.position - (new Vector3(0, tileLowerSpeed, 0) * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(canLower)
        {
            if (other.GetComponent<PlayerController>())
            {
                canSpawnObject = false;
                playerBlockingSpawn = true;
            }

            if (other.GetComponent<Enemy>())
            {
                canSpawnObject = false;
                enemyBlockingSpawn = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(canLower)
        {
            if (other.GetComponent<PlayerController>() && playerBlockingSpawn)
                playerBlockingSpawn = false;

            if (other.GetComponent<Enemy>())
                enemyBlockingSpawn = false;

            if (!playerBlockingSpawn && !enemyBlockingSpawn && !tileLowering)
                canSpawnObject = true;
        }
    }
}
