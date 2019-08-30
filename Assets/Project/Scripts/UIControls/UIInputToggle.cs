using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class UIInputToggle : MonoBehaviour
{
    [SerializeField] SteamVR_Action_Boolean openMenuAction;
    [SerializeField] SteamVR_Input_Sources menuHand = SteamVR_Input_Sources.LeftHand;
    [SerializeField] GameObject menuObject;
    [Space]
    [SerializeField] UILaserpointer laserPointerLeft;
    [SerializeField] UILaserpointer laserPointerRight;
    [SerializeField] SteamVR_Action_Boolean clickAction;
    [Space]
    [SerializeField] float pointerMoveSpeedTreshold = 3; //The speed you can move at before the laserpointer will deactivate

    private Rigidbody playerRB;
    private UILaserpointer activeHand;
    private bool pointerActive = true;
    private bool menuPressed = false;


    private void Start()
    {
        activeHand = laserPointerRight;
        activeHand.ToggleLaser(true);
        playerRB = transform.root.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (openMenuAction.GetState(menuHand))
        {
            if (!menuPressed)
            {
                menuPressed = true;
                menuObject.SetActive(!menuObject.activeSelf);

                if (menuObject.activeSelf && activeHand == laserPointerLeft && pointerActive)
                {
                    activeHand.ToggleLaser(false);
                    pointerActive = false;
                }
            }
        }
        else if (menuPressed)
        {
            menuPressed = false;
        }

        if (pointerActive && playerRB.velocity.magnitude > pointerMoveSpeedTreshold)
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

            if (!pointerActive && !menuObject.activeSelf)
            {
                activeHand.ToggleLaser(true);
                pointerActive = true;
            }
        }
    }
}
