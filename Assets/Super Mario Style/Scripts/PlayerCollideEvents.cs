using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PlayerCollideEvents : MonoBehaviour
{
    public UnityEvent onEnter;
    public UnityEvent onExit;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            onEnter.Invoke();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            onExit.Invoke();    
        }
    }

    public void InstantiateEffect(GameObject effect)
    {
        var clone = Instantiate(effect,transform.position,transform.rotation);
        Destroy(clone, 3);
    }
}
