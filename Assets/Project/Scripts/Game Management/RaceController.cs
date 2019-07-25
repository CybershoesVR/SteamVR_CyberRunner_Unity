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

    [SerializeField] TextMeshProUGUI uiPlayerTimeText;
    [SerializeField] RectTransform scoreList;
    [SerializeField] GameObject scoreRowPrefab;
    [Space]
    [SerializeField] float timePerGem;
    [SerializeField] float finishTimeout = 1f;

    private FinishBoard finishBoard;
    private TextMeshProUGUI highscoreInfo;

    private bool raceActive = false;

    private float time = 0;
    private float bestTime = 1000000; //Default Value set really high so a first best time can be achieved
    private float lastTime = 0;

    private Gem[] gemObjects;

    private string scoreListPath;
    private FileStream stream;

    private LeaderboardManager leaderboardManager;

    #endregion


    private void Start()
    {
        leaderboardManager = FindObjectOfType<LeaderboardManager>();

        leaderboardManager.FindLeaderboard("BestTime");

        Invoke("LoadLeaderboardStats", 0.5f);
        LoadStats();

        //Only display if values are not default
        //if (bestTime != 1000000)
        //{
        //    bestTimeText.text = $"{bestTime:#0.000} s";
        //}

        gemObjects = FindObjectsOfType<Gem>();

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

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            leaderboardManager.DownloadScores();
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
            }
            else
            {
                highscoreInfo.text = "Too slow!!!";
            }

            leaderboardManager.UploadScore(lastTime);
            Invoke("LoadLeaderboardStats", 0.5f);

            finishBoard.Popup(finishTimeout);

            //Save Stats
            SaveStats();

            //Display Stats
            //bestTimeText.text = $"{bestTime:#0.000} s";

            //lastTimeText.text = $"{lastTime:#0.000} s";
            uiPlayerTimeText.text = $"{lastTime:#0.000}";

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
            uiPlayerTimeText.text = $"{lastTime:#0.000}";
        }
        else
        {
            uiPlayerTimeText.text = "0.000";
        }

        foreach (Gem gem in gemObjects)
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

        //lastTimeText.text = "-";
        uiPlayerTimeText.text = "0.000";
        //bestTimeText.text = "-";

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
    }

    void LoadLeaderboardStats()
    {
        leaderboardManager.DownloadScores();
        Invoke("DisplayLeaderboardStats", 1f);
    }

    void DisplayLeaderboardStats()
    {
        int childCount = scoreList.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Destroy(scoreList.GetChild(i).gameObject);
        }

        if (leaderboardManager.loadedScores == null)
        {
            Debug.LogError("LoadedScores empty!");
            return;
        }

        for (int i = 0; i < leaderboardManager.loadedScores.Length; i++)
        {
            GameObject newRow = Instantiate(scoreRowPrefab, scoreList);

            newRow.transform.Find("RankText").GetComponent<TextMeshProUGUI>().text = leaderboardManager.loadedScores[i].rank.ToString();
            newRow.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = leaderboardManager.loadedScores[i].playerName.ToString();
            newRow.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = $"{leaderboardManager.loadedScores[i].score:#0.000}";
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

            uiPlayerTimeText.text = $"{time:#0.000}";
        }
    }
}
