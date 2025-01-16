using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] 
    private int value = 1;
    [SerializeField] 
    private bool destroy;
    [SerializeField]
    private GameObject pickUpEffect;
    [SerializeField]
    private bool isRed;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.instance)
            {
                GameManager.instance.AddCoin(value);

                if (isRed)
                    GameManager.instance.AddRedCoin();
            }
       
            if (pickUpEffect)
                Instantiate(pickUpEffect, transform.position, transform.rotation);

            if (destroy)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
