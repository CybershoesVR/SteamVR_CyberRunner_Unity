using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class LeaderboardManager : MonoBehaviour
{
    public int scoreMultiplier = 1000000000;
    public int leaderBoardEntryMax = 10;
    [HideInInspector]
    public ScoreEntry[] loadedScores;

    private SteamLeaderboard_t currentLeaderboard;
    private int leaderBoardEntryAmount;

    CallResult<LeaderboardFindResult_t> callResultFindLeaderBoard;
    CallResult<LeaderboardScoreUploaded_t> callResultUploadScore;
    CallResult<LeaderboardScoresDownloaded_t> callResultDownloadScore;


    private void OnEnable()
    {
        if (SteamManager.Initialized)
        {
            callResultFindLeaderBoard = CallResult<LeaderboardFindResult_t>.Create(OnFindLeaderboard);
            callResultUploadScore = CallResult<LeaderboardScoreUploaded_t>.Create(OnUploadScore);
            callResultDownloadScore = CallResult<LeaderboardScoresDownloaded_t>.Create(OnDownloadScore);
        }
    }

    public void FindLeaderboard(string leaderboardID)
    {
        currentLeaderboard = new SteamLeaderboard_t();

	    SteamAPICall_t steamAPICall = SteamUserStats.FindLeaderboard(leaderboardID);
        callResultFindLeaderBoard.Set(steamAPICall, OnFindLeaderboard);
    }

    public bool UploadScore(float score)
    {
        if (currentLeaderboard == null)
            return false;

        int compressedScore = Mathf.RoundToInt(1 / score * scoreMultiplier);
        Debug.Log($"Divided Score: {1 / score}, Multiplied Score: {compressedScore}");

        SteamAPICall_t steamAPICall = SteamUserStats.UploadLeaderboardScore(currentLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, compressedScore, null, 0);

        callResultUploadScore.Set(steamAPICall, OnUploadScore);

        return true;
    }

    public bool DownloadScores()
    {
        if (currentLeaderboard == null)
            return false;

        SteamAPICall_t steamAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, -(leaderBoardEntryMax/2)+1, (leaderBoardEntryMax / 2));
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
    }

    void OnDownloadScore(LeaderboardScoresDownloaded_t callback, bool ioFailure)
    {
        if (!ioFailure)
        {
            leaderBoardEntryAmount = Mathf.Min(callback.m_cEntryCount, leaderBoardEntryMax);

            Debug.Log("Downloaded " + leaderBoardEntryAmount + " Scores:");

            loadedScores = new ScoreEntry[leaderBoardEntryAmount];

            LeaderboardEntry_t currentEntry;

            for (int i = 0; i < leaderBoardEntryAmount; i++)
            {
                SteamUserStats.GetDownloadedLeaderboardEntry(callback.m_hSteamLeaderboardEntries, i, out currentEntry, null, 0);

                loadedScores[i] = new ScoreEntry(SteamFriends.GetFriendPersonaName(currentEntry.m_steamIDUser), currentEntry.m_nGlobalRank, 1 / ((float)currentEntry.m_nScore / scoreMultiplier));
            }
        }
        else
        {
            Debug.LogWarning("Failure to download scores");
        }
    }
}
