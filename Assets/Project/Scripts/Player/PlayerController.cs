using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
    [SerializeField] AudioSource colliderSource;
    [SerializeField] int gemMaxPitch = 10;
    [SerializeField] float recentGemTime = 1;

    private RaceController raceController;

    private int recentGems = 0;


    void Start()
    {
        raceController = FindObjectOfType<RaceController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            AchievementManager.DEBUG_ClearAchievement("achievement_00");
            AchievementManager.DEBUG_ClearAchievement("achievement_01");
            AchievementManager.DEBUG_ClearAchievement("achievement_02");
            AchievementManager.DEBUG_ClearAchievement("achievement_03");
            AchievementManager.DEBUG_ClearAchievement("achievement_04");
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.collider.CompareTag("Wall"))
    //    {
    //        colliderSource.Play();
    //    }
    //}

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
