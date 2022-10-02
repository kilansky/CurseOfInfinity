using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject blackOverlay;

    private void Start()
    {
        Time.timeScale = 1f;
    }

    public void StartGame(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            blackOverlay.SetActive(true);
            StartCoroutine(LoadGame());
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator LoadGame()
    {
        yield return new WaitForSecondsRealtime(2.25f);
        SceneManager.LoadScene(1);
    }
}
