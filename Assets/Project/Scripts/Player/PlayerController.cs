using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
    [SerializeField] int gemMaxPitch = 10; //The highest Pitch for the Gem Collection sound before it resets
    [SerializeField] float recentGemTime = 1; //The time-limit to get a higher pitch when collecting a Gem

    [SerializeField] SteamVR_Action_Boolean resetAction; //Action for Race Reset
    [SerializeField] SteamVR_Input_Sources resetSource; //Hand doing the Reset

    private RaceController raceController;
    private Teleporter teleporter;

    private int recentGems = 0; //Gems collected in the current time limit


    void Start()
    {
        //Find References
        raceController = FindObjectOfType<RaceController>();
        teleporter = FindObjectOfType<Teleporter>();
    }

    private void Update()
    {
        //On Press, Reset Race
        if (resetAction.GetState(resetSource))
        {
            StartCoroutine(ResetRace());
        }

        //IN EDITOR press DEL to reset all Achievements for the current Steam Account (Only when Steam is enabled)
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Delete))
        {
            AchievementManager.DEBUG_ClearAchievement("achievement_00");
            AchievementManager.DEBUG_ClearAchievement("achievement_01");
            AchievementManager.DEBUG_ClearAchievement("achievement_02");
            AchievementManager.DEBUG_ClearAchievement("achievement_03");
            AchievementManager.DEBUG_ClearAchievement("achievement_04");
            AchievementManager.DEBUG_ClearAchievement("achievement_05");
            AchievementManager.DEBUG_ClearAchievement("achievement_06");
            Debug.Log("Achievements Cleared");
        }
    }

    IEnumerator ResetRace()
    {
        teleporter.Teleport();
        yield return new WaitForSeconds(teleporter.fadeDuration);
        raceController.ResetRace();
    }

    public int AddGems(int amount)
    {
        raceController.AddGems(amount);


        if (recentGems > gemMaxPitch)
        {
            recentGems = 0;
            CancelInvoke("ResetRecentGems");
        }
        else
        {
            recentGems++;
            CancelInvoke("ResetRecentGems");
            Invoke("ResetRecentGems", recentGemTime);
        }

        return recentGems; //The recentGems determine the pitch of the gem collection sound.
    }

    void ResetRecentGems()
    {
        recentGems = 0;
    }
}
