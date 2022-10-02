using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    //Publics
    [Header ("Player Stats")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 12f;
    public float killWorldHeight = -10f;
    public float maxLanternIntensity;
    public float minLanternIntensity;
    public float gunFireRate = 1f;

    [Header("Object References")]
    public Light[] lanternLights = new Light[2];
    public GameObject boyModel;
    public GameObject boyGunModel;
    public GameObject boyLanternModel;
    public GameObject boyLanternGunModel;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [HideInInspector] public bool canMove = true;

    //Privates
    private Vector3 movementVector;
    private Quaternion lastTargetRotation;
    private CharacterController controller;
    private float gravity = 9.81f / 3;
    private float vSpeed = 0;
    private bool hasGun = false;
    private bool hasLantern = false;
    private bool canFire = true; //false after shooting until gunFireRate time has elapsed 
    private bool gamePaused = false;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(canMove)
        {
            MovePlayer();
            RotatePlayer();
        }

        SetLanternBrightness();
    }

    private void MovePlayer()
    {
        //Set vertical speed to zero if grounded
        if (controller.isGrounded)
            vSpeed = 0;
        else //otherwise, calculate gravity
            vSpeed -= gravity * Time.deltaTime;

        movementVector.y = vSpeed;
        controller.Move(movementVector * moveSpeed * Time.deltaTime);

        if(transform.position.y < killWorldHeight)
        {
            GameManager.Instance.GameOver(causesOfDeath.fell);
        }
    }

    //Returns true if the player is standing on the ground
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.15f);
    }

    //Set movementVector based on movement input
    public void Move(InputAction.CallbackContext context)
    {
        if (!gamePaused && canMove)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            movementVector = new Vector3(moveInput.x, 0, moveInput.y); //Set input y value to z axis
        }
    }

    //Pickup skull or fire gun
    public void Fire(InputAction.CallbackContext context)
    {
        if(context.performed && !gamePaused && canMove)
        {
            if (Skull.Instance && Skull.Instance.canPickUp)
                GameManager.Instance.SkullGrabbed();

            if (hasGun)
                ShootGun();
        }
    }

    //Pause the game
    public void Pause(InputAction.CallbackContext context)
    {
        if (context.performed && !GameManager.Instance.gameOver)
        {
            if(gamePaused)//unpause the game
            {
                gamePaused = false;
                Time.timeScale = 1f;
                HudManager.Instance.Unpause();
            }
            else//pause the game
            {
                gamePaused = true;
                Time.timeScale = 0f;
                HudManager.Instance.Pause();
            }
        }
    }

    public void GameOverRetry(InputAction.CallbackContext context)
    {
        if (context.performed && GameManager.Instance.canResetGame)
            GameManager.Instance.ResetLevel();
    }

    public void GameOverQuit(InputAction.CallbackContext context)
    {
        if (context.performed && GameManager.Instance.canResetGame)
            GameManager.Instance.QuitToMenu();
    }

    public void ActivateLanternCurse()
    {
        boyModel.SetActive(false);
        boyGunModel.SetActive(false);
        boyLanternModel.SetActive(false);

        if (hasGun)
            boyLanternGunModel.SetActive(true);
        else
            boyLanternModel.SetActive(true);

        lanternLights[0].gameObject.SetActive(true);
        lanternLights[1].gameObject.SetActive(true);
        lanternLights[0].intensity = 2;
        lanternLights[1].intensity = 2;

        hasLantern = true;
    }

    public void ActivateGunCurse()
    {
        boyModel.SetActive(false);
        boyGunModel.SetActive(false);
        boyLanternModel.SetActive(false);

        if (hasLantern)
            boyLanternGunModel.SetActive(true);
        else
            boyGunModel.SetActive(true);

        hasGun = true;
    }

    private void RotatePlayer()
    {
        //Smoothly Rotate Character in movement direction (if moving)
        if (Mathf.Abs(movementVector.x) > 0 || Mathf.Abs(movementVector.z) > 0)
        {
            Vector3 rotationVector = new Vector3(movementVector.x, 0, movementVector.z);
            Quaternion targetRotation = Quaternion.LookRotation(rotationVector);

            //Change rotation speed based on player state
            float rotSpeed = rotateSpeed;

            //Smoothly Rotate Player
            lastTargetRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
            transform.rotation = lastTargetRotation;
        }
    }

    private void SetLanternBrightness()
    {
        float percentCharged = GameManager.Instance.lanternTimer / 10f;
        float newIntensity = Mathf.Lerp(minLanternIntensity, maxLanternIntensity, percentCharged);

        lanternLights[0].intensity = newIntensity;
        lanternLights[1].intensity = newIntensity;
    }

    private void ShootGun()
    {
        if(canFire)
        {
            Vector3 spawnDirection = GetBulletSpawnDirection();
            Quaternion spawnRotation = Quaternion.LookRotation(spawnDirection, Vector3.up);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, spawnRotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(spawnDirection * bullet.GetComponent<Bullet>().fireSpeed);
            AudioManager.Instance.Play("BulletFired");

            canFire = false;
            StartCoroutine(RefreshGun());
        }
    }

    private IEnumerator RefreshGun()
    {
        yield return new WaitForSeconds(gunFireRate);
        canFire = true;
    }

    //Returns the transform of the closest enemy from the designated point (used for auto-aiming)
    private Transform GetClosestEnemy(Vector3 fromPoint, float autoAimRange)
    {
        //Get all monsters in the scene, store in 'monsters' array
        List<Enemy> enemies = new List<Enemy>(FindObjectsOfType<Enemy>());

        //Get the distance to the first monster in the array, set as closest monster
        float distToClosestEnemy = float.MaxValue;
        Transform closestEnemy = null;

        //Determine which monster is closest to the player
        foreach (Enemy enemy in enemies)
        {
            float enemyDist = Vector3.Distance(fromPoint, enemy.transform.position);
            if (enemyDist <= autoAimRange && enemyDist <= distToClosestEnemy)
                closestEnemy = enemy.transform;
        }

        return closestEnemy;
    }

    //Returns the direction to spawn a bullet with auto-aiming
    private Vector3 GetBulletSpawnDirection()
    {
        Vector3 spawnDirection = transform.forward;
        Quaternion spawnRotation = lastTargetRotation;

        Vector3 aheadOfPlayerPoint = transform.position + transform.forward * 5f;
        Transform closestMonsterController = GetClosestEnemy(aheadOfPlayerPoint, 5f);

        if (closestMonsterController != null)
            spawnDirection = new Vector3(closestMonsterController.position.x, 0, closestMonsterController.position.z) - new Vector3(firePoint.transform.position.x, 0, firePoint.transform.position.z);

        //spawnDirection = spawnDirection.normalized / 2;

        return spawnDirection.normalized;
    }
}