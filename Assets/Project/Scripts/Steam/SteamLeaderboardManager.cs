using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamLeaderboardManager : MonoBehaviour
{
    public int scoreMultiplier = 1000000000;

    [HideInInspector]
    public SteamScoreEntry[] loadedScores;
    [HideInInspector]
    public bool downloadInProgress = false;
    [HideInInspector]
    public bool uploadInProgress = false;

    private SteamLeaderboard_t currentLeaderboard;

    protected int leaderBoardEntryMax = 6;

    private CallResult<LeaderboardFindResult_t> callResultFindLeaderBoard;
    private CallResult<LeaderboardScoreUploaded_t> callResultUploadScore;
    private CallResult<LeaderboardScoresDownloaded_t> callResultDownloadScore;


    private void OnEnable()
    {
        if (SteamManager.Initialized)
        {
            callResultFindLeaderBoard = CallResult<LeaderboardFindResult_t>.Create(OnFindLeaderboard);
            callResultUploadScore = CallResult<LeaderboardScoreUploaded_t>.Create(OnUploadScore);
            callResultDownloadScore = CallResult<LeaderboardScoresDownloaded_t>.Create(OnDownloadScore);
        }
    }

    public void AssignEntryMax(int entryMax)
    {
        leaderBoardEntryMax = entryMax;
    }

    public void FindLeaderboard(string leaderboardID)
    {
        downloadInProgress = true;

	    SteamAPICall_t steamAPICall = SteamUserStats.FindLeaderboard(leaderboardID);
        callResultFindLeaderBoard.Set(steamAPICall, OnFindLeaderboard);
    }

    public bool UploadScore(float score)
    {
        if (currentLeaderboard == null)
            return false;

        uploadInProgress = true;

        int compressedScore = Mathf.RoundToInt(1 / score * scoreMultiplier);
        Debug.Log($"Compressed Score: {compressedScore}");

        SteamAPICall_t steamAPICall = SteamUserStats.UploadLeaderboardScore(currentLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, compressedScore, null, 0);

        callResultUploadScore.Set(steamAPICall, OnUploadScore);

        return true;
    }

    public bool DownloadScores(bool newPlayer=false)
    {
        if (currentLeaderboard == null)
            return false;

        downloadInProgress = true;
        loadedScores = null;

        SteamAPICall_t steamAPICall;

        if (!newPlayer)
        {
            steamAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -leaderBoardEntryMax/2, (leaderBoardEntryMax/2)-1);
        }
        else
        {
            steamAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, leaderBoardEntryMax);
        }

        callResultDownloadScore.Set(steamAPICall, OnDownloadScore);

        return true;
    }

    void OnFindLeaderboard(LeaderboardFindResult_t callback, bool ioFailure)
    {
        if (callback.m_bLeaderboardFound == 0 || ioFailure)
        {
            Debug.LogError("Leaderboard could not be found!");
            return;
        }
        else
        {
            Debug.Log("Leaderboard found: " + callback.m_hSteamLeaderboard);
        }

        currentLeaderboard = callback.m_hSteamLeaderboard;
        downloadInProgress = false;
    }

    void OnUploadScore(LeaderboardScoreUploaded_t callback, bool ioFailure)
    {
        if (callback.m_bSuccess == 0 || ioFailure)
        {
            Debug.LogError("Score could not be uploaded to Steam!");
        }
        else
        {
            Debug.Log("Uploaded Score!");
        }

        uploadInProgress = false;
    }

    void OnDownloadScore(LeaderboardScoresDownloaded_t callback, bool ioFailure)
    {
        if (!ioFailure)
        {
            int leaderBoardEntryAmount = Mathf.Min(callback.m_cEntryCount, leaderBoardEntryMax);

            Debug.Log("Downloaded " + leaderBoardEntryAmount + " Scores:");

            if (leaderBoardEntryAmount <= 0)
            {
                Debug.Log("Retrying Download as user without score...");
                DownloadScores(true);
                return;
            }

            loadedScores = new SteamScoreEntry[leaderBoardEntryAmount];

            LeaderboardEntry_t currentEntry;

            for (int i = 0; i < leaderBoardEntryAmount; i++)
            {
                SteamUserStats.GetDownloadedLeaderboardEntry(callback.m_hSteamLeaderboardEntries, i, out currentEntry, null, 0);

                loadedScores[i] = new SteamScoreEntry(currentEntry.m_steamIDUser, currentEntry.m_nGlobalRank, 1 / ((float)currentEntry.m_nScore / scoreMultiplier));
            }
        }
        else
        {
            Debug.LogWarning("Failure to download scores");
        }

        downloadInProgress = false;
    }
}
