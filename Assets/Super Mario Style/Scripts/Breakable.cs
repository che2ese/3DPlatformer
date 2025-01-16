using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Breakable : MonoBehaviour
{
    public GameObject hitEffect;
    public GameObject breakEffect;

    public int health = 1;
    public bool destroy;

    [Space(10)]
    public UnityEvent onHit;
    public UnityEvent onBreak;

    public bool isGolden;

    private int currentHealth;

    private void Start()
    {
        currentHealth = health;
    }

    public void Hit()
    {
        if (hitEffect)
        {
            Instantiate(hitEffect, transform.position, transform.rotation);
        }

        currentHealth -= 1;
        onHit.Invoke();

        if (currentHealth <= 0)
        {
            Break();
        }
    }

    private void Break()
    {
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

        if (isGolden)
        {
            GameManager.instance.AddGolden();
        }
        else
        {
            GameManager.instance.AddBlock();
        }

        onBreak.Invoke();
    }

    private void Repair()
    {
        currentHealth = health;
        gameObject.SetActive(true);
    }

    public void AddCoin(int amount)
    {
        GameManager.instance.AddCoin(amount);
    }
}
