using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class RaceController : MonoBehaviour
{
    //Variables
    #region

    [SerializeField] float timeScoreMultiplier;
    [Space]
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

    private string scoreListPath;
    private FileStream stream;

    #endregion


    private void Start()
    {
        LoadStats();

        //Only display if values are not default
        if (highscore != 0)
        {
            highscoreText.text = "Highscore: " + highscore;
            bestTimeText.text = "Time: " + bestTime;
            bestGemText.text = "Gems: " + bestGems;
        }

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
            SaveStats();

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

        if (lastScore != 0)
        {
            uiPlayerGemText.text = "Gems: " + lastGems;
            uiPlayerTimeText.text = "Time: " + lastTime;
        }
        else
        {
            uiPlayerTimeText.text = "Time: 0";
            uiPlayerGemText.text = "Gems: 0";
        }

        foreach (Coin gem in gemObjects)
        {
            gem.Respawn();
        }
    }

    public void ResetStats()
    {
        if (raceActive)
        {
            raceActive = false;
            time = 0;
            gems = 0;
        }

        lastTime = 0;
        bestTime = 1000000;
        lastGems = 0;
        bestGems = 0;
        highscore = 0;
        lastScore = 0;

        lastTimeText.text = "Time: -";
        uiPlayerTimeText.text = "Time: -";
        bestTimeText.text = "Time: -";
        lastGemText.text = "Gems: -";
        uiPlayerGemText.text = "Gems: -";
        bestGemText.text = "Gems: -";
        highscoreText.text = "Highscore: -";
        lastScoreText.text = "Score: -";

        SaveStats();
    }

    void SaveStats()
    {
        stream = new FileStream(scoreListPath, FileMode.Create);

        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(highscore);
        writer.Write(bestTime);
        writer.Write(bestGems);

        stream.Flush();
        writer.Close();
    }

    void LoadStats()
    {
        scoreListPath = Application.dataPath + "/Saves/";

        if (!Directory.Exists(scoreListPath))
        {
            Directory.CreateDirectory(scoreListPath);
        }

        scoreListPath += "ScoreList.cr";

        if (File.Exists(scoreListPath))
        {
            stream = new FileStream(scoreListPath, FileMode.Open);

            BinaryReader reader = new BinaryReader(stream);

            highscore = reader.ReadSingle();
            bestTime = reader.ReadSingle();
            bestGems = reader.ReadInt32();

            reader.Close();
        }
        else
        {
            SaveStats();
        }
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
