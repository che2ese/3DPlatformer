using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public Vector3 direction;

    [SerializeField]
    private GameObject pickUpEffect;
    [SerializeField]
    private bool isRed;

    private void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }
    }


    void Update()
    {
        transform.Rotate(direction * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Instantiate(pickUpEffect, transform.position, transform.rotation);
            Destroy(gameObject);
            if (isRed)
            {
                // 맵 코인 증가
            }
            else
            {
                // 상점 코인 증가  
            }
        }
    }
}