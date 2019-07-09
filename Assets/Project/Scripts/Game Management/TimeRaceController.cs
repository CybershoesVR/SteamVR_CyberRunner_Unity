using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeRaceController : MonoBehaviour
{
    //Variables
    #region

    [SerializeField] TextMeshProUGUI bestTimeText;
    [SerializeField] TextMeshProUGUI lastTimeText;
    [SerializeField] TextMeshProUGUI playerTimeText;

    [SerializeField] TextMeshProUGUI bestCoinText;
    [SerializeField] TextMeshProUGUI lastCoinText;
    [SerializeField] TextMeshProUGUI playerCoinText;

    private bool raceActive = false;

    private float time = 0;
    private float bestTime = 1000000; //Default Value set really high so a first best time can be achieved
    private float lastTime = 0;

    private int coins = 0;
    private int bestCoins = 0;
    private int lastCoins = 0;

    private Coin[] coinObjects;

    #endregion


    private void Start()
    {
        //Load Stats
        lastTime = PlayerPrefs.GetFloat("LastTime", 0);
        bestTime = PlayerPrefs.GetFloat("BestTime", 1000000);
        lastCoins = PlayerPrefs.GetInt("LastCoins", 0);
        bestCoins = PlayerPrefs.GetInt("BestCoins", 0);

        //Only display if values are not default
        if (lastTime != 0)
        {
            lastTimeText.text = "Last Time: " + lastTime;
        }
        if (bestTime != 1000000)
        {
            bestTimeText.text = "Best Time: " + bestTime;
        }

        lastCoinText.text = "Last Coins: " + lastCoins;
        bestCoinText.text = "Best Coins: " + bestCoins;

        coinObjects = FindObjectsOfType<Coin>();
    }

    private void Update()
    {
        //Count up time and display it
        if (raceActive)
        {
            time += Time.deltaTime;
            lastTimeText.text = "Last Time: " + time;
            playerTimeText.text = "Time: " + time;
        }
    }

    public void StartRace()
    {
        time = 0;
        raceActive = true;
    }

    public void FinishRace()
    {
        if (raceActive)
        {
            raceActive = false;
            lastTime = time;

            if (time < bestTime)
            {
                bestTime = time;
            }

            lastCoins = coins;

            if (coins > bestCoins)
            {
                bestCoins = coins;
            }

            coins = 0;

            //Save Stats
            PlayerPrefs.SetFloat("LastTime", lastTime);
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.SetInt("LastCoins", lastCoins);
            PlayerPrefs.SetInt("BestCoins", bestCoins);

            //Display Stats
            bestTimeText.text = "Best Time: " + bestTime;
            lastTimeText.text = "Last Time: " + lastTime;
            playerTimeText.text = "Time: " + lastTime;
            bestCoinText.text = "Best Coins: " + bestCoins;
            lastCoinText.text = "Last Coins: " + lastCoins;
            playerCoinText.text = "Coins: " + lastCoins;

            foreach (Coin coin in coinObjects)
            {
                coin.Respawn();
            }
        }
    }

    public void ResetRace()
    {
        time = 0;
        raceActive = false;

        if (lastTime != 0)
        {
            lastTimeText.text = "Last Time: " + lastTime;
            playerTimeText.text = "Time: " + lastTime;
        }
        else
        {
            lastTimeText.text = "Last Time: -";
            playerTimeText.text = "Time: -";
        }

        lastCoinText.text = "Last Coins: " + lastCoins;
        playerCoinText.text = "Coins: " + lastCoins;

        coins = 0;

        foreach (Coin coin in coinObjects)
        {
            coin.Respawn();
        }
    }

    public void ResetStats()
    {
        if (raceActive)
        {
            raceActive = false;
            time = 0;
        }

        lastTime = 0;
        lastTimeText.text = "Last Time: -";
        playerTimeText.text = "Time: -";

        bestTime = 1000000;
        bestTimeText.text = "Best Time: -";

        lastCoins = 0;
        lastCoinText.text = "Last Coins: " + lastCoins;
        playerCoinText.text = "Coins: " + lastCoins;

        bestCoins = 0;
        bestCoinText.text = "Best Coins: " + bestCoins;

        //Save Stats
        PlayerPrefs.SetFloat("LastTime", lastTime);
        PlayerPrefs.SetFloat("BestTime", bestTime);
        PlayerPrefs.SetInt("LastCoins", lastCoins);
        PlayerPrefs.SetInt("BestCoins", bestCoins);
    }

    public void AddCoins(int amount)
    {
        if (raceActive)
        {
            coins += amount;
            lastCoinText.text = "Last Coins: " + coins;
            playerCoinText.text = "Coins: " + coins;
        }
    }
}
