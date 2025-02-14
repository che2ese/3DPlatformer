using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPhysics : MonoBehaviour
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
    public LayerMask itemLayer;

    // 공격 관련 변수
    [Header("Attack")]
    public float CatAttackCooldown;
    public float PandaAttackCooldown;
    public float MonkeyAttackCooldown;
    public float RabbitAttackCooldown;
    public GameObject CatPunchEffect;
    public GameObject PandaPunchEffect;

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
    bool isFalling;
    bool isJump;
    bool isCollidingWithGround; // 땅에 있는 지 (콜라이더 기준)

    bool wallHit;

    Vector3 moveVec;

    Animator anim;
    Rigidbody rigid;

    void Awake()
    {
        SetActiveCharacter(characterNum);
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        stamina = maxStamina;
    }

    void SetActiveCharacter(int characterNum)
    {
        // 모든 캐릭터를 비활성화한 후, 선택된 것만 활성화
        for (int i = 0; i < character.Length; i++)
        {
            character[i].SetActive(i == characterNum);
        }
        // 캐릭터 번호가 3일 경우 CapsuleCollider 크기 조정
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (characterNum == 3 && col != null)
        {
            col.center = new Vector3(col.center.x, -0.45f, col.center.z);
            col.height = 3.5f;
        }
    }

    void Update()
    {
        GetInput();
        CheckWallCollision();
        Move();
        Stamina();
        Attack();
        Jump();
        Fall();
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

    void CheckWallCollision()
    {

        // 벽 충돌 감지 (머리, 중심, 발)
        Vector3 centerPosition = transform.position + Vector3.down * 0.6f;
        Vector3 headPosition = transform.position + Vector3.up * 0.4f;
        Vector3 footPosition = transform.position + Vector3.down * 1.6f;

        Vector3 checkDirection = (moveVec != Vector3.zero && !isAttack) ? moveVec : transform.forward; // 이동 중이면 moveVec, 아니면 정면 방향 사용

        bool wallHitCenter = Physics.Raycast(centerPosition, checkDirection, 0.9f, LayerMask.GetMask("Ground"));
        bool wallHitHead = Physics.Raycast(headPosition, checkDirection, 0.8f, LayerMask.GetMask("Ground"));
        bool wallHitFoot = Physics.Raycast(footPosition, checkDirection, 0.4f, LayerMask.GetMask("Ground"));

        wallHit = wallHitCenter || wallHitHead || wallHitFoot;

        // 벽 감지 레이캐스트 디버깅
        Debug.DrawRay(centerPosition, checkDirection * 0.9f, wallHitCenter ? Color.red : Color.green, 0.1f);
        Debug.DrawRay(headPosition, checkDirection * 0.8f, wallHitHead ? Color.red : Color.green, 0.1f);
        Debug.DrawRay(footPosition, checkDirection * 0.4f, wallHitFoot ? Color.red : Color.green, 0.1f);
    }

    void Move()
    {
        // 카메라 기준 방향 벡터 가져오기
        Vector3 camForward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
        Vector3 camRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z).normalized;

        // 입력 방향을 카메라 방향 기준으로 변환
        moveVec = (camForward * vAxis + camRight * hAxis).normalized;

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
            stamina += staminaDecreaseRate / 10 * Time.deltaTime;
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
            stamina += staminaRecoveryRate / 5 * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
            staminaBar.fillAmount = stamina / maxStamina;

            recoveryTime += Time.deltaTime;
            yield return null; 
        }

        isStaminaDepleted = false;
    }

    void Attack()
    {
        if (isStaminaDepleted) return;

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
            CatPunchEffect.SetActive(true);

            // 애니메이션 보정 
            Quaternion originalRotation = transform.rotation;

            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y + 45,
                transform.rotation.eulerAngles.z
            );

            yield return new WaitForSeconds(1f);

            // 원래 회전값으로 되돌리기
            transform.rotation = originalRotation;
            CatPunchEffect.SetActive(false);
        }
        else if (characterNum == 1)
        {
            // 슬라이딩 전 달리기
            yield return MoveCharacter(startPosition, transform.forward * 6.0f, 0.6f);

            // 이펙트 보이기와 슬라이딩
            var emission = PandaPunchEffect.GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = 20;
            PandaPunchEffect.SetActive(true);
            PandaPunchEffect.GetComponent<ParticleSystem>().Play();

            startPosition = transform.position;
            yield return MoveCharacter(startPosition, transform.forward * 10.5f, 0.5f);

            // 이펙트 서서히 제거 및 대기 
            emission.rateOverTime = 0;
            yield return new WaitForSeconds(0.4f);
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

        // 캐릭터에 따른 공격 후 쿨타임
        if (characterNum == 0)
            yield return new WaitForSeconds(CatAttackCooldown);
        else if (characterNum == 1)
        {
            yield return new WaitForSeconds(PandaAttackCooldown);
            PandaPunchEffect.SetActive(false);
        }
        else if (characterNum == 2)
            yield return new WaitForSeconds(MonkeyAttackCooldown);
        else if (characterNum == 3)
            yield return new WaitForSeconds(RabbitAttackCooldown);
        isAbleAttack = true;
    }

    // 공격 이동을 담당 함수 
    private IEnumerator MoveCharacter(Vector3 startPos, Vector3 moveOffset, float moveDuration)
    {
        Vector3 targetPosition = startPos + moveOffset;
        float elapsedTime = 0f;
        bool stoppedByWall = false; // 벽에 의해 이동이 멈췄는지 확인

        while (elapsedTime < moveDuration)
        {
            if (wallHit)
            {
                // 벽에 닿으면 현재 위치 저장 후 이동 중단
                stoppedByWall = true;
                break;
            }

            bool isAttackGrounded = Physics.Raycast(transform.position, Vector3.down, raycastDistance * 1.2f, groundLayer);
            if (!isAttackGrounded)
            {
                // 공중에 있을 경우 점점 떨어지게 만듦
                targetPosition += Vector3.down * 7 * Time.deltaTime;
            }

            transform.position = Vector3.Lerp(startPos, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // 벽에 부딪혔다면, 남은 시간 동안 현재 위치 유지
        if (stoppedByWall)
        {
            float remainingTime = moveDuration - elapsedTime;
            while (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // 벽에 부딪히지 않았을 경우 최종 위치 보정
            transform.position = targetPosition;
        }
    }

    void Jump()
    {
        // 땅에 있는 상태 확인 
        isGrounded = Physics.Raycast(transform.position, Vector3.down, raycastDistance, groundLayer);
        Debug.DrawRay(transform.position, Vector3.down * raycastDistance, isGrounded ? Color.green : Color.red, 0.1f);

        if (isStaminaDepleted || isAttack) return; 

        // float rayRadius = 0.7f; // 아이템 판독
        // isBlock = Physics.SphereCast(transform.position, rayRadius, Vector3.up, out RaycastHit hit, raycastDistance, itemLayer);

        if (isGrounded && jumpDown) // 땅에 있을 때 점프 가능
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Fall()
    {
        // 낙하 애니메이션
        // 거리 측정하여 땅과 근접하다면 낙하 애니메이션 실행 X

        RaycastHit hit;
        bool isNearGround = Physics.Raycast(transform.position, Vector3.down, out hit, 10f, groundLayer);

        if (!isGrounded && rigid.velocity.y < -0.1f && !isNearGround && !isFalling && !isJump && !isCollidingWithGround)
        {
            anim.SetTrigger("doFall");
            anim.SetBool("isFall", true);
            isFalling = true;
        }
    }

    void DrawThickRay(Vector3 start, Vector3 direction, float distance, Color color, float thickness = 0.05f)
    {
        Vector3 right = Vector3.Cross(direction, Camera.main.transform.forward).normalized * thickness;

        Debug.DrawLine(start, start + direction * distance, color, 0.1f);
        Debug.DrawLine(start + right, start + right + direction * distance, color, 0.1f);
        Debug.DrawLine(start - right, start - right + direction * distance, color, 0.1f);
    }

    void Respawn()
    {
        // DeadZone에 닿았을 때 부활
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        transform.position = respawnPosition;
        stamina = 100;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ScaleBlock"))
        {
            rigid.useGravity = true;
            rigid.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 땅에 닿았을 때 + 착지 애니메이션 작동 
        if (collision.gameObject.layer == 6)
        {
            anim.SetBool("isJump", false);
            anim.SetBool("isFall", false);
            isFalling = false;
            isJump = false;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 6) 
        {
            isCollidingWithGround = true;

            if (isFalling)
            {
                anim.SetBool("isFall", false);
                isFalling = false;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 6) 
        {
            isCollidingWithGround = false;
        }
    }
}
