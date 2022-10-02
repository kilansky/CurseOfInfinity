using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public enum causesOfDeath
{
    dehydration,
    starvation,
    zombie,
    fell,
    orange
}

public class GameManager : SingletonPattern<GameManager>
{
    public int score = 0;

    [Header("Curse Activation Info")]
    [Range(0f, 30f)] public float baseCurseTime = 10f;
    [Range(0f, 30f)] public float baseStarvationTime = 10f;
    [Range(0f, 30f)] public float baseDehydrationTime = 10f;
    [Range(0f, 30f)] public float baseFoodSpawnTime = 10f;
    [Range(0f, 30f)] public float baseFireflySpawnTime = 10f;
    [Range(0f, 30f)] public float baseCoinSpawnTime = 10f;
    [Range(0f, 30f)] public float baseEnemySpawnTime = 10f;
    [Range(0f, 30f)] public float baseLanternDimTime = 10f;
    [Range(0f, 30f)] public float baseTileFallTime = 4f;
    [Range(0, 5)] public int baseEnemyHealth = 1;
    [Range(0, 5f)] public float baseEnemySpeed = 2f;
    [Range(0, 5f)] public float waterFillRate = 0.05f;

    //[Header("Curse Timers")]
    [HideInInspector] public float curseTimer;
    [HideInInspector] public float hungerTimer;
    [HideInInspector] public float thirstTimer;
    [HideInInspector] public float foodTimer;
    [HideInInspector] public float fireflyTimer;
    [HideInInspector] public float coinTimer;
    [HideInInspector] public float enemyTimer;
    [HideInInspector] public float tileTimer;
    [HideInInspector] public float lanternTimer;
    [HideInInspector] public bool canResetGame;

    [Header("Object References")]
    public GameObject[] foodPrefabs;
    public GameObject coinPrefab;
    public GameObject enemyPrefab;
    public GameObject fireflyPrefab;
    public Fountains fountains;
    public GameObject controlsCanvas;

    [Header("Sunlight & Brightness")]
    public Light sunlight;
    public float dayLightIntensity = 1f;
    public float nightLightIntensity = 1f;
    public float nightTransitionTime = 5f;

    [Header("Game Over")]
    public float gameOverTimeSlowFactor = 0.25f;

    [Header("Curses")]
    public List<Curse> stage1Curses = new List<Curse>();
    public List<Curse> stage2Curses = new List<Curse>();
    public List<Curse> stage3Curses = new List<Curse>();
    public Curse curseOfSkull;
    public Curse curseOfWealth;
    public Curse curseOfMoreWealth;
    public Curse curseOfGun;
    public Curse curseOfUnstableGround;

    [HideInInspector] public bool gameOver = false;
    [HideInInspector] public bool playerNeedsWater = false;

    private Curse nextCurse;
    private bool forceNextCurse;
    private bool stage1Cleared = false;
    private bool stage2Cleared = false;
    private bool allergicToOranges = false;
    private int lastFoodSpawnIndex = 0;

    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        Time.timeScale = 1f;
    }

    public IEnumerator CurseTimer()
    {
        while (curseTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            curseTimer -= Time.deltaTime;
        }

        StartNewCurse(ChooseRandomCurse());
        curseTimer = baseCurseTime;
        StartCoroutine(CurseTimer());
    }

    //Increases score by 1 each second that the player is cursed and alive
    public IEnumerator ScoreIncrementer()
    {
        while (!gameOver)
        {
            score += 1;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator FoodTimer()
    {
        while(foodTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            foodTimer -= Time.deltaTime;
        }

        SpawnNextFood();
        foodTimer = baseFoodSpawnTime;
        StartCoroutine(FoodTimer());
    }

    public IEnumerator HungerTimer()
    {
        hungerTimer = baseStarvationTime;

        while (hungerTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            hungerTimer -= Time.deltaTime;
        }
        GameOver(causesOfDeath.starvation);
    }

    public IEnumerator ThirstTimer()
    {
        thirstTimer = baseDehydrationTime;

        while (thirstTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            thirstTimer -= Time.deltaTime;
        }
        GameOver(causesOfDeath.dehydration);
    }

    public IEnumerator FireflyTimer()
    {
        while (fireflyTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            fireflyTimer -= Time.deltaTime;
        }

        SpawnObject(fireflyPrefab);
        fireflyTimer = baseFireflySpawnTime;
        StartCoroutine(FireflyTimer());
    }

    public IEnumerator CoinTimer()
    {
        while (coinTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            coinTimer -= Time.deltaTime;
        }

        SpawnObject(coinPrefab);
        coinTimer = baseCoinSpawnTime;
        StartCoroutine(CoinTimer());
    }

    public IEnumerator EnemyTimer()
    {
        while (enemyTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            enemyTimer -= Time.deltaTime;
        }

        SpawnObject(enemyPrefab);
        enemyTimer = baseEnemySpawnTime;
        StartCoroutine(EnemyTimer());
    }

    public IEnumerator TileTimer()
    {
        while (tileTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            tileTimer -= Time.deltaTime;
        }

        TileManager.Instance.GetRandomSpawnTile().RemoveTile();
        //RebuildNavMesh();
        tileTimer = baseTileFallTime;
        StartCoroutine(TileTimer());
    }

    public IEnumerator LanternTimer()
    {
        lanternTimer = baseLanternDimTime;

        while (lanternTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            lanternTimer -= Time.deltaTime;
        }
    }

    private Curse ChooseRandomCurse()
    {
        Curse newCurse;

        if(forceNextCurse)
        {
            forceNextCurse = false;
            return nextCurse;
        }
        else if (stage1Curses.Count > 0)
        {
            int curseIndex = Random.Range(0, stage1Curses.Count);
            newCurse = stage1Curses[curseIndex];
            stage1Curses.Remove(newCurse);
        }
        else if (stage2Curses.Count > 0)
        {
            if (!stage1Cleared)
            {
                stage1Cleared = true;
                nextCurse = curseOfUnstableGround;
                forceNextCurse = true;
                return curseOfWealth;
            }

            int curseIndex = Random.Range(0, stage2Curses.Count);
            newCurse = stage2Curses[curseIndex];
            stage2Curses.Remove(newCurse);
        }
        else
        {
            if (!stage2Cleared)
            {
                stage2Cleared = true;
                return curseOfMoreWealth;
            }

            newCurse = stage3Curses[Random.Range(0, stage3Curses.Count)];
        }

        return newCurse;
    }

    private void StartNewCurse(Curse newCurse)
    {
        HudManager.Instance.DisplayCurse(newCurse);
        AudioManager.Instance.Play("NewCurse");

        switch (newCurse.curseType)
        {
            case curses.Skull:
                curseTimer = baseCurseTime;
                StartCoroutine(CurseTimer());
                StartCoroutine(ScoreIncrementer());
                HudManager.Instance.ActivateCurseClock();
                break;
            case curses.Hunger:
                foodTimer = 1f;
                StartCoroutine(FoodTimer());
                StartCoroutine(HungerTimer());
                HudManager.Instance.ActivateHungerMeter();
                break;
            case curses.Starvation:
                baseStarvationTime /= 1.4f;
                break;
            case curses.Thirst:
                playerNeedsWater = true;
                StartCoroutine(ThirstTimer());
                HudManager.Instance.ActivateThirstMeter();
                break;
            case curses.Dehydration:
                baseDehydrationTime /= 1.4f;
                break;
            case curses.Zombies:
                enemyTimer = 1f;
                StartCoroutine(EnemyTimer());
                nextCurse = curseOfGun;
                forceNextCurse = true;
                break;
            case curses.Darkness:
                StartCoroutine(FadeToNight());
                player.ActivateLanternCurse();
                break;
            case curses.FadingLight:
                fireflyTimer = 1f;
                StartCoroutine(LanternTimer());
                StartCoroutine(FireflyTimer());
                break;
            case curses.Gun:
                player.ActivateGunCurse();
                break;
            case curses.UncontrollableFire:
                break;
            case curses.Ammo:
                break;
            case curses.Bombs:
                break;
            case curses.StrongerZombies:
                baseEnemyHealth += 1;
                break;
            case curses.Wealth:
                coinTimer = 1f;
                StartCoroutine(CoinTimer());
                break;
            case curses.Allergy:
                allergicToOranges = true;
                HudManager.Instance.SetMelonMeter();
                break;
            case curses.MoreStarvation:
                baseStarvationTime /= 1.4f;
                break;
            case curses.MoreDehydration:
                baseDehydrationTime /= 1.4f;
                break;
            case curses.MoreWealth:
                coinTimer = 1f;
                baseCoinSpawnTime /= 2f;
                break;
            case curses.MoreZombieHealth:
                baseEnemyHealth += 1;
                break;
            case curses.FastZombies:
                baseEnemySpeed += 1f;
                break;
            case curses.UnstableGround:
                tileTimer = 1f;
                StartCoroutine(TileTimer());
                break;
            case curses.MoreZombies:
                enemyTimer = 1f;
                baseEnemySpawnTime /= 1.4f;
                break;
            case curses.Drought:
                fountains.StartCurseOfDrought();
                break;
            default:
                break;
        }
    }

    private IEnumerator FadeToNight()
    {
        float timeElapsed = 0f;
        while(timeElapsed < nightTransitionTime)
        {
            float lightIntensity = Mathf.Lerp(dayLightIntensity, nightLightIntensity, timeElapsed / nightTransitionTime);
            sunlight.intensity = lightIntensity;
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        sunlight.intensity = nightLightIntensity;
    }

    public void SkullGrabbed()
    {
        Destroy(Skull.Instance.gameObject);
        controlsCanvas.SetActive(false);
        StartNewCurse(curseOfSkull);
    }

    public void FoodEaten(bool isOrange)
    {
        score += 1;
        AudioManager.Instance.Play("FoodCollected");

        if (allergicToOranges && isOrange)
            GameOver(causesOfDeath.orange);
        else
        {
            foodTimer = baseFoodSpawnTime;
            hungerTimer = baseStarvationTime;
            SpawnNextFood();
        }
    }

    public void WaterDrank()
    {
        thirstTimer = Mathf.Clamp(thirstTimer + waterFillRate, 0, baseDehydrationTime);
    }

    private void SpawnNextFood()
    {
        lastFoodSpawnIndex++;
        if (lastFoodSpawnIndex >= foodPrefabs.Length)
            lastFoodSpawnIndex = 0;

        GameObject foodToSpawn = foodPrefabs[lastFoodSpawnIndex];
        SpawnObject(foodToSpawn);
    }

    public void CoinCollected()
    {
        score += 5;
        AudioManager.Instance.Play("CoinCollected");
    }

    public void FireflyCollected()
    {
        AudioManager.Instance.Play("FireflyCollected");

        if (lanternTimer <= 0)
            StartCoroutine(LanternTimer());
        else
            lanternTimer = baseLanternDimTime;
    }

    public void GameOver(causesOfDeath causeOfDeath)
    {
        curseTimer = 10f;
        if (gameOver)
            return;

        gameOver = true;
        AudioManager.Instance.Play("PlayerDeath");
        FindObjectOfType<PlayerController>().canMove = false;
        Time.timeScale = gameOverTimeSlowFactor;
        HudManager.Instance.SetGameOverScreen(causeOfDeath);
        CheckForHighscore();
        StartCoroutine(WaitToEnableReset());
    }

    private void CheckForHighscore()
    {
        int highscore = PlayerPrefs.GetInt("Highscore", 0);

        if(score > highscore)
        {
            PlayerPrefs.SetInt("Highscore", score);
        }
    }

    private IEnumerator WaitToEnableReset()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        canResetGame = true;
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(1);
    }

    private void SpawnObject(GameObject objectToSpawn)
    {
        Transform spawnPoint = TileManager.Instance.GetRandomSpawnTile().spawnPoint.transform;

        Instantiate(objectToSpawn, spawnPoint.position, Quaternion.identity);
    }

    //Re-Build Navigation Mesh
    public void RebuildNavMesh()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
