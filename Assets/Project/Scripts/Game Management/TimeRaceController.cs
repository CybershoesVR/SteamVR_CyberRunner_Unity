using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class TimeRaceController : MonoBehaviour
{
    //Variables
    #region

    [SerializeField] float timeScoreMultiplier;

    [SerializeField] TextMeshProUGUI bestTimeText;
    [SerializeField] TextMeshProUGUI lastTimeText;
    [SerializeField] TextMeshProUGUI uiPlayerTimeText;

    [SerializeField] TextMeshProUGUI bestGemText;
    [SerializeField] TextMeshProUGUI lastGemText;
    [SerializeField] TextMeshProUGUI uiPlayerGemText;

    [SerializeField] TextMeshProUGUI highscoreText;
    [SerializeField] TextMeshProUGUI lastScoreText;

    private bool raceActive = false;

    private float time = 0;
    private float bestTime = 1000000; //Default Value set really high so a first best time can be achieved
    private float lastTime = 0;

    private int gems = 0;
    private int bestGems = 0;
    private int lastGems = 0;

    private float highscore = 0;
    private float lastScore = 0;

    private Coin[] gemObjects;

    #endregion


    private void Start()
    {
        //Load Stats
        //lastTime = PlayerPrefs.GetFloat("LastTime", 0);
        //bestTime = PlayerPrefs.GetFloat("BestTime", 1000000);
        //lastCrystals = PlayerPrefs.GetInt("LastCoins", 0);
        //bestCrystals = PlayerPrefs.GetInt("BestCoins", 0);

        //Only display if values are not default
        //if (lastTime != 0)
        //{
        //    lastTimeText.text = "Last Time: " + lastTime;
        //}
        //if (bestTime != 1000000)
        //{
        //    bestTimeText.text = "Best Time: " + bestTime;
        //}

        //lastCoinText.text = "Last Coins: " + lastCrystals;
        //bestCoinText.text = "Best Coins: " + bestCrystals;

        gemObjects = FindObjectsOfType<Coin>();
    }

    private void Update()
    {
        //Count up time and display it
        if (raceActive)
        {
            time += Time.deltaTime;
            uiPlayerTimeText.text = "Time: " + time;
        }
    }

    public void StartRace()
    {
        time = 0;
        gems = 0;
        uiPlayerGemText.text = "Gems: 0";
        raceActive = true;
    }

    public void FinishRace()
    {
        if (raceActive)
        {
            raceActive = false;
            lastTime = time;
            lastGems = gems;

            lastScore = 1 / lastTime * timeScoreMultiplier + lastGems;

            if (lastScore > highscore)
            {
                highscore = lastScore;
                bestTime = lastTime;
                bestGems = lastGems;
            }

            //Save Stats
            //PlayerPrefs.SetFloat("LastTime", lastTime);
            //PlayerPrefs.SetFloat("BestTime", bestTime);
            //PlayerPrefs.SetInt("LastCoins", lastGems);
            //PlayerPrefs.SetInt("BestCoins", bestGems);
            FileStream stream = new FileStream(Application.dataPath + "/Assets/PlayerSave.bin", FileMode.Create);

            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(highscore);
            writer.Write(bestTime);
            writer.Write(bestGems);

            writer.Close();

            //Display Stats
            highscoreText.text = "Highscore: " + highscore;
            bestTimeText.text = "Time: " + bestTime;
            bestGemText.text = "Gems: " + bestGems;

            lastScoreText.text = "Score: " + lastScore;
            lastTimeText.text = "Time: " + lastTime;
            uiPlayerTimeText.text = "Time: " + lastTime;
            lastGemText.text = "Gems: " + lastGems;
            uiPlayerGemText.text = "Gems: " + lastGems;

            foreach (Coin coin in gemObjects)
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
            lastTimeText.text = "Time: " + lastTime;
            uiPlayerTimeText.text = "Time: " + lastTime;
        }
        else
        {
            lastTimeText.text = "Last Time: -";
            uiPlayerTimeText.text = "Time: -";
        }

        uiPlayerGemText.text = "Gems: " + lastGems;

        foreach (Coin coin in gemObjects)
        {
            coin.Respawn();
        }
    }

    public void ResetStats()
    {
        //if (raceActive)
        //{
        //    raceActive = false;
        //    time = 0;
        //}

        //lastTime = 0;
        //lastTimeText.text = "Last Time: -";
        //uiPlayerTimeText.text = "Time: -";

        //bestTime = 1000000;
        //bestTimeText.text = "Best Time: -";

        //lastGems = 0;
        //lastGemText.text = "Last Coins: " + lastGems;
        //uiPlayerGemText.text = "Coins: " + lastGems;

        //bestGems = 0;
        //bestGemText.text = "Best Coins: " + bestGems;

        ////Save Stats
        //PlayerPrefs.SetFloat("LastTime", lastTime);
        //PlayerPrefs.SetFloat("BestTime", bestTime);
        //PlayerPrefs.SetInt("LastCoins", lastGems);
        //PlayerPrefs.SetInt("BestCoins", bestGems);
    }

    public void AddCoins(int amount)
    {
        if (raceActive)
        {
            gems += amount;
            uiPlayerGemText.text = "Gems: " + gems;
        }
    }
}
