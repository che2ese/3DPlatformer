using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerES : MonoBehaviour
{
    // 설정 관련 변수
    [Header("Setting")]
    public Vector3 respawnPosition;

    // 이동 관련 변수
    [Header("Move")]
    public float walkSpeed;
    public float runSpeed;
    public float jumpPower;

    // 스테미나 관련 변수
    [Header ("Stamina")]
    public float maxStamina = 100f; 
    public float stamina;
    public float staminaDecreaseRate = 30f;
    public float staminaRecoveryRate = 10f;
    public float staminaRecoveryDelay = 1f;
    public Image staminaBar;

    // 점프 관련 변수
    [Header("Jump")]
    public float raycastDistance = 1.1f;
    public LayerMask groundLayer;

    // 이동 입력값
    float hAxis;
    float vAxis;

    // 버튼 입력값
    bool runDown;
    bool jumpDown;
    bool attack1Down;

    // 활동 상태
    bool isGrounded;
    bool isRunning;
    bool isStaminaDepleted;
    bool isAttack;

    Vector3 moveVec;

    Animator anim;
    Rigidbody rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        stamina = maxStamina;
    }

    void Update()
    {
        GetInput();
        Move();
        Stamina();
        if (!isStaminaDepleted) Attack();
        if (!isStaminaDepleted) Jump();

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("DeadZone"))
        {
            Respawn();
        }
    }

    void GetInput()
    {
        // 입력값 받기
        hAxis = Input.GetAxis("Horizontal");
        vAxis = Input.GetAxis("Vertical");

        // 스테미나가 0 이상일 때만 달리기 가능
        if (isGrounded)
        {
            if (stamina > 0)
            {
                runDown = Input.GetKey(KeyCode.LeftShift);
            }
            else
            {
                runDown = false;
            }
        }
        
        jumpDown = Input.GetButtonDown("Jump");
        attack1Down = Input.GetButtonDown("Fire1");
    }
    
    void Move()
    {
        // 이동 코드 
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        // 벽 뚫림 방지
        bool wallHit = Physics.Raycast(transform.position, moveVec, 1f, LayerMask.GetMask("Ground"));

        // 벽 인지 레이캐스트 보기 
        Debug.DrawRay(transform.position, moveVec * 1f, wallHit ? Color.red : Color.green, 0.1f);

        if (!wallHit && !isStaminaDepleted)
        {
            transform.position += moveVec * (runDown ? runSpeed : walkSpeed) * Time.deltaTime;
        }

        // 애니메이션 설정 
        anim.SetBool("isWalk", moveVec != Vector3.zero);
        anim.SetBool("isRun", runDown);

        // 방향 설정
        if (!isStaminaDepleted) transform.LookAt(transform.position + moveVec);
    }

    void Stamina()
    {
        // 스테미나 관리
        isRunning = runDown && moveVec != Vector3.zero;

        // 달리는 동시에 땅에 있지 않을 때
        if (isRunning && !isGrounded)
        {
            stamina -= staminaDecreaseRate / 4 * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);

            // 스테미나 0이 되면 일정 시간 대기 후 회복 시작
            if (stamina <= 0 && !isStaminaDepleted)
            {
                isRunning = false;
            }
        }

        // 달리는 동시에 땅에 있을 때
        if (isRunning && isGrounded)
        {
            stamina -= staminaDecreaseRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);

            // 스테미나 0이 되면 일정 시간 대기 후 회복 시작
            if (stamina <= 0 && !isStaminaDepleted)
            {
                StartCoroutine(WaitBeforeRecovery());
            }
        }

        // 달리지 않을 때 스테미나 회복
        if (!isRunning && stamina < maxStamina && !isStaminaDepleted)
        {
            stamina += staminaRecoveryRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }

        // 스테미나 UI 업데이트
        staminaBar.fillAmount = stamina / maxStamina;
    }

    IEnumerator WaitBeforeRecovery()
    {
        // 스테미나 고갈 딜레이
        isStaminaDepleted = true;
        yield return new WaitForSeconds(staminaRecoveryDelay);
        stamina = 3;
        isStaminaDepleted = false;
    }

    void Attack()
    {
        // 근접 공격 
        if (attack1Down && !isAttack)
        {
            anim.SetTrigger("doAttack1");
            StartCoroutine(ResetAttackCooldown());
        }
    }

    IEnumerator ResetAttackCooldown()
    {
        // 공격하면 2초 쿨타임 && 다른 anim 제한
        anim.SetBool("isAttack", true);
        isAttack = true;

        yield return new WaitForSeconds(1f);

        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    void Jump()
    {
        // 점프 기능 
        isGrounded = Physics.Raycast(transform.position, Vector3.down, raycastDistance, groundLayer);

        // 바닥 인지 레이캐스트 보기 
        Debug.DrawRay(transform.position, Vector3.down * raycastDistance, isGrounded ? Color.green : Color.red, 0.1f);

        if (isGrounded)
        {
            if (jumpDown)
            {
                rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                anim.SetBool("isJump", true);
                anim.SetTrigger("doJump");
            }
            else
            {
                anim.SetBool("isJump", false);
            }
        } 
    }

    void Respawn()
    {
        // DeadZone에 닿았을 때 부활
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        transform.position = respawnPosition;
        stamina = 100;
    }
}
