using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Steamworks;

public class RaceController : MonoBehaviour
{
    //Variables
    #region

    [SerializeField] TextMeshProUGUI bestTimeText;
    [SerializeField] TextMeshProUGUI lastTimeText;
    [SerializeField] TextMeshProUGUI uiPlayerTimeText;
    [Space]
    [SerializeField] float timePerGem;
    [SerializeField] float finishTimeout = 1f;

    private FinishBoard finishBoard;
    private TextMeshProUGUI highscoreInfo;

    private bool raceActive = false;

    private float time = 0;
    private float bestTime = 1000000; //Default Value set really high so a first best time can be achieved
    private float lastTime = 0;

    private Coin[] gemObjects;

    private string scoreListPath;
    private FileStream stream;

    private static SteamLeaderboard_t currentLeaderboard;
    private static CallResult<LeaderboardFindResult_t> findResult = new CallResult<LeaderboardFindResult_t>();

    #endregion


    private void Start()
    {
        LoadStats();

        //Only display if values are not default
        if (bestTime != 1000000)
        {
            bestTimeText.text = $"{bestTime:#0.000} s";
        }

        gemObjects = FindObjectsOfType<Coin>();

        finishBoard = FindObjectOfType<FinishBoard>();
        highscoreInfo = finishBoard.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        //Count up time and display it
        if (raceActive)
        {
            time += Time.deltaTime;
            uiPlayerTimeText.text = $"{time:#0.000}";
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

            if (lastTime < bestTime)
            {
                bestTime = lastTime;

                highscoreInfo.text = "New Highscore!!!";
            }
            else
            {
                highscoreInfo.text = "Too slow!!!";
            }

            finishBoard.Popup(finishTimeout);

            //Save Stats
            SaveStats();

            //Display Stats
            bestTimeText.text = $"{bestTime:#0.000} s";

            lastTimeText.text = $"{lastTime:#0.000} s";
            uiPlayerTimeText.text = $"{lastTime:#0.000}";

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
            uiPlayerTimeText.text = $"{lastTime:#0.000}";
        }
        else
        {
            uiPlayerTimeText.text = "0.000";
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
        }

        lastTime = 0;
        bestTime = 1000000;

        lastTimeText.text = "-";
        uiPlayerTimeText.text = "0.000";
        bestTimeText.text = "-";

        SaveStats();
    }

    void SaveStats()
    {
        stream = new FileStream(scoreListPath, FileMode.Create);

        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(bestTime);

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

            bestTime = reader.ReadSingle();

            reader.Close();
        }
        else
        {
            SaveStats();
        }

        if (SteamManager.Initialized)
        {
            SteamAPICall_t steamAPICall = SteamUserStats.FindLeaderboard("BestTime");
            findResult.Set(steamAPICall, OnLeaderboardFindResult);
            string name = SteamFriends.GetPersonaName();
            Debug.Log(name);
        }
    }

    static private void OnLeaderboardFindResult(LeaderboardFindResult_t callback, bool failure)
    {
        UnityEngine.Debug.Log("STEAM LEADERBOARDS: Found - " + callback.m_bLeaderboardFound + " leaderboardID - " + callback.m_hSteamLeaderboard.m_SteamLeaderboard);
        currentLeaderboard = callback.m_hSteamLeaderboard;
    }

    public void AddGems(int amount)
    {
        if (raceActive)
        {
            time -= amount * timePerGem;

            if (lastTime <= 0)
            {
                lastTime = 0.001f;
            }

            uiPlayerTimeText.text = $"{time:#0.000}";
        }
    }
}
