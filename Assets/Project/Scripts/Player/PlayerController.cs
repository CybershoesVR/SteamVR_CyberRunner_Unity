using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
    private RaceController raceController;
    private AudioSource source;


    void Start()
    {
        raceController = FindObjectOfType<RaceController>();
        source = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            source.Play();
        }
    }

    public void AddCoins(int amount)
    {
        raceController.AddCoins(amount);
    }
}
