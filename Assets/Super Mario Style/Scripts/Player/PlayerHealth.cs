using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] 
    private int maxHealth = 3;
    
    [SerializeField]
    private GameObject hitEffect;
    [SerializeField]
    private Animator cameraAnimator;

    private int currentHealth;
    private PlayerController controller;
    private PlayerPowerUp powerUp;
    [HideInInspector]
    public bool isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        controller = GetComponent<PlayerController>();
        powerUp = GetComponent<PlayerPowerUp>();
    }

    public void Revive()
    {
        currentHealth = maxHealth;
        powerUp.LosePowerUp();
        isDead = false;
        controller.enabled = true;
        controller.isAttacking = false;
        controller.anim.SetTrigger("Revive");
    }

    public void TakeDamage(int damage, Vector3 direction)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, 999);
        
        if (currentHealth == 0)
        {
            isDead = true;
            controller.StopAllCoroutines();
            controller.enabled = false;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            controller.anim.SetTrigger("Dead");

            GameManager.instance.Death(1);
        }
        else
        {
          
            controller.AddForce(direction);
        }

        if (hitEffect)
            Instantiate(hitEffect, transform.position, hitEffect.transform.rotation);

        if (cameraAnimator)
            cameraAnimator.SetTrigger("Hurt");

        powerUp.LosePowerUp();
    }

}
