using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFreezer : MonoBehaviour
{
    private Rigidbody playerRB;


    void Start()
    {
        playerRB = FindObjectOfType<PlayerController>().GetComponent<Rigidbody>();
    }

    public void ToggleFreeze(bool frozen)
    {
        if (frozen)
        {
            playerRB.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            playerRB.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}
