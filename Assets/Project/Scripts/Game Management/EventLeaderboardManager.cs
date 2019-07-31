using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EventLeaderboardManager : MonoBehaviour
{
    public int scoreMultiplier = 1000000000;
    public string EventName = "Gamescom2019";

    [HideInInspector]
    public List<LocalScoreEntry> loadedScores;

    [HideInInspector]
    public string scoreListPath { get; private set; }
    private FileStream stream;

    private int leaderboardEntryMax;


    private void OnDisable()
    {
        ClearEmptyPlayers();
    }

    public void AssignEntryMax(int entryMax)
    {
        leaderboardEntryMax = entryMax;
    }

    public void SaveScore(LocalScoreEntry newScore)
    {
        if (loadedScores != null)
        {
            int index = loadedScores.FindIndex(0, x => x.playerName == newScore.playerName);

            if (index == -1)
            {
                loadedScores.Add(newScore);
            }
            else
            {
                loadedScores[index] = newScore;
            }
        }
        else
        {
            loadedScores = new List<LocalScoreEntry>();
            loadedScores.Add(newScore);
        }

        UpdateScores();

        string fileText = EventName + "\n";

        for (int i = 0; i < loadedScores.Count; i++)
        {
            fileText += $"{loadedScores[i].rank}\t{loadedScores[i].playerName}\t{loadedScores[i].score}\t{loadedScores[i].email}\n";
        }

        File.WriteAllText(scoreListPath, fileText);
    }

    public void LoadScoreList()
    {
        scoreListPath = Application.dataPath + "/Saves/";

        if (!Directory.Exists(scoreListPath))
        {
            Directory.CreateDirectory(scoreListPath);
        }

        scoreListPath += $"{EventName}_ScoreList.txt";

        if (File.Exists(scoreListPath))
        {
            //stream = new FileStream(scoreListPath, FileMode.Open);
            //BinaryReader reader = new BinaryReader(stream);

            loadedScores = new List<LocalScoreEntry>();

            //int count = reader.ReadInt32();
            //Debug.Log("COUNT: " + count);

            //for (int i = 0; i < count; i++)
            //{
            //    string name = reader.ReadString();
            //    int rank = reader.ReadInt32();
            //    float time = reader.ReadSingle();

            //    loadedScores.Add(new LocalScoreEntry(name, rank, time));
            //}

            //reader.Close();

            string[] fileLines = File.ReadAllLines(scoreListPath);

            for (int i = 1; i < fileLines.Length; i++)
            {
                string[] lineChunks = fileLines[i].Split('\t');

                //Rank is update right after, so there is no need to read it out
                loadedScores.Add(new LocalScoreEntry(lineChunks[1], 0, float.Parse(lineChunks[2]), lineChunks[3]));
            }

            if (loadedScores.Count > 1)
            {
                UpdateScores();
            }
        }
        else
        {
            File.Create(scoreListPath);
            loadedScores = null;
        }
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
                UnityEngine.Debug.Log("FOUND PLAYER");

                for (int j = (i - rangeStart); j <= (i + rangeEnd); j++)
                {
                    if (j < loadedScores.Count && j >= 0)
                    {
                        UnityEngine.Debug.Log("SCORE APPROPRIATE");
                        scoreRange.Add(loadedScores[j]);
                    }
                }
                nameFound = true;
                break;
            }
        }

        if (!nameFound)
        {
            UnityEngine.Debug.Log("NO PLAYER STATS FOUND");

            for (int i = 0; i < leaderboardEntryAmount; i++)
            {
                scoreRange.Add(loadedScores[i]);
            }
        }

        return scoreRange;
    }

    public void ClearEmptyPlayers()
    {
        if (loadedScores != null && loadedScores.Count > 0)
        {
            for (int i = loadedScores.Count-1; i >= 0; i--)
            {
                if (loadedScores[i].score > 100000)
                {
                    loadedScores.RemoveAt(loadedScores.Count - 1);
                }
            }

            string fileText = EventName + "\n";

            for (int i = 0; i < loadedScores.Count; i++)
            {
                fileText += $"{loadedScores[i].rank}\t{loadedScores[i].playerName}\t{loadedScores[i].score}\t{loadedScores[i].email}\n";
            }

            File.WriteAllText(scoreListPath, fileText);
        }
    }

    public LocalScoreEntry InitPlayer()
    {
        LocalScoreEntry addedPlayerScore = loadedScores[loadedScores.Count - 1];

        loadedScores.RemoveAt(loadedScores.Count - 1);

        if (loadedScores.Count > 0)
        {
            int index = loadedScores.FindIndex(0, x => x.playerName == addedPlayerScore.playerName);

            if (index == -1)
            {
                loadedScores.Add(addedPlayerScore);
            }
            else
            {
                loadedScores[index] = new LocalScoreEntry(loadedScores[index].playerName, loadedScores[index].rank, loadedScores[index].score, addedPlayerScore.email);

                string fileText = EventName + "\n";

                for (int i = 0; i < loadedScores.Count; i++)
                {
                    fileText += $"{loadedScores[i].rank}\t{loadedScores[i].playerName}\t{loadedScores[i].score}\t{loadedScores[i].email}\n";
                }

                File.WriteAllText(scoreListPath, fileText);

                return loadedScores[index];
            }
        }
        else
        {
            loadedScores.Add(addedPlayerScore);
        }

        return addedPlayerScore;
    }

    void UpdateScores()
    {
        loadedScores.Sort(delegate (LocalScoreEntry x, LocalScoreEntry y)
        {
            return x.score.CompareTo(y.score);
        });

        for (int i = 0; i < loadedScores.Count; i++)
        {
            loadedScores[i] = new LocalScoreEntry(loadedScores[i].playerName, i + 1, loadedScores[i].score, loadedScores[i].email);
        }
    }
}
