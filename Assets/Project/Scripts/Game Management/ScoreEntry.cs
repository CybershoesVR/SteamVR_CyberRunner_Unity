using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScoreEntry
{
    public string playerName;
    public int rank;
    public float score;

    public ScoreEntry(string playerName, int rank, float score)
    {
        this.playerName = playerName;
        this.rank = rank;
        this.score = score;
    }
}
