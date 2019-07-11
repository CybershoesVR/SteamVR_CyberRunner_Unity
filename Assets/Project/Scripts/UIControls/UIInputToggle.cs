using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class UIInputToggle : MonoBehaviour
{
    [SerializeField] UILaserpointer laserPointerLeft;
    [SerializeField] UILaserpointer laserPointerRight;

    [SerializeField] SteamVR_Action_Boolean clickAction;

    private Rigidbody playerRB;
    private UILaserpointer activeHand;
    private bool pointerActive = true;


    private void Start()
    {
        activeHand = laserPointerRight;
        activeHand.ToggleLaser(true);
        playerRB = transform.root.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (pointerActive && playerRB.velocity.magnitude > 3)
        {
            activeHand.ToggleLaser(false);
            pointerActive = false;
        }

        if (clickAction.GetState(SteamVR_Input_Sources.RightHand))
        {
            if (activeHand == laserPointerLeft)
            {
                activeHand.ToggleLaser(false);
                pointerActive = false;
                activeHand = laserPointerRight;
            }

            if (!pointerActive)
            {
                activeHand.ToggleLaser(true);
                pointerActive = true;
            }
        }
        else if (clickAction.GetState(SteamVR_Input_Sources.LeftHand))
        {
            if (activeHand == laserPointerRight)
            {
                activeHand.ToggleLaser(false);
                pointerActive = false;
                activeHand = laserPointerLeft;
            }

            if (!pointerActive)
            {
                activeHand.ToggleLaser(true);
                pointerActive = true;
            }
        }
    }
}
