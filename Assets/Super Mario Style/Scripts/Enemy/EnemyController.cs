using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(Rigidbody))]

[RequireComponent(typeof(CapsuleCollider))]
public class EnemyController : MonoBehaviour
{
    public enum type
    {
        none,
        Patrol,
        Shoot,
        Tackles,
        Attacker,
    }
    [SerializeField] private int Health = 1;
    [SerializeField] private int damage = 1;
    [SerializeField] private Transform[] waypoints; // Array of points to follow
    [SerializeField] private float waypointMinDist = 0.5f;
    [SerializeField] private float speed = 2f; // Enemy speed
    [SerializeField] private float rotationSpeed = 20;
    [Header("On take damage:")]
    [SerializeField] private float stopOnHit = 1;
    [SerializeField] private float hitForce = 15;

    [Header("Player detection:")]
    [SerializeField] private float detectionRadius = 10f; // Player detection radius
    [SerializeField] private LayerMask detectionLayerMask; // Layers to detect
    [SerializeField] private type enemyType;
    [Header("Shooter:")]
    [SerializeField] private float shootInterval = 2f; // Time interval between shots
    [SerializeField] private GameObject projectile; // Projectile prefab to shoot
    [SerializeField] private Transform projectileSpawnPoint; // Projectile spawn point
    [Header("Tackler:")]
    [SerializeField] private float tacklerSpeed = 6;
    [Header("Attacker:")]
    [SerializeField] private float attackDistance = 2;
    [SerializeField] private float attackArea = 2;
    [SerializeField] private float attackStopTime = 2;
    [SerializeField] private string animStateHitHash = "Hit";
    [SerializeField] private float damageDelay = 0.15f;

    [Header("Effects:")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject deadEffect;
    [Header("Sound:")]
    [SerializeField] private AudioSource moveSound;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip tackleClip;
    [SerializeField] private float patrolVolume = 0.6f;
    [SerializeField] private float patrolPitch = 1f;
    [Space(10)]
    [SerializeField] private float tackleVolume = 1f;
    [SerializeField] private float tacklePitch = 1.2f;
    [SerializeField] private float yDeadZone = -100;
    private int currentWaypointIndex = 0; // Index of current point
    private Rigidbody rb; // Enemy Rigidbody
    private float lastShootTime; // Time when last shot was fired
    private bool dead;
    private Animator anim;
    private CapsuleCollider collider;
    private bool firstShoot = true;
    private float currentHitTime;


    public UnityEngine.Events.UnityEvent onTakeHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider>();

        foreach (var p in waypoints)
        {
            RaycastHit hit;
            if (Physics.Raycast(p.position, Vector3.down, out hit, Mathf.Infinity))
            {
                p.position = hit.point + Vector3.up * 0.5f;
            }
            p.parent = null;
        }
    }

    void FixedUpdate()
    {
        if (transform.position.y < yDeadZone)
        {
            Destroy(gameObject);
        }

        if (dead)
            return;

        if (enemyType == type.none)
        {
            if (moveSound)
            {
                moveSound.volume = 0;
            }

            return;
        }


        // Perform a SphereCast to detect the player
        bool playerDetected = false;
        Vector3 playerPosition = Vector3.zero;
        if (enemyType != type.Patrol)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayerMask);

            if (colliders.Length == 0)
            {
                firstShoot = true;
            }

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.CompareTag("Player"))
                {

                    playerPosition = collider.transform.position;
                    playerDetected = true;
                    break;
                }
            }
        }

        if (playerDetected)
        {
            Vector3 playerDirection = playerPosition - transform.position;
            playerDirection.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(playerDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, (rotationSpeed * 2) * Time.deltaTime);

            if (enemyType == type.Shoot)
            {
                if (Time.time - lastShootTime > shootInterval)
                {
                    if (!firstShoot)
                    {
                        Instantiate(projectile, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                        anim.SetTrigger("Shoot");
                    }

                    firstShoot = false;
                    anim.SetBool("Move", false);

                    if (moveSound)
                        moveSound.volume = 0;

                    lastShootTime = Time.time; // Update the last shot time
                }
            }
            else if (enemyType == type.Tackles)
            {
                if (currentHitTime <= 0)
                {
                    rb.AddForce(transform.forward * tacklerSpeed, ForceMode.Acceleration);
                }
                else
                {
                    currentHitTime -= Time.fixedDeltaTime;
                }

                anim.SetInteger("Speed", 2);
                anim.SetBool("Move", true);

                if (moveSound)
                {
                    moveSound.volume = tackleVolume;
                    moveSound.pitch = tacklePitch;
                    if (tackleClip)
                        moveSound.clip = tackleClip;

                    if (!moveSound.isPlaying)
                        moveSound.Play();
                }

            }
            else if (enemyType == type.Attacker)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName(animStateHitHash) || anim.GetNextAnimatorStateInfo(0).IsName(animStateHitHash))
                {
                    return;
                }
                
                if (Vector3.Distance(playerPosition, transform.position) > attackDistance)
                {
                    if (currentHitTime <= 0)
                    {
                        rb.AddForce(transform.forward * tacklerSpeed, ForceMode.Acceleration);
                    }
                    else
                    {
                        currentHitTime -= Time.fixedDeltaTime;
                    }

                    anim.SetInteger("Speed", 2);
                    anim.SetBool("Move", true);

                    if (moveSound)
                    {
                        moveSound.volume = tackleVolume;
                        moveSound.pitch = tacklePitch;
                        if (tackleClip)
                            moveSound.clip = tackleClip;

                        if (!moveSound.isPlaying)
                            moveSound.Play();
                    }
                } 
                else
                {
                    if (Time.time - lastShootTime > shootInterval)
                    {

                        rb.velocity = Vector3.zero;

                        StartCoroutine(DamageDelay());

                        currentHitTime = attackStopTime;
                        anim.SetBool("Move", false);
                        anim.SetTrigger("Shoot");

                        if (moveSound)
                            moveSound.volume = 0;

                        lastShootTime = Time.time; // Update the last shot time
                    }
                }
  

            }
        }

        // If the player has been detected and enough time has passed since the last shot, shoot a projectile
        if (!playerDetected)
        {
            anim.SetInteger("Speed", 1);
            anim.SetBool("Move", true);

            if (moveSound)
            {
                moveSound.volume = patrolVolume;
                moveSound.pitch = patrolPitch;

                if (walkClip)
                    moveSound.clip = walkClip;

                if (!moveSound.isPlaying)
                    moveSound.Play();
            }


            // Move the enemy towards the current point
            Vector3 direction = (waypoints[currentWaypointIndex].position - transform.position).normalized;
            if (currentHitTime <= 0)
            {
                Vector3 dir = transform.forward;
                dir.y = 0;
                rb.velocity = (Vector3.down * 9) +  dir * speed;
            }
            else
            {
                currentHitTime -= Time.fixedDeltaTime; 
            }
           

            // If the enemy reaches the current point, move to the next one
            float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);
            if (distanceToWaypoint < waypointMinDist)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = 0;
                }
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw the detection sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + (transform.forward * attackDistance), attackArea);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (dead)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 contactNormal = collision.contacts[0].normal;
            float angle = Vector3.Angle(contactNormal, Vector3.down);

            if (angle < 45f)
            {
                Death(collision.gameObject);

                collision.gameObject.GetComponent<PlayerController>().AddForce(Vector3.up * 6);
            }
            else
            {
                if (collision.gameObject.GetComponent<PlayerController>().isAttacking && !collision.gameObject.GetComponent<PlayerController>().isGolden)
                {
                    currentHitTime = stopOnHit;
                    onTakeHit.Invoke();
                    rb.AddForce((transform.position - collision.transform.position).normalized * hitForce, ForceMode.Impulse);
                }
                else if (collision.gameObject.GetComponent<PlayerController>().isAttacking && collision.gameObject.GetComponent<PlayerController>().isGolden)
                {
                    Death(collision.gameObject);
                }
                else
                {
                    collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damage, (collision.transform.position - transform.position).normalized * 15);
                    rb.AddForce((transform.position - collision.transform.position).normalized * hitForce/2, ForceMode.Impulse);
                }

            }
        }

        
    }

    IEnumerator DamageDelay()
    {
        yield return new WaitForSeconds(damageDelay);
        if (dead)
        {
            yield break;
        }

        var cols = Physics.OverlapSphere(transform.position + (transform.forward * attackDistance), attackArea, detectionLayerMask);
        foreach (var col in cols)
        {
            col.gameObject.GetComponent<PlayerHealth>().TakeDamage(damage, (col.transform.position - transform.position).normalized * 15);
        }
    }
    public void Death(GameObject toIgnore)
    {
        Health = Health - 1;

        if (hitEffect)
            Instantiate(hitEffect, transform.position + (Vector3.up * collider.height), Quaternion.identity);


        if (Health <= 0)
        {

            dead = true;
            rb.isKinematic = true;
            collider.enabled = false;

            anim.SetTrigger("Dead");
            moveSound.volume = 0;

            StartCoroutine(DestroyDelay());
        }
        else
        {
            currentHitTime = attackStopTime;
            rb.isKinematic = true;
            anim.SetTrigger("Damage");
            moveSound.volume = 0;

            onTakeHit.Invoke();

            StartCoroutine(HitDelay(toIgnore));
        }
    }


    IEnumerator HitDelay(GameObject toIgnore)
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), toIgnore.GetComponent<Collider>(), true);
        yield return new WaitForSeconds(1);

        Physics.IgnoreCollision(GetComponent<Collider>(), toIgnore.GetComponent<Collider>(), false);
        rb.isKinematic = false;
    }

    IEnumerator DestroyDelay()
    {
        yield return new WaitForSeconds(3);

        if (deadEffect)
            Instantiate(deadEffect, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);
    }

    public void SetAttacker()
    {
        enemyType = type.Attacker;
        currentHitTime = attackStopTime;

    }

    public void SetPatrol()
    {
        enemyType = type.Patrol;
    }

    public void SetTackler()
    {
        enemyType = type.Tackles;
    }

    public void SetShooter()
    {
        enemyType = type.Shoot;
    }   
    
    public void SetNone()
    {
        enemyType = type.none;
    }
}