using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGamePoint : MonoBehaviour
{

    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource upSound;

    bool isActive = false;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive)
            return;

        if (other.CompareTag("Player"))
        {
            anim.SetTrigger("Play");
            upSound.Play();
            GameManager.instance.StopTimer();
            UIController.instance.ShowEndGame();
            isActive = true;
        }
    }
}
