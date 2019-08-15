using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpeedMenu : MonoBehaviour
{
    //[SerializeField] TextMeshProUGUI speedDisplayText;
    //[SerializeField] Button[] addButtons;
    //[SerializeField] Button[] subButtons;
    //[SerializeField] Animator normalAnim;
    //[SerializeField] Animator slowAnim;
    //[SerializeField] Animator fastAnim;

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

    public void AddSpeed(float amount)
    {
        float newSpeed = player.AddSpeed(amount);

        //speedDisplayText.text = newSpeed.ToString();

        if (newSpeed == player.minSpeed)
        {
            normalSelection.Stop();
            fastSelection.Stop();

            slowSelection.Play();
            slowSource.Play();
        }
        else if (newSpeed == player.maxSpeed)
        {
            normalSelection.Stop();
            slowSelection.Stop();

            fastSelection.Play();
            fastSource.Play();
        }
        else
        {
            fastSelection.Stop();
            slowSelection.Stop();

            normalSelection.Play();
            normalSource.Play();
        }
    }

    public void SetSpeed(float newSpeed)
    {
        player.SetSpeed(newSpeed);

        if (newSpeed == player.minSpeed)
        {
            normalSelection.Stop();
            fastSelection.Stop();

            slowSelection.Play();
            slowSource.Play();
        }
        else if (newSpeed == player.maxSpeed)
        {
            normalSelection.Stop();
            slowSelection.Stop();

            fastSelection.Play();
            fastSource.Play();
        }
        else
        {
            fastSelection.Stop();
            slowSelection.Stop();

            normalSelection.Play();
            normalSource.Play();
        }
    }
}
