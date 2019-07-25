using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class AchievementManager : MonoBehaviour
{
    private bool unlocked = false;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SetAchievement("achievement_00");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            DEBUG_ClearAchievement("achievement_00");
        }
    }

    public void SetAchievement(string id)
    {
        if (SteamManager.Initialized)
        {
            CheckAchievement(id);
            
            if (!unlocked)
            {
                Debug.Log("Unlocking...");
                SteamUserStats.SetAchievement(id);
                SteamUserStats.StoreStats();
            }
        }
    }

    public void DEBUG_ClearAchievement(string id)
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

    void CheckAchievement(string id)
    {
        SteamUserStats.GetAchievement(id, out unlocked);
    }
}
