using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerES : MonoBehaviour
{
    // 설정 관련 변수
    [Header("Setting")]
    public Vector3 respawnPosition;

    // 캐릭터 관련 변수
    [Header("Character")]
    public int characterNum;
    public GameObject[] character;

    // 이동 관련 변수
    [Header("Move")]
    public float walkSpeed;
    public float runSpeed;
    public float jumpPower;

    Transform parentTransform;

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

    // 공격 관련 변수
    [Header("Attack")]
    public float attackCooldown;

    bool isAbleAttack = true;

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
        SetActiveCharacter(characterNum);
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        parentTransform = transform;
        stamina = maxStamina;
    }

    void SetActiveCharacter(int characterNum)
    {
        // 모든 캐릭터를 비활성화한 후, 선택된 것만 활성화
        for (int i = 0; i < character.Length; i++)
        {
            character[i].SetActive(i == characterNum);
        }
    }

    void Update()
    {
        GetInput();
        Move();
        Stamina();
        if (!isStaminaDepleted) Attack();
        if (!isStaminaDepleted && !isAttack) Jump();
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
        // 카메라 기준 방향 벡터 가져오기
        Vector3 camForward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
        Vector3 camRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z).normalized;

        // 입력 방향을 카메라 방향 기준으로 변환
        moveVec = (camForward * vAxis + camRight * hAxis).normalized;

        // 벽 충돌 감지 (머리, 중심, 발)
        Vector3 centerPosition = transform.position + Vector3.down * 0.6f;
        Vector3 headPosition = transform.position + Vector3.up * 0.4f;
        Vector3 footPosition = transform.position + Vector3.down * 1.6f;

        bool wallHitCenter = Physics.Raycast(centerPosition, moveVec, 0.9f, LayerMask.GetMask("Ground"));
        bool wallHitHead = Physics.Raycast(headPosition, moveVec, 0.8f, LayerMask.GetMask("Ground"));
        bool wallHitFoot = Physics.Raycast(footPosition, moveVec, 0.4f, LayerMask.GetMask("Ground"));
        bool wallHit = wallHitCenter || wallHitHead || wallHitFoot;

        // 벽 감지 레이캐스트 보기 
        Debug.DrawRay(centerPosition, moveVec * 0.9f, wallHitCenter ? Color.red : Color.green, 0.1f);
        Debug.DrawRay(headPosition, moveVec * 0.8f, wallHitHead ? Color.red : Color.green, 0.1f);
        Debug.DrawRay(footPosition, moveVec * 0.4f, wallHitFoot ? Color.red : Color.green, 0.1f);

        if (!wallHit && !isStaminaDepleted && !isAttack)
        {
            // 이동 처리
            transform.position += moveVec * (runDown ? runSpeed : walkSpeed) * Time.deltaTime;

            if (moveVec != Vector3.zero)
            {

                Quaternion targetRotation = Quaternion.LookRotation(moveVec, Vector3.up);

                if (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                }
            }
        }

        // 애니메이션 설정 
        anim.SetBool("isWalk", moveVec != Vector3.zero);
        anim.SetBool("isRun", runDown);
    }

    void Stamina()
    {
        // 스테미나 관리
        isRunning = !isStaminaDepleted && runDown && moveVec != Vector3.zero;

        // 달리는 동시에 땅에 있지 않을 때
        if (isRunning && !isGrounded && !isAttack)
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
        if (isRunning && isGrounded && !isAttack)
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
        anim.SetTrigger("doStaminaRecovery");

        float recoveryTime = 0f;

        while (recoveryTime < staminaRecoveryDelay)
        {
            stamina += staminaRecoveryRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
            staminaBar.fillAmount = stamina / maxStamina;

            recoveryTime += Time.deltaTime;
            yield return null; 
        }

        isStaminaDepleted = false;
    }

    void Attack()
    {
        // 근접 공격 
        if (attack1Down && !isAttack && isGrounded && isAbleAttack)
        {
            anim.SetTrigger("doAttack1");
            StartCoroutine(ResetAttackCooldown());
        }
    }

    IEnumerator ResetAttackCooldown()
    {
        // 공격하면 쿨타임 && 다른 anim 제한
        anim.SetBool("isAttack", true);
        isAttack = true;
        isAbleAttack = false;

        Vector3 startPosition = transform.position;

        // 각 캐릭터에 따른 공격 이동 
        if (characterNum == 0)
        {
            yield return new WaitForSeconds(1f);
        }
        else if (characterNum == 1)
        {
            yield return MoveCharacter(startPosition, transform.forward * 8.0f, 1.2f);
        }
        else if (characterNum == 2)
        {
            yield return MoveCharacter(startPosition, transform.forward * 4.0f, 0.5f);

            startPosition = transform.position;
            yield return MoveCharacter(startPosition, transform.forward * 5.5f + Vector3.up * 1.4f, 0.6f);
        }
        else if (characterNum == 3)
        {
            yield return MoveCharacter(startPosition, transform.forward * 4.0f, 0.5f);

            startPosition = transform.position;
            yield return MoveCharacter(startPosition, transform.forward * 3.5f + Vector3.up * 1.4f, 0.4f);

            startPosition = transform.position;
            yield return MoveCharacter(startPosition, transform.forward * 6.5f + Vector3.up * -1f, 0.6f);
        }

        isAttack = false;
        anim.SetBool("isAttack", false);

        // 공격 후 쿨타임 
        yield return new WaitForSeconds(attackCooldown);
        isAbleAttack = true;
    }

    // 이동을 담당 함수 
    private IEnumerator MoveCharacter(Vector3 startPos, Vector3 moveOffset, float moveDuration)
    {
        Vector3 targetPosition = startPos + moveOffset;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
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

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ScaleBlock"))
        {
            rigid.useGravity = true;
            rigid.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
    }
}
