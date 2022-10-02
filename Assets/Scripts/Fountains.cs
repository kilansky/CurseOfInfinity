using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fountains : MonoBehaviour
{
    public List<Water> fountains = new List<Water>();

    [Header("Curse of Drought")]
    public float minWaterActiveTime;
    public float maxWaterActiveTime;

    private int activeFountainIndex = 0;

    public void StartCurseOfDrought()
    {
        int activeFountainIndex = Random.Range(0, fountains.Count);

        for (int i = 0; i < fountains.Count; i++)
        {
            if(i != activeFountainIndex)
                fountains[i].DisableWaterFountain();
        }

        StartCoroutine(WaitToToggleFountains());
    }

    private void ToggleFountains()
    {
        for (int i = 0; i < fountains.Count; i++)
        {
            fountains[i].DisableWaterFountain();
        }

        activeFountainIndex++;
        if (activeFountainIndex >= fountains.Count)
            activeFountainIndex = 0;

        fountains[activeFountainIndex].EnableWaterFountain();

        StartCoroutine(WaitToToggleFountains());
    }

    private IEnumerator WaitToToggleFountains()
    {
        float timeToWait = Random.Range(minWaterActiveTime, maxWaterActiveTime);

        while (timeToWait > 0)
        {
            yield return new WaitForEndOfFrame();
            timeToWait -= Time.deltaTime;
        }

        ToggleFountains();
    }
}
