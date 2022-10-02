using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudManager : SingletonPattern<HudManager>
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;

    [Header("Curse Clock")]
    public GameObject curseClock;
    public Image clockOverlay;

    [Header("Hunger & Thirst")]
    public GameObject hungerMeter;
    public GameObject thirstMeter;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public Image hungerMeterIcon;
    public Image hungerFillBar;
    public Sprite melonSprite;
    public Color melonMeterColor;
    public Image hungerMeterRedBG;
    public Image thirstMeterRedBG;

    [Header("Curse Popup")]
    public GameObject cursePopup;
    public TextMeshProUGUI curseTitle;
    public TextMeshProUGUI curseDescription;
    public float textDisplayTime = 3.5f;
    public float camShakeIntensity = 0.5f;
    public float camShakeLength = 3.5f;

    [Header("Game Over Popup")]
    public GameObject gameOverPopup;
    public TextMeshProUGUI deathDescription;
    public TextMeshProUGUI finalScoreText;

    [Header("Paused Screen")]
    public GameObject pausePanel;
    public GameObject blackOverlay;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
        hungerMeter.SetActive(false);
        thirstMeter.SetActive(false);
        curseClock.SetActive(false);
        blackOverlay.SetActive(true);

        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        if (highscore > 0)
            highscoreText.text = "Best: " + highscore.ToString();
        else
            highscoreText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameManager.gameOver)
        {
            scoreText.text = gameManager.score.ToString();
            clockOverlay.fillAmount = Mathf.Lerp(1, 0, gameManager.curseTimer / 10f);
            SetMeterValues();
        }
    }

    //Sets the slider value and background color of the hunger and thirst meters
    private void SetMeterValues()
    {
        float hungerPerc = gameManager.hungerTimer / gameManager.baseStarvationTime;
        float thirstPerc = gameManager.thirstTimer / gameManager.baseDehydrationTime;

        hungerSlider.value = hungerPerc;
        thirstSlider.value = thirstPerc;

        Color tempColor = hungerMeterRedBG.color;
        tempColor.a = Mathf.Lerp(1, 0, hungerPerc);
        hungerMeterRedBG.color = tempColor;

        tempColor = thirstMeterRedBG.color;
        tempColor.a = Mathf.Lerp(1, 0, thirstPerc);
        thirstMeterRedBG.color = tempColor;
    }

    public void ActivateCurseClock()
    {
        curseClock.SetActive(true);
    }

    public void ActivateHungerMeter()
    {
        hungerMeter.SetActive(true);
    }

    public void ActivateThirstMeter()
    {
        thirstMeter.SetActive(true);
    }

    public void SetMelonMeter()
    {
        hungerFillBar.color = melonMeterColor;
        hungerMeterIcon.sprite = melonSprite;
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
    }

    public void Unpause()
    {
        pausePanel.SetActive(false);
    }

    public void DisplayCurse(Curse newCurse)
    {
        if (gameManager.gameOver)
            return;

        cursePopup.SetActive(true);
        curseTitle.text = "Curse of " + newCurse.curseName;
        curseDescription.text = newCurse.curseDescription;
        CineShake.Instance.Shake(camShakeIntensity, camShakeLength);
        StartCoroutine(WaitToRemoveCurseFromScreen());
    }

    public void SetGameOverScreen(causesOfDeath causeOfDeath)
    {
        gameOverPopup.SetActive(true);
        cursePopup.SetActive(false);
        CineShake.Instance.Shake(camShakeIntensity * 0.5f, camShakeLength / 2f);
        deathDescription.text = GameOverText(causeOfDeath);
        finalScoreText.text = "Final Score: " + GameManager.Instance.score.ToString();
    }

    private string GameOverText(causesOfDeath causeOfDeath)
    {
        string gameOverText;

        switch (causeOfDeath)
        {
            case causesOfDeath.dehydration:
                gameOverText = "Died of Dehydration";
                break;
            case causesOfDeath.starvation:
                gameOverText = "Starved to Death";
                break;
            case causesOfDeath.zombie:
                gameOverText = "You became a Zombie";
                break;
            case causesOfDeath.fell:
                gameOverText = "Stuck Falling for Eternity";
                break;
            case causesOfDeath.orange:
                gameOverText = "Had an allergic reaction from an orange. What a way to go.";
                break;
            default:
                gameOverText = "How did you Die??";
                break;
        }

        return gameOverText;
    }

    private IEnumerator WaitToRemoveCurseFromScreen()
    {
        yield return new WaitForSeconds(textDisplayTime);
        cursePopup.SetActive(false);
    }
}
