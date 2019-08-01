using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
    [SerializeField] AudioSource colliderSource;

    private RaceController raceController;


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

    public void AddCoins(int amount)
    {
        raceController.AddGems(amount);
    }
}
