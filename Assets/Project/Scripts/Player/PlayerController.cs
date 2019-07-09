using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
    //[SerializeField] float maxClickDistance;
    //[SerializeField] LayerMask clickMask;

    //[SerializeField] SteamVR_Action_Boolean triggerAction;

    //[SerializeField] Transform leftHand;
    //[SerializeField] Transform rightHand;

    //[SerializeField] LineRenderer cursorLine;

    //private SteamVR_Input_Sources leftHandSource = SteamVR_Input_Sources.LeftHand;
    //private SteamVR_Input_Sources rightHandSource = SteamVR_Input_Sources.RightHand;

    //private Transform activeHand;
    private TimeRaceController raceController;

    //private bool handsEnabled = false;


    void Start()
    {
        //activeHand = rightHand;
        raceController = FindObjectOfType<TimeRaceController>();
    }

    //void Update()
    //{
    //    if (!handsEnabled)
    //    {
    //        return;
    //    }

    //    if (triggerAction.GetState(leftHandSource))
    //    {
    //        if (activeHand == rightHand)
    //        {
    //            activeHand = leftHand;
    //        }

    //        UIClick();
    //    }
    //    else if (triggerAction.GetState(rightHandSource))
    //    {
    //        if (activeHand == leftHand)
    //        {
    //            activeHand = rightHand;
    //        }

    //        UIClick();
    //    }

    //    cursorLine.SetPosition(0, activeHand.position);
    //    cursorLine.SetPosition(1, activeHand.forward * maxClickDistance);
    //}

    //private void UIClick()
    //{
    //    Ray ray = new Ray(activeHand.position, activeHand.forward);
    //    RaycastHit hit;

    //    if (Physics.Raycast(ray, out hit, maxClickDistance, clickMask))
    //    {
    //        ButtonController button = hit.collider.GetComponent<ButtonController>();

    //        if (button != null)
    //        {
    //            Debug.Log("BUTTON CLICKED!!!");
    //            button.TriggerButton();
    //        }
    //    }
    //}

    //public void SetUIInteraction(bool isActive)
    //{
    //    handsEnabled = isActive;
    //    cursorLine.enabled = isActive;
    //}

    public void AddCoins(int amount)
    {
        raceController.AddCoins(amount);
    }
}
