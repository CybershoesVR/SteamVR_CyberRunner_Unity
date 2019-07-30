using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LocalScoreEntry
{
    public string playerName;
    public int rank;
    public float score;
    public string email;

    public LocalScoreEntry(string playerName, int rank, float score, string email = "[No Email]")
    {
        this.playerName = playerName;
        this.rank = rank;
        this.score = score;
        this.email = email;
    }
}
