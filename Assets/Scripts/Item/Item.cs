using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour
{
    public GameObject hitEffect;
    public GameObject breakEffect;

    private bool destroy;

    private Renderer childRenderer;

    private void Start()
    {
        // 자식 오브젝트에서 Renderer 컴포넌트를 찾기
        childRenderer = GetComponentInChildren<Renderer>();

        // 자식 Renderer가 존재하면 그림자 설정
        if (childRenderer != null)
        {
            childRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; // 그림자 생성
            childRenderer.receiveShadows = true;  // 그림자 받기
        }
    }

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
            // breakEffect를 현재 물체와 같은 부모 아래에 생성
            GameObject effect = Instantiate(breakEffect, transform.position, transform.rotation);
            effect.transform.SetParent(transform.parent);  // 부모를 현재 오브젝트의 부모로 설정

            // 새로 생성된 breakEffect에도 그림자 설정
            Renderer effectRenderer = effect.GetComponentInChildren<Renderer>();  // 자식 Renderer 찾기
            if (effectRenderer != null)
            {
                effectRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; // 그림자 생성
                effectRenderer.receiveShadows = true;  // 그림자 받기
            }
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
