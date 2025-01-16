using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterInteraction : MonoBehaviour
{
    public List<ParticleSystem> waterParticles;
    public ParticleSystem enterParticles;
    public ParticleSystem exitParticles;

    private Rigidbody playerRigidbody;
    
    [HideInInspector]
    public bool isInsideWater = false;

    private void Start()
    {

        playerRigidbody = GetComponent<Rigidbody>();

        foreach (var p in waterParticles)
        {
            p.Play();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {            
            // Activa el sistema de partículas cuando el jugador entra en el agua por primera vez.
            if (!isInsideWater)
            {
                EmitParticles(2);

                if (enterParticles)
                {
                    enterParticles.Play();
                }
            }
               

            isInsideWater = true;
            
            

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            // Activa el sistema de partículas cuando el jugador entra en el agua por primera vez.

            isInsideWater = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Water"))
        {
            EmitParticles(2);

            if (exitParticles)
            {
                exitParticles.Play();
            }

            isInsideWater = false;

        }
    }



    private void Update()
    {
    
        if (!isInsideWater)
        {

            foreach (var p in waterParticles)
            {
                var velocityOverLifetime = p.velocityOverLifetime;
                velocityOverLifetime.x = 0;
                velocityOverLifetime.z = 0;
            }

            EmitParticles(0);

            return;
        }


        Shader.SetGlobalVector("_Player", transform.position);

        // Obtiene la dirección de movimiento del jugador.
        Vector3 moveDirection = new Vector3(-playerRigidbody.velocity.x, 0f, -playerRigidbody.velocity.z);

        foreach (var p in waterParticles)
        {
            // Aplica la dirección de movimiento al módulo "Velocity over Lifetime".
            var velocityOverLifetime = p.velocityOverLifetime;
            velocityOverLifetime.x = moveDirection.x;
            velocityOverLifetime.z = moveDirection.z;
        }

        // Comprueba si el jugador se está moviendo.
        bool isMoving = moveDirection.magnitude > 0.01f;

        // Si el jugador se está moviendo, emite 10 partículas.
        if (isMoving)
        {
            EmitParticles(10);
        }
        else
        {
            if (Mathf.Abs(playerRigidbody.velocity.y) > 0.0001f)
                EmitParticles(2);
            else
                EmitParticles(0);
        }

    }

    private void EmitParticles(int count)
    {
        foreach (var p in waterParticles)
        {
            // Ajusta la cantidad de partículas a emitir.
            var emissionModule = p.emission;
            emissionModule.rateOverTime = count;
        }

    }

}