using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource upSound;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.instance.SetCheckpoint(point, anim))
            {
                anim.SetTrigger("Play");
                upSound.Play();
            }
     
        }
    }
}
