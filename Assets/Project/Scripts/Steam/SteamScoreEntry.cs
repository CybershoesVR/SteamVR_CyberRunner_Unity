using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public struct SteamScoreEntry
{
    public CSteamID playerID;
    public int rank;
    public float score;

    public SteamScoreEntry(CSteamID playerID, int rank, float score)
    {
        this.playerID = playerID;
        this.rank = rank;
        this.score = score;
    }
}
