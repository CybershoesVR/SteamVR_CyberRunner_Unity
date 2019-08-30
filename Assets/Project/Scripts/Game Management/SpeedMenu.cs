using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpeedMenu : MonoBehaviour
{
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;

    [SerializeField] ParticleSystem normalSelection;
    [SerializeField] ParticleSystem slowSelection;
    [SerializeField] ParticleSystem fastSelection;

    private AudioSource normalSource;
    private AudioSource slowSource;
    private AudioSource fastSource;

    private PlayerMovement player;


    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();

        normalSource = normalSelection.GetComponentInParent<AudioSource>();
        slowSource = slowSelection.GetComponentInParent<AudioSource>();
        fastSource = fastSelection.GetComponentInParent<AudioSource>();
    }

    public void SetSpeed(float newSpeed)
    {
        player.SetSpeed(newSpeed);

        //Select Slow
        if (newSpeed == minSpeed)
        {
            normalSelection.Stop();
            fastSelection.Stop();

            slowSelection.Play();
            slowSource.Play();
        }
        //Select Fast
        else if (newSpeed == maxSpeed)
        {
            normalSelection.Stop();
            slowSelection.Stop();

            fastSelection.Play();
            fastSource.Play();
        }
        //Select Normal
        else
        {
            fastSelection.Stop();
            slowSelection.Stop();

            normalSelection.Play();
            normalSource.Play();
        }
    }
}
