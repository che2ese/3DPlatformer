using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Animator:")]
    public Animator anim;
    [Space(10)]
    [Header("Movement settings:")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float sprintSpeed = 15f;
    [Range(0,1)]
    [SerializeField] private float airControl = 0.5f;
    [Space(10)]
    [SerializeField] private float jumpDelay = 0.2f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpHold = 1f;
    [SerializeField] private float coyoteTime = 1f;
    [Space(10)]
    [SerializeField] private float rotationSpeed = 15.0f;
    [Space(10)]
    [SerializeField] private float gravityScale = 2f;
    [Space(10)]
    [SerializeField] private float climbForce = 1f;
    [SerializeField] private float stepHeight = 0.25f;
    [Space(10)]
    [SerializeField] private float attackDuration = 1.2f;
    [SerializeField] private float attackImpulse = 10;
    [Header("Effects:")]
    [SerializeField] private GameObject attackEffectDefault;
    [SerializeField] private GameObject attackEffectGolden;
    [SerializeField] private ParticleSystem moveEffect;

    [Header("Sounds:")]
    [SerializeField] private AudioSource attackSound;
    [SerializeField] private AudioSource headHitSound;
    [SerializeField] private AudioSource jumpSound;
    [SerializeField] private AudioSource doubleJumpSound;
    [SerializeField] private AudioSource walkSound;
    [SerializeField] private AudioSource runSound;
    [SerializeField] private AudioSource landingSound;

    private Rigidbody rb;
    private Transform camTransform;
   
    private float yDirection;
    private float maxWalkVol = 0.3f;
    private float maxRunVol = 0.5f;

    private Coroutine coJump;
    private Coroutine coCoyote;

    private CapsuleCollider characterCollider;
    private Vector3 lastPosition;
    private float stockTime = 0;

    [Header("Debug:")]
    [SerializeField] private bool canDoubleJump;
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isGrounded;
    [HideInInspector]
    public bool isGolden;
    [HideInInspector]
    public bool isAttacking;

    private WaterInteraction waterInteraction;
    private void Awake()
    {
        if (walkSound)
            maxWalkVol = walkSound.volume;

        if (runSound)
            maxRunVol = runSound.volume;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camTransform = Camera.main.transform;
        characterCollider = GetComponent<CapsuleCollider>();
        waterInteraction = GetComponent<WaterInteraction>();
    }

    private void OnDisable()
    {
        if (walkSound)
            walkSound.volume = 0;

        if (runSound)
            runSound.volume = 0;

        if (rb)
            rb.velocity = Vector3.zero;

        anim.SetBool("Move", false);
    }

    void FixedUpdate()
    {
        bool isInsideWater = false;
        if (waterInteraction)
        {
            isInsideWater = waterInteraction.isInsideWater;
        }

        // Check if the player is grounded:
        if (isGrounded)
        {
            RaycastHit hit;
            if (!Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 1.2f))
            {
                if (coCoyote == null)
                {
                    coCoyote = StartCoroutine(CoyoteTime());
                }
            }
            else
            {
                if (coCoyote != null)
                {
                    StopCoroutine(coCoyote);
                }
            }
        }

        float horizontalInput = InputManager.AxisX; 
        float verticalInput = InputManager.AxisY;  

        //Input:
        if (isAttacking)
        {
            horizontalInput = 0;
            verticalInput = 0;
        }


        //Calcule camera direction

        Vector3 camForward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
        Vector3 camRight = new Vector3(camTransform.right.x, 0, camTransform.right.z).normalized;

        Vector3 moveDirection = camForward * verticalInput + camRight * horizontalInput;
        moveDirection = moveDirection.normalized;
 
        //Rotation
        if (moveDirection != Vector3.zero)
        {

            RaycastHit hit;

            // Si el objeto está en el suelo, intentamos subir un escalón si lo hay
            if (Physics.Raycast(transform.position + transform.up * 0.1f, moveDirection, out hit, characterCollider.radius * 1.2f) &&
                !Physics.Raycast(transform.position + transform.up * stepHeight, moveDirection, characterCollider.radius * 1.2f))
            {
                // Comprobamos si el escalón es lo suficientemente bajo para subirlo
                if (hit.distance <= stepHeight)
                {
                    Vector3 toStep = hit.point - transform.position;
                    toStep.y = 0.0f;
                    float angle = Vector3.Angle(moveDirection, toStep);

                    if (angle < 90.0f)
                    {
                        yDirection = Mathf.Abs(Physics.gravity.y) * climbForce;
                    }

                }
            }
            
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            anim.SetBool("Move", true);

        }
        else
        {
            anim.SetBool("Move", false);

        }

        // Sprint
        if (InputManager.Run)
        {
            isSprinting = true;
            moveDirection = moveDirection.normalized * sprintSpeed;
        }
        else
        {
            isSprinting = false;
            moveDirection = moveDirection.normalized * speed;
        }

        //Sound:
        if (isGrounded)
        {
            if (moveDirection != Vector3.zero)
            {
                if (isSprinting)
                {
                    if (walkSound)
                    {
                        walkSound.volume = 0;
                    }

                    if (runSound)
                    {
                        runSound.volume = maxRunVol;
                    }
                }
                else
                {
                    if (walkSound)
                    {
                        walkSound.volume = maxWalkVol;
                    }

                    if (runSound)
                    {
                        runSound.volume = 0;
                    }
                }
            }
            else
            {
                if (walkSound)
                {
                    walkSound.volume = 0;
                }

                if (runSound)
                {
                    runSound.volume = 0;
                }
            }
        }
        else
        {
            if (walkSound)
            {
                walkSound.volume = 0;
            }

            if (runSound)
            {
                runSound.volume = 0;
            }
        }
       
        
        anim.SetBool("Sprint", isSprinting);

        yDirection = Mathf.Clamp(yDirection - (Mathf.Abs(Physics.gravity.y) * gravityScale) * Time.fixedDeltaTime, -Mathf.Abs(Physics.gravity.y) * gravityScale, 9999f);

        moveDirection.y = yDirection;


        if (isGrounded)
        {
            //Move:
            rb.AddForce(moveDirection - rb.velocity);

            if (Mathf.Abs(horizontalInput) < 0.01f && Mathf.Abs(verticalInput) < 0.01f)
            {
                float y = rb.velocity.y;
                rb.velocity = Vector3.MoveTowards(rb.velocity, new Vector3(0, y, 0), Time.fixedDeltaTime * 2); 
            }

            if (isInsideWater)
            {
                if (moveEffect)
                {
                    ParticleSystem.EmissionModule em = moveEffect.emission;
                    em.rateOverDistanceMultiplier = 0;
                }
            }
            else
            {
                if (moveEffect)
                {
                    ParticleSystem.EmissionModule em = moveEffect.emission;
                    em.rateOverDistanceMultiplier = 1;
                }
            }

        }
        else
        {
            //Air move:
            rb.AddForce(new Vector3(moveDirection.x * airControl, moveDirection.y, moveDirection.z * airControl) - rb.velocity);

            if (moveEffect)
            {
                ParticleSystem.EmissionModule em = moveEffect.emission;
                em.rateOverDistanceMultiplier = 0;
            }
        }

        if (!isAttacking)
            anim.SetBool("Ground", isGrounded);

        
    }

    private void Update()
    {
       
        if (isAttacking)
            return;

        // Jump:
        if (isGrounded)
        {
            if (InputManager.JumpDown)
            {
                canDoubleJump = true;
                if (coJump != null)
                {
                    StopCoroutine(coJump);
                }

                anim.SetTrigger("Jump");
                if (jumpSound)
                    jumpSound.Play();

                coJump = StartCoroutine(JumpSequence());
            }
        }
        // Double Jump:
        else
        {
            if (canDoubleJump && InputManager.JumpDown)
            {
                canDoubleJump = false;
                if (coJump != null)
                {
                    StopCoroutine(coJump);
                }

                anim.SetTrigger("DoubleJump");
                if (doubleJumpSound)
                    doubleJumpSound.Play();

                coJump = StartCoroutine(JumpSequence());
            }
        }

        //attack
        if (InputManager.AttackDown)
        {
            StartCoroutine(Attacking());
        }

        //unstock
        if (!isGrounded)
        {
            if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
            {
                if (stockTime > 3)
                {
                    isGrounded = true;
                }
                else
                {
                    stockTime += Time.deltaTime;
                }

            }
            else
            {
                stockTime = 0;
            }
        }
        else
        {
            stockTime = 0;
        }
    }

    IEnumerator JumpSequence()
    {
        float myTime = 0;
        float currentForce = jumpForce;

        //Jump Delay:
        while (myTime < jumpDelay)
        {
            //Hold Force:
            if (InputManager.Jump)
            {
                currentForce += jumpHold * Time.deltaTime;
            }
            
            myTime += Time.deltaTime;
            yield return null;
        }

        //Do Jump:
        isJumping = true;
        isGrounded = false;
        yDirection = 0;

        rb.AddForce(Vector3.up * currentForce, ForceMode.Impulse);

        coJump = null;
    }


    IEnumerator Attacking()
    {
        isAttacking = true;
        AddForce(transform.forward * attackImpulse);
        

        if (isGolden)
        {
            if (attackEffectGolden)
            {
                var clone = Instantiate(attackEffectGolden, transform.position + Vector3.up * 0.5f, transform.rotation);
                clone.transform.parent = this.transform;
            }

        }
        else
        {
            if (attackEffectDefault)
            {
                var clone = Instantiate(attackEffectDefault, transform.position + Vector3.up * 0.5f, transform.rotation);
                clone.transform.parent = this.transform;
            }

        }
     

        if (attackSound)
            attackSound.Play();

        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;

    }

    public void AddForce(Vector3 direction)
    {
        rb.AddForce(direction, ForceMode.Impulse);
    }

    IEnumerator CoyoteTime()
    {
        yield return new WaitForSeconds(coyoteTime);

        RaycastHit hit;
        if (!Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 1.15f))
        {
            isGrounded = false;
            if (!isJumping)
            {
                if (!isAttacking)
                {
                    anim.SetTrigger("Falling");
                    anim.SetBool("Ground", isGrounded);
                }
                   
            }
        }

        coCoyote = null;
    }

    private void OnCollisionStay(Collision collision)
    {
        Vector3 contactNormal;
        float angle;

        if (isGolden && isAttacking)
        {
            var breakable = collision.gameObject.GetComponent<Breakable>();

            if (!breakable)
            {
                breakable = collision.gameObject.GetComponentInParent<Breakable>();
            }

            if (breakable)
            {
                contactNormal = collision.contacts[0].normal;
                angle = Vector3.Angle(contactNormal, -transform.forward);

                if (angle < 45)
                {
                    breakable.Hit();
                    if (headHitSound)
                    {
                        headHitSound.Play();
                    }
                }
   
            }
        }

    }


    private void OnCollisionEnter(Collision collision)
    {
        bool firstGrounded = false;

        for (int i = 0; i < collision.contacts.Length; i++)
        {
            Vector3 contactNormal = collision.contacts[i].normal;
            float angle = Vector3.Angle(contactNormal, Vector3.up);


            if (!firstGrounded)
            {
                //Ground:
                // Si el ángulo es menor a 45 grados, el jugador está en una superficie horizontal
                if (angle < 45f)
                {

                    if (coCoyote != null)
                    {
                        StopCoroutine(coCoyote);
                        coCoyote = null;
                        
                        if (!isAttacking)
                            anim.SetTrigger("Landing");
                        
                        if (landingSound)
                            landingSound.Play();
                    }
                    else
                    {
                        if (!isGrounded)
                        {
                            if (!isAttacking)
                                anim.SetTrigger("Landing");

                            if (landingSound)
                                landingSound.Play();
                        }
                    }

                    isGrounded = true;
                    firstGrounded = true;
                    anim.SetBool("HeadHit", false);

                    isJumping = false;
                    canDoubleJump = true;

                }
            }

            if (isGolden && isAttacking)
            {
                var breakable = collision.gameObject.GetComponent<Breakable>();

                if (!breakable)
                {
                    breakable = collision.gameObject.GetComponentInParent<Breakable>();
                }

                if (breakable)
                {
                    breakable.Hit();
                }

                if (headHitSound)
                {
                    headHitSound.Play();
                }
            }
            else
            {
                //Roof:
                angle = Vector3.Angle(contactNormal, Vector3.down);
                if (angle < 45f)
                {
                    var breakable = collision.gameObject.GetComponent<Breakable>();

                    if (!breakable)
                    {
                        breakable = collision.gameObject.GetComponentInParent<Breakable>();
                    }

                    if (breakable)
                    {
                        breakable.Hit();
                    }

                    anim.SetBool("HeadHit", true);
                    if (headHitSound)
                    {
                        headHitSound.Play();
                    }

                }

                if (isAttacking)
                {
                    if (headHitSound)
                    {
                        headHitSound.Play();
                    }
                }
            }



        }

    

    }

 
}

