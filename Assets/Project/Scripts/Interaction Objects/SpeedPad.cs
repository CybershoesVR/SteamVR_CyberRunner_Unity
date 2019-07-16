using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPad : MonoBehaviour
{
    //Boost is always in the forward direction of the pad
    [SerializeField] float speedMultiplier = 2;
    [SerializeField] float fadeTime = 2;

    private AudioSource source;


    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.transform.root.GetComponent<PlayerMovement>();

        if (player)
        {
            player.SpeedBoost(transform.forward * speedMultiplier, fadeTime);
            source.Play();
        }
    }
}
