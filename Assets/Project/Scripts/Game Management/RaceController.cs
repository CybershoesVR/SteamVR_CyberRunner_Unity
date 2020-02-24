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
    [SerializeField] string eventPlayerName;
    [SerializeField] string cachedEmail = "[No Email]";

    [SerializeField] TextMeshProUGUI uiTimeText;
    [SerializeField] RectTransform scoreList;
    [SerializeField] GameObject scoreRowPrefab;
    [SerializeField] TextMeshProUGUI leaderboardTitle;
    [SerializeField] TextMeshProUGUI playerNameText;
    [Space]
    [SerializeField] float timePerGem;
    [SerializeField] float finishTimeout = 1f;
    [SerializeField] Color playerScoreHighlight;
    [SerializeField] int leaderBoardEntryMax = 6;
    [Space]
    [SerializeField] float speedDecreaseBonus;
    [SerializeField] float speedIncreaseMalus;

    private FinishBoard finishBoard;
    private TextMeshProUGUI highscoreInfo;

    private bool raceActive = false;

    private float time = 0;
    private float bestTime = 1000000; //Default Value set really high so a first best time can be achieved
    private float lastTime = 0;

    private int currentRank = 9999;

    private float defaultSpeed;
    private PlayerMovement player;

    private int gems;
    private int maxGems;

    private int marathonRunCounter = 0;

    private Gem[] gemObjects;

    //private string scoreListPath;
    //private FileStream stream;

    private SteamLeaderboardManager steamLeaderboard;
    private EventLeaderboardManager eventLeaderboard;

    #endregion

    private void Start()
    {
        if (SteamManager.Initialized)
        {
            steamLeaderboard = FindObjectOfType<SteamLeaderboardManager>();
            steamLeaderboard.AssignEntryMax(leaderBoardEntryMax);
            steamLeaderboard.FindLeaderboard("BestTime");
            StartCoroutine(LoadSteamLeaderboardStats());
            leaderboardTitle.text = "Global Leaderboard";
            playerNameText.text = SteamFriends.GetPersonaName();
        }
        else
        {
            eventLeaderboard = FindObjectOfType<EventLeaderboardManager>();
            eventLeaderboard.AssignEntryMax(leaderBoardEntryMax);
            eventLeaderboard.LoadScoreList();
            eventLeaderboard.ClearEmptyPlayers();
            eventPlayerName = "Runner#1";
            LoadNewPlayerStats();
            leaderboardTitle.text = eventLeaderboard.eventName;
        }

        player = FindObjectOfType<PlayerMovement>();
        defaultSpeed = player.GetSpeed();

        gemObjects = FindObjectsOfType<Gem>();
        maxGems = gemObjects.Length;
        Debug.Log($"Number of Gems: {maxGems}");

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

        if (Input.GetKeyDown(KeyCode.F5) && SteamManager.Initialized)
        {
            steamLeaderboard.DownloadScores();
        }

        if (Input.GetKeyDown(KeyCode.Return) && !SteamManager.Initialized)
        {
            eventLeaderboard.ClearEmptyPlayers();

            int exitCode = PlayerForm.AddPlayer(eventLeaderboard.scoreListPath);

            if (exitCode > 0)
            {
                eventLeaderboard.LoadScoreList();

                LocalScoreEntry newPlayer = eventLeaderboard.InitPlayer();

                eventPlayerName = newPlayer.playerName;
                cachedEmail = newPlayer.email;

                LoadNewPlayerStats();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && SteamManager.Initialized)
        {
            bestTime = 1000000;
            steamLeaderboard.UploadScore(bestTime,true);
        }
    }

    public void StartRace()
    {
        if (raceActive)
        {
            return;
        }


        float currentSpeed = player.GetSpeed();

        if (currentSpeed < defaultSpeed - 0.1f)
        {
            time = -speedDecreaseBonus;
        }
        else if (currentSpeed > defaultSpeed + 0.1f)
        {
            time = speedIncreaseMalus;
        }
        else
        {
            time = 0;
        }
        
        gems = 0;
        raceActive = true;
    }

    public void FinishRace()
    {
        if (raceActive)
        {
            raceActive = false;
            lastTime = Mathf.Floor(time * 1000) / 1000;

            if (lastTime < 16)
            {
                marathonRunCounter++;

                if (marathonRunCounter >= 3)
                {
                    AchievementManager.SetAchievement("achievement_04");
                }

                if (lastTime <= 14)
                {
                    AchievementManager.SetAchievement("achievement_03");
                }
            }
            else
            {
                marathonRunCounter = 0;
            }

            if (lastTime < bestTime)
            {
                bestTime = lastTime;

                //if (eventLeaderboard.RankUpgrade(currentRank))
                //{
                //    highscoreInfo.text = "New Rank!!!";
                //}
                //else
                //{
                //    highscoreInfo.text = "Personal Best!!!";
                //}

                highscoreInfo.text = "Personal Best!!!";


                if (SteamManager.Initialized)
                {
                    steamLeaderboard.UploadScore(bestTime);
                }
                else
                {
                    eventLeaderboard.SaveScore(new LocalScoreEntry(eventPlayerName, currentRank, bestTime, cachedEmail));
                }
            }
            else
            {
                highscoreInfo.text = "You can be faster!!!";
            }

            if (SteamManager.Initialized)
            {
                StartCoroutine(LoadSteamLeaderboardStats());
            }
            else
            {
                DisplayEventLeaderboardStats(eventLeaderboard.GetScoreRange(eventPlayerName));
            }

            finishBoard.Popup(finishTimeout);

            uiTimeText.text = $"{lastTime:#0.000}";

            if (gems <= 0)
            {
                AchievementManager.SetAchievement("achievement_02");

                if (player.collectedBoosters <= 0)
                {
                    AchievementManager.SetAchievement("achievement_05");
                }

                player.collectedBoosters = 0;
            }

            foreach (Gem coin in gemObjects)
            {
                coin.Respawn();
            }
        }
    }

    public void ResetRace()
    {
        if (!raceActive)
            return;

        time = 0;
        gems = 0;
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

        playerNameText.text = eventPlayerName;

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
            gems++;

            time -= amount * timePerGem;

            uiTimeText.text = $"{time:#0.000}";

            if (gems >= 30)
            {
                AchievementManager.SetAchievement("achievement_00");

                if (gems >= 60)
                {
                    AchievementManager.SetAchievement("achievement_01");
                }
            }
        }
    }
}
