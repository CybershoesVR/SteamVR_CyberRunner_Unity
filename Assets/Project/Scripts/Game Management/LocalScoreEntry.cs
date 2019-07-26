using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LocalScoreEntry
{
    public string playerName;
    public int rank;
    public float score;

    public LocalScoreEntry(string playerName, int rank, float score)
    {
        this.playerName = playerName;
        this.rank = rank;
        this.score = score;
    }
}
