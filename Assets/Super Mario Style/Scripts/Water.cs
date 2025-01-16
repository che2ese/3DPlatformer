using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{

    [SerializeField] private GameObject enterEffect;

    [SerializeField] private GameObject moveEffect;

    private ParticleSystem currentMoveEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var clone = Instantiate(enterEffect, new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z), enterEffect.transform.rotation);
            Destroy(clone,3);            
            if (!currentMoveEffect)
            {
                currentMoveEffect = Instantiate(moveEffect, new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z), Quaternion.identity).GetComponent<ParticleSystem>();
                currentMoveEffect.gameObject.transform.parent = other.gameObject.transform;
            }

            var em = currentMoveEffect.emission;
            em.enabled = false;
            currentMoveEffect.transform.position = new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentMoveEffect)
            {
                currentMoveEffect.transform.position = new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z);
                var em = currentMoveEffect.emission;
                em.enabled = true;
              
            }
   
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentMoveEffect)
            {
                var em = currentMoveEffect.emission;
                em.enabled = false;
            }

        }
    }
}
