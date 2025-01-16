using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] Animator cameraAnimator;
    public int gameDuration = 300;

    [SerializeField] private int maxLifes = 3;
    [SerializeField] private GameObject player;
    [SerializeField] private int GetPowerUpAtCoins = 10;
    [SerializeField] private Transform defaultCheckpoint;

    [Space(10)]
    [SerializeField]
    private AudioSource gameplayMusic;
    [SerializeField]
    private AudioSource victoryMusic;
    [SerializeField]
    private GameObject winEffect;

    [HideInInspector]
    public int playerCoins;
    [HideInInspector]
    public int playerRedCoins;
    [HideInInspector]
    public float currentTime = 300;

    [HideInInspector]
    public int playerBlocks;
    [HideInInspector]
    public int playerGoldens;

    [HideInInspector]
    public bool isEndGame = false;

    private int powerUpCount;
    private PlayerController controller;
    private PlayerPowerUp powerUp;
    private PlayerHealth health;
    private Animator lastCheckpoint;
    private Transform currentCheckpoint;
    private bool doTimer = false;
  

    public static GameManager instance;

    private void Awake()
    {
        if (instance)
        {
            Destroy(instance.gameObject);
        }

        instance = this;
        controller = player.GetComponent<PlayerController>();
        powerUp = player.GetComponent<PlayerPowerUp>();
        health = player.GetComponent<PlayerHealth>();

        cameraAnimator.SetTrigger("FadeToGameplay");

        currentCheckpoint = defaultCheckpoint;

        currentTime = gameDuration;

        Application.targetFrameRate = 60;

    }

    private void Start()
    {
        UIController.instance.SetTime(Mathf.RoundToInt(currentTime));
        UIController.instance.SetLife(maxLifes);
    }

    public void AddCoin(int amount)
    {
        playerCoins += amount;
        UIController.instance.AddCoin(playerCoins);

        if (!controller.isGolden)
        {
            powerUpCount += amount;

            float perc = (float)powerUpCount / ((float)GetPowerUpAtCoins + 1);
            UIController.instance.Fill(perc);

            if (powerUpCount >= GetPowerUpAtCoins)
            {
                powerUpCount = 0;
                powerUp.DoPowerUp();
            }
        }

    }

    public void AddBlock()
    {
        playerBlocks += 1;
    }

    public void AddGolden()
    {
        playerGoldens += 1;
    }

    public void AddRedCoin()
    {
        playerRedCoins += 1;
    }

    public void IgnorePlayerCollision(Collider toIgnore)
    {
        Physics.IgnoreCollision(player.GetComponent<Collider>(), toIgnore, true);
    }

    public bool SetCheckpoint(Transform point, Animator anim)
    {
        if (lastCheckpoint != anim)
        {
            if (lastCheckpoint)
            {
                lastCheckpoint.SetTrigger("Close");
            }

            currentCheckpoint = point;
            lastCheckpoint = anim;

            return true;
        }
        else
        {
            return false;
        }

    }

    public void Death(float delay)
    {
        maxLifes -= 1;

        UIController.instance.SetLife(maxLifes);

        if (maxLifes <= 0)
        {
            StartCoroutine(GameOverSequence(delay));
        }
        else
        {
            StartCoroutine(RestartSequence(delay));
        }
    }

    IEnumerator GameOverSequence(float delay)
    {
        yield return new WaitForSeconds(delay);
        cameraAnimator.SetTrigger("FadeToBlack");
        yield return new WaitForSeconds(1.1f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator RestartSequence(float delay)
    {
        yield return new WaitForSeconds(delay);
        cameraAnimator.SetTrigger("FadeToBlack");
        yield return new WaitForSeconds(1f);

        //Move player to checkpoint:
        player.transform.position = currentCheckpoint.position;
        player.transform.eulerAngles = new Vector3(0,0,0);

        powerUpCount = 0;

        yield return new WaitForSeconds(0.2f);
        cameraAnimator.SetTrigger("FadeToGameplay");

        health.Revive();
    }

    public void StartTimer()
    {
        doTimer = true;
    }

    public void StopTimer()
    {
        doTimer = false;
    }

    public void PowerUpCamera()
    {
        cameraAnimator.SetTrigger("PowerUp");
    }

    public void StopPlayer()
    {
        controller.enabled = false;
        controller.GetComponent<Rigidbody>().useGravity = true;
    }

    public void EndGame()
    {
        gameplayMusic.Stop();
        victoryMusic.Play();
        isEndGame = true;
        CreateWinEffect();
    }

    public void CreateWinEffect()
    {
        Instantiate(winEffect, controller.transform.position + Vector3.up * 2, Quaternion.identity);
    }

    
    private void Update()
    {
        if (doTimer)
        {
            currentTime -= Time.deltaTime;
            UIController.instance.SetTime(Mathf.Clamp(Mathf.RoundToInt(currentTime),0, 999999));

            if (currentTime <= 0)
            {
                StartCoroutine(GameOverSequence(0.1f));
                doTimer = false;
            }
        }

        if (isEndGame)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(GameOverSequence(0.1f));
                enabled = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Application.targetFrameRate = 10;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Application.targetFrameRate = 25;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Application.targetFrameRate = 60;
        }

    }

}

