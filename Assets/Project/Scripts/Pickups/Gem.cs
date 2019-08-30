using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    [SerializeField] int value = 1;

    private MeshRenderer meshRenderer;
    private SphereCollider gemCollider;
    private ParticleSystem collectParticles;
    private AudioSource source;


    void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        gemCollider = GetComponent<SphereCollider>();
        collectParticles = GetComponentInChildren<ParticleSystem>();
        source = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.transform.root.GetComponent<PlayerController>();

        if (player)
        {
            float pitchOffset = player.AddGems(value); //Give Gem to player & find collect pitch
            source.pitch = 1 + (pitchOffset * 0.05f); //Set pitch
            source.Play();

            gemCollider.enabled = false;
            meshRenderer.enabled = false;
            collectParticles.Play();
        }
    }

    public void Respawn()
    {
        gemCollider.enabled = true;
        meshRenderer.enabled = true;
    }
}
