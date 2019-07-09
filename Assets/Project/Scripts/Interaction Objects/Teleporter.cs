using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Teleporter : MonoBehaviour
{
    [SerializeField]
    Transform spawnPoint;
    [SerializeField]
    Animator effectCanvas;
    [SerializeField]
    float darkTime = 2;

    private Transform player;


    void Start()
    {
        player = FindObjectOfType<PlayerController>().transform;
    }

    public void Teleport()
    {
        effectCanvas.SetTrigger("Fade");
        StartCoroutine(TpAfterFade());
    }

    IEnumerator TpAfterFade()
    {
        yield return new WaitForSeconds(darkTime);

        player.position = spawnPoint.position;

        effectCanvas.SetTrigger("Fade");
    }
}
