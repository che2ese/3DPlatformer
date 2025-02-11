using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour
{
    public GameObject hitEffect;
    public GameObject breakEffect;

    private bool destroy;

    private void OnCollisionEnter(Collision collision)
    {
        // 플레이어 태그와 충돌 시
        if (collision.gameObject.CompareTag("Player"))
        {
            Break();
        }
    }

    private void Break()
    {
        if (hitEffect)
        {
            Instantiate(hitEffect, transform.position, transform.rotation);
        }

        if (breakEffect)
        {
            Instantiate(breakEffect, transform.position, transform.rotation);
        }

        if (destroy)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Repair()
    {
        gameObject.SetActive(true);
    }
}
