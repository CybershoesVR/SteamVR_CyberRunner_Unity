using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public static class AchievementManager
{
    private static bool unlocked = false;

    public static void SetAchievement(string id)
    {
        if (SteamManager.Initialized)
        {
            CheckAchievement(id);
            
            if (!unlocked)
            {
                Debug.Log($"Unlocking {id}...");
                SteamUserStats.SetAchievement(id);
                SteamUserStats.StoreStats();
            }
        }
    }

    public static void DEBUG_ClearAchievement(string id)
    {
        if (SteamManager.Initialized)
        {
            CheckAchievement(id);

            if (unlocked)
            {
                SteamUserStats.ClearAchievement(id);
            }
        }
    }

    static void CheckAchievement(string id)
    {
        SteamUserStats.GetAchievement(id, out unlocked);
    }
}
