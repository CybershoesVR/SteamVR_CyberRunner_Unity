﻿using System.Collections;
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
    [SerializeField] string eventPlayerName;
    [SerializeField] bool eventActive = false;

    [SerializeField] TextMeshProUGUI uiTimeText;
    [SerializeField] RectTransform scoreList;
    [SerializeField] GameObject scoreRowPrefab;
    [Space]
    [SerializeField] float timePerGem;
    [SerializeField] float finishTimeout = 1f;
    [SerializeField] Color playerScoreHighlight;
    [SerializeField] int leaderBoardEntryMax = 6;

    private FinishBoard finishBoard;
    private TextMeshProUGUI highscoreInfo;

    private bool raceActive = false;

    private float time = 0;
    private float bestTime = 1000000; //Default Value set really high so a first best time can be achieved
    private float lastTime = 0;

    private int currentRank = 9999;

    private int playerCount = 1;

    private Gem[] gemObjects;

    //private string scoreListPath;
    //private FileStream stream;

    private SteamLeaderboardManager steamLeaderboard;
    private EventLeaderboardManager eventLeaderboard;

    #endregion

    private void Start()
    {
        if (!eventActive)
        {
            steamLeaderboard = FindObjectOfType<SteamLeaderboardManager>();
            steamLeaderboard.AssignEntryMax(leaderBoardEntryMax);
            steamLeaderboard.FindLeaderboard("BestTime");
            StartCoroutine(LoadSteamLeaderboardStats());
        }
        else
        {
            eventLeaderboard = FindObjectOfType<EventLeaderboardManager>();
            eventLeaderboard.AssignEntryMax(leaderBoardEntryMax);
            eventLeaderboard.LoadScoreList();
            LoadNewPlayerStats();
            //DisplayEventLeaderboardStats(eventLeaderboard.GetScoreRange(eventPlayerName));
        }

        gemObjects = FindObjectsOfType<Gem>();
        Debug.Log($"Number of Gems: {gemObjects.Length}");

        finishBoard = FindObjectOfType<FinishBoard>();
        highscoreInfo = finishBoard.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        //Count up time and display it
        if (raceActive)
        {
            time += Time.deltaTime;
            uiTimeText.text = $"{time:#0.000}";
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (!eventActive)
            {
                steamLeaderboard.DownloadScores();
            }
            else
            {
                playerCount++;
                eventPlayerName = $"Runner#{playerCount}";
                LoadNewPlayerStats();
            }
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
            lastTime = Mathf.Floor(time * 1000) / 1000;

            if (lastTime < bestTime)
            {
                bestTime = lastTime;

                highscoreInfo.text = "New Highscore!!!";

                if (!eventActive)
                {
                    steamLeaderboard.UploadScore(bestTime);
                }
                else
                {
                    currentRank = eventLeaderboard.GetPlayerRank(bestTime);
                    eventLeaderboard.SaveScore(new LocalScoreEntry(eventPlayerName, currentRank, bestTime));
                }
            }
            else
            {
                highscoreInfo.text = "You can be faster!!!";
            }

            if (!eventActive)
            {
                StartCoroutine(LoadSteamLeaderboardStats());
            }
            else
            {
                DisplayEventLeaderboardStats(eventLeaderboard.GetScoreRange(eventPlayerName));
            }

            finishBoard.Popup(finishTimeout);

            uiTimeText.text = $"{lastTime:#0.000}";

            foreach (Gem coin in gemObjects)
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
            uiTimeText.text = $"{lastTime:#0.000}";
        }
        else
        {
            uiTimeText.text = "0.000";
        }

        foreach (Gem gem in gemObjects)
        {
            gem.Respawn();
        }
    }

    IEnumerator LoadSteamLeaderboardStats()
    {
        while (steamLeaderboard.downloadInProgress || steamLeaderboard.uploadInProgress)
        {
            yield return new WaitForSeconds(0.25f);
        }

        steamLeaderboard.DownloadScores();

        while (steamLeaderboard.downloadInProgress)
        {
            yield return new WaitForSeconds(0.25f);
        }

        ProcessSteamLeaderboardStats();
        DisplaySteamLeaderboardStats();
    }

    void ProcessSteamLeaderboardStats()
    {
        for (int i = 0; i < steamLeaderboard.loadedScores.Length; i++)
        {
            if (steamLeaderboard.loadedScores[i].playerID == SteamUser.GetSteamID())
            {
                bestTime = steamLeaderboard.loadedScores[i].score;
                currentRank = steamLeaderboard.loadedScores[i].rank;
            }
        }
    }

    void LoadNewPlayerStats()
    {
        Debug.Log("Stats Reloading...");

        if (eventLeaderboard.loadedScores != null)
        {
            for (int i = 0; i < eventLeaderboard.loadedScores.Count; i++)
            {
                if (eventLeaderboard.loadedScores[i].playerName == eventPlayerName)
                {
                    bestTime = eventLeaderboard.loadedScores[i].score;
                    currentRank = eventLeaderboard.loadedScores[i].rank;
                    DisplayEventLeaderboardStats(eventLeaderboard.GetScoreRange(eventPlayerName));
                    return;
                }
            }
        }

        bestTime = 1000000;
        currentRank = 9999;
        DisplayEventLeaderboardStats(eventLeaderboard.GetScoreRange(eventPlayerName));
    }

    void DisplaySteamLeaderboardStats()
    {
        int childCount = scoreList.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Destroy(scoreList.GetChild(i).gameObject);
        }

        if (steamLeaderboard.loadedScores == null)
        {
            Debug.LogError("Loaded Scores empty!");
            return;
        }

        for (int i = 0; i < steamLeaderboard.loadedScores.Length; i++)
        {
            GameObject newRow = Instantiate(scoreRowPrefab, scoreList);

            newRow.transform.Find("RankText").GetComponent<TextMeshProUGUI>().text = steamLeaderboard.loadedScores[i].rank.ToString();
            newRow.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = SteamFriends.GetFriendPersonaName(steamLeaderboard.loadedScores[i].playerID);
            newRow.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = $"{steamLeaderboard.loadedScores[i].score:#0.000}";

            if (steamLeaderboard.loadedScores[i].playerID == SteamUser.GetSteamID())
            {
                newRow.GetComponent<Image>().color = playerScoreHighlight;
            }
        }
    }

    void DisplayEventLeaderboardStats(List<LocalScoreEntry> scoreRange)
    {
        int childCount = scoreList.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Destroy(scoreList.GetChild(i).gameObject);
        }

        if (scoreRange == null)
        {
            Debug.Log("Loaded Scores empty!");
            return;
        }

        for (int i = 0; i < scoreRange.Count; i++)
        {
            GameObject newRow = Instantiate(scoreRowPrefab, scoreList);

            newRow.transform.Find("RankText").GetComponent<TextMeshProUGUI>().text = scoreRange[i].rank.ToString();
            newRow.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = scoreRange[i].playerName;
            newRow.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = $"{scoreRange[i].score:#0.000}";

            if (scoreRange[i].playerName == eventPlayerName)
            {
                newRow.GetComponent<Image>().color = playerScoreHighlight;
            }
        }
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

            uiTimeText.text = $"{time:#0.000}";
        }
    }
}
