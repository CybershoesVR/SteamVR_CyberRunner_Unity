using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
    [SerializeField] AudioSource colliderSource;
    [SerializeField] int gemMaxPitch = 10;
    [SerializeField] float recentGemTime = 1;

    [SerializeField] SteamVR_Action_Boolean resetAction;
    [SerializeField] SteamVR_Input_Sources resetSource;

    private RaceController raceController;
    private Teleporter teleporter;

    private int recentGems = 0;


    void Start()
    {
        raceController = FindObjectOfType<RaceController>();
        teleporter = FindObjectOfType<Teleporter>();
    }

    private void Update()
    {
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

        if (resetAction.GetState(resetSource))
        {
            teleporter.Teleport();
            Invoke("ResetRace", teleporter.fadeDuration);
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.collider.CompareTag("Wall"))
    //    {
    //        colliderSource.Play();
    //    }
    //}

    void ResetRace()
    {
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

        return recentGems;
    }

    void ResetRecentGems()
    {
        recentGems = 0;
    }
}
