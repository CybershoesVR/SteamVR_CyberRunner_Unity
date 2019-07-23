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
