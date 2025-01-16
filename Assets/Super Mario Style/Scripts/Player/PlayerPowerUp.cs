using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerPowerUp : MonoBehaviour
{
    [SerializeField] Animator cameraAnimator;
    [SerializeField] SkinnedMeshRenderer bodyMesh;
    [SerializeField] Material defaultMaterial;
    [SerializeField] Material goldMaterial;
    [SerializeField] GameObject changeEffect;

    public static PlayerPowerUp instance;

    private PlayerController controller;
    private void Awake()
    {
        instance = this;
        controller = GetComponent<PlayerController>();
    }
    
    public void DoPowerUp()
    {
        if (!controller.isGolden)
        {
            controller.isGolden = true;
            bodyMesh.material = goldMaterial;
            StartCoroutine(ChangeSequence());
        }
    }

    IEnumerator ChangeSequence()
    {
        Time.timeScale = 0;

        if (cameraAnimator)
            cameraAnimator.SetTrigger("PowerUp");

        if (changeEffect)
            Instantiate(changeEffect, transform.position + Vector3.up * 0.5f, Quaternion.identity);

        bodyMesh.material = goldMaterial;
        yield return new WaitForSecondsRealtime(0.05f);
        bodyMesh.material = defaultMaterial;
        yield return new WaitForSecondsRealtime(0.05f);
        bodyMesh.material = goldMaterial;
        yield return new WaitForSecondsRealtime(0.1f);
        bodyMesh.material = defaultMaterial;
        yield return new WaitForSecondsRealtime(0.15f);
        bodyMesh.material = goldMaterial;
        yield return new WaitForSecondsRealtime(0.2f);
        bodyMesh.material = defaultMaterial;
        yield return new WaitForSecondsRealtime(0.25f);
        bodyMesh.material = goldMaterial;
        yield return new WaitForSecondsRealtime(1);

        Time.timeScale = 1;
    }


    public void LosePowerUp()
    {
        if (controller.isGolden)
        {
            UIController.instance.Fill(0);
            controller.isGolden = false;
            StartCoroutine(ChangeLose());
        }

    }

    IEnumerator ChangeLose()
    {
        bodyMesh.material = defaultMaterial;
        yield return new WaitForSecondsRealtime(0.25f);
        bodyMesh.material = goldMaterial;
        yield return new WaitForSecondsRealtime(0.2f);
        bodyMesh.material = defaultMaterial;
        yield return new WaitForSecondsRealtime(0.15f);
        bodyMesh.material = goldMaterial;
        yield return new WaitForSecondsRealtime(0.1f);
        bodyMesh.material = defaultMaterial;
        yield return new WaitForSecondsRealtime(0.05f);
        bodyMesh.material = goldMaterial;
        yield return new WaitForSecondsRealtime(0.05f);
        bodyMesh.material = defaultMaterial;
    }

}
