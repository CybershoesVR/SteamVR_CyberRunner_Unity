using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EventLeaderboardManager : MonoBehaviour
{
    public string EventName = "Gamescom2019";

    [HideInInspector]
    public List<LocalScoreEntry> loadedScores;

    private string scoreListPath;
    private FileStream stream;

    private int leaderboardEntryMax;


    public void AssignEntryMax(int entryMax)
    {
        leaderboardEntryMax = entryMax;
    }

    public void SaveScore(LocalScoreEntry newScore)
    {
        if (loadedScores != null)
        {
            for (int i = 0; i < loadedScores.Count; i++)
            {
                if (loadedScores[i].playerName == newScore.playerName)
                {
                    Debug.Log($"Removing {loadedScores[i].playerName}...");
                    loadedScores.RemoveAt(i);
                }
            }

            loadedScores.Insert(newScore.rank - 1, newScore);
        }
        else
        {
            loadedScores = new List<LocalScoreEntry>();
            loadedScores.Add(newScore);
        }

        stream = new FileStream(scoreListPath, FileMode.Create);

        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(loadedScores.Count); //COUNT

        for (int i = 0; i < loadedScores.Count; i++)
        {
            writer.Write(loadedScores[i].playerName);
            writer.Write(i+1);
            writer.Write(loadedScores[i].score);
        }

        stream.Flush();
        writer.Close();
    }

    public void LoadScoreList()
    {
        scoreListPath = Application.dataPath + "/Saves/";

        if (!Directory.Exists(scoreListPath))
        {
            Directory.CreateDirectory(scoreListPath);
        }

        scoreListPath += $"{EventName}_ScoreList.cr";

        if (File.Exists(scoreListPath))
        {
            stream = new FileStream(scoreListPath, FileMode.Open);

            BinaryReader reader = new BinaryReader(stream);

            loadedScores = new List<LocalScoreEntry>();

            int count = reader.ReadInt32();
            Debug.Log("COUNT: " + count);

            for (int i = 0; i < count; i++)
            {
                string name = reader.ReadString();
                int rank = reader.ReadInt32();
                float time = reader.ReadSingle();

                loadedScores.Add(new LocalScoreEntry(name, rank, time));
            }

            reader.Close();
        }
        else
        {
            loadedScores = null;
        }
    }

    public int GetPlayerRank(float score)
    {
        if (loadedScores == null)
        {
            return 1;
        }

        for (int i = loadedScores.Count-1; i >= 0 ; i--)
        {
            if (score < loadedScores[i].score)
            {
                return i+1;
            }
        }

        return loadedScores.Count;
    }

    public List<LocalScoreEntry> GetScoreRange(string playerName)
    {
        if (loadedScores == null)
        {
            return null;
        }

        List<LocalScoreEntry> scoreRange = new List<LocalScoreEntry>();

        int leaderboardEntryAmount = Mathf.Min(loadedScores.Count, leaderboardEntryMax);

        int rangeStart = (leaderboardEntryMax / 2) - 1;
        int rangeEnd = leaderboardEntryMax / 2;

        bool nameFound = false;

        for (int i = 0; i < loadedScores.Count; i++)
        {
            if (loadedScores[i].playerName == playerName)
            {
                Debug.Log("FOUND PLAYER");

                for (int j = (i - rangeStart); j <= (i + rangeEnd); j++)
                {
                    if (j < loadedScores.Count && j >= 0)
                    {
                        Debug.Log("SCORE APPROPRIATE");
                        scoreRange.Add(loadedScores[j]);
                    }
                }
                nameFound = true;
                break;
            }
        }

        if (!nameFound)
        {
            Debug.Log("NO PLAYER STATS FOUND");

            for (int i = 0; i < leaderboardEntryAmount; i++)
            {
                scoreRange.Add(loadedScores[i]);
            }
        }

        return scoreRange;
    }
}
