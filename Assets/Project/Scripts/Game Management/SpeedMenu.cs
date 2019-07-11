using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpeedMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI speedDisplayText;
    [SerializeField] Button[] addButtons;
    [SerializeField] Button[] subButtons;

    private PlayerMovement player;


    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
    }

    public void AddSpeed(float amount)
    {
        float newSpeed = player.AddSpeed(amount);

        speedDisplayText.text = newSpeed.ToString();

        if (newSpeed == player.minSpeed)
        {
            foreach (Button button in addButtons)
            {
                button.interactable = true;
            }

            foreach (Button button in subButtons)
            {
                button.interactable = false;
            }
        }
        else if (newSpeed == player.maxSpeed)
        {
            foreach (Button button in addButtons)
            {
                button.interactable = false;
            }

            foreach (Button button in subButtons)
            {
                button.interactable = true;
            }
        }
        else
        {
            foreach (Button button in addButtons)
            {
                button.interactable = true;
            }

            foreach (Button button in subButtons)
            {
                button.interactable = true;
            }
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speedDisplayText.text = player.SetSpeed(newSpeed).ToString();

        foreach (Button button in addButtons)
        {
            button.interactable = true;
        }

        foreach (Button button in subButtons)
        {
            button.interactable = true;
        }
    }
}
