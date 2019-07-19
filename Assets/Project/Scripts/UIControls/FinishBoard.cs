using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishBoard : MonoBehaviour
{
    private Animator anim;
    private Teleporter teleporter;


    private void Start()
    {
        anim = GetComponent<Animator>();
        teleporter = GetComponent<Teleporter>();
    }

    public void Popup(float delay)
    {
        anim.SetTrigger("Toggle");
        Invoke("Hide", delay);
    }

    public void Hide()
    {
        anim.SetTrigger("Toggle");
        teleporter.Teleport();
    }
}
