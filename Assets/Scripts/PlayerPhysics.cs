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
    public GameObject RunEffect;
    public GameObject SpeedUpRunEffect;

    // 스테미나 관련 변수
    [Header("Stamina")]
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
    public GameObject MonkeyPunchEffect;
    public GameObject RabbitPunchEffect;

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
    bool isRabbitRespawn;
    bool isShock;
    bool isSpeedUp;
    [HideInInspector]
    public bool isInvincibility;
    [HideInInspector]
    public bool isPush;

    // 아이템
    [Header("Item")]
    public GameObject[] CatBody;
    public GameObject[] PandaBody;
    public GameObject[] MonkeyBody;
    public GameObject[] RabbitBody; // 플레이어의 자식 게임오브젝트 배열
    public GameObject CatHide;
    public GameObject RabbitHide;
    public Material invincibleMaterial; // 무적 상태일 때 적용할 메테리얼
    private Material[][] originalMaterials;  // 원래 메테리얼을 저장할 배열

    GameObject[] body = null;

    public Material afterImageMaterial; // 잔상용 머티리얼 (반투명 머티리얼)
    float afterImageDuration = 0.5f; // 잔상이 유지되는 시간
    float afterImageInterval = 0.1f; // 잔상 생성 간격
    Coroutine afterSpeedImageCoroutine; // 코루틴 핸들 저장
    Coroutine afterShockImageCoroutine; // 코루틴 핸들 저장

    public GameObject StaminaUpEffect;
    public GameObject NoStaminaEffect;

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

    private void Start()
    {
        switch (characterNum)
        {
            case 0:
                body = CatBody;
                break;
            case 1:
                body = PandaBody;
                break;
            case 2:
                body = MonkeyBody;
                break;
            case 3:
                body = RabbitBody;
                break;
        }

        // 원래의 메테리얼들을 배열에 저장
        originalMaterials = new Material[body.Length][];
        for (int i = 0; i < body.Length; i++)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = body[i].GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                originalMaterials[i] = skinnedMeshRenderer.materials; // 여러 메테리얼 저장
            }
        }
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
        Attack();
        Jump();
        Stamina();
        Fall();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("DeadZone"))
        {
            Respawn();
        }

        if (!isInvincibility && other.CompareTag("ManHole"))
        {
            isPush = true;
            anim.SetBool("isPush", true);
            anim.SetTrigger("doPush");

            Vector3 playerPosition = transform.position;
            Vector3 manholePosition = other.transform.position;
            Vector3 contactPoint = other.ClosestPoint(playerPosition); // 가장 가까운 충돌 지점
            Vector3 bounceDirection = (playerPosition - contactPoint).normalized; // 반대 방향

            // 충돌 방향 확인: 좌우(X축 변화량이 Z축 변화량보다 클 경우)
            Vector3 collisionDirection = (playerPosition - manholePosition).normalized;
            if (Mathf.Abs(collisionDirection.x) > Mathf.Abs(collisionDirection.z))
                other.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            else
                other.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            float bounceForce = 15f; // 튕겨 나가는 힘
            float upwardForce = 1.5f; // 위로 뜨는 힘

            // 기존 속도 초기화 후 반대 방향 + 위쪽으로 튕겨 나감
            rigid.velocity = Vector3.zero;

            // 바운스 방향 보정 (수직 충돌 시 옆 방향으로 강제 튕기기)
            if (Mathf.Abs(bounceDirection.x) < 0.1f && Mathf.Abs(bounceDirection.z) < 0.1f)
            {
                bounceDirection += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * 0.5f;
            }

            Vector3 finalBounceDirection = bounceDirection + Vector3.up * 0.2f; // 위로 살짝 추가
            rigid.AddForce(finalBounceDirection.normalized * bounceForce + Vector3.up * upwardForce, ForceMode.Impulse);
        }
        if (other.CompareTag("Invincibility"))
        {
            isInvincibility = true;
            StartCoroutine(ActivateInvincibility());
            Destroy(other.gameObject); // 아이템 제거
        }
        if (other.CompareTag("SpeedUp"))
        {
            StartCoroutine(SpeedUp());
            Destroy(other.gameObject);
        }
        if (!isInvincibility && !isShock && other.CompareTag("Lightning"))
        {
            isShock = true;
            StartCoroutine(ElectricShock());
        }
        if (other.CompareTag("StaminaUp"))
        {
            StaminaUpEffect.SetActive(true);
            stamina = maxStamina;
            Destroy(other.gameObject);
        }
    }

    IEnumerator ElectricShock()
    {
        anim.SetTrigger("doShock");

        // 코루틴 시작 및 핸들 저장
        afterShockImageCoroutine = StartCoroutine(GenerateAfterImage());

        yield return new WaitForSeconds(3f);

        // 잔상 코루틴 중지
        if (afterShockImageCoroutine != null)
        {
            StopCoroutine(afterShockImageCoroutine);
            afterShockImageCoroutine = null;
        }
        isShock = false;
    }

    IEnumerator ActivateInvincibility()
    {
        if (CatHide.activeSelf)
        {
            CatHide.SetActive(false);
        }
        if (RabbitHide.activeSelf)
        {
            RabbitHide.SetActive(false);
        }

        // 각 body의 SkinnedMeshRenderer에 대해 material을 변경
        for (int i = 0; i < body.Length; i++)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = body[i].GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                // 기존의 모든 메테리얼을 무적 상태 메테리얼로 변경
                Material[] newMaterials = new Material[skinnedMeshRenderer.materials.Length];
                for (int j = 0; j < newMaterials.Length; j++)
                {
                    newMaterials[j] = invincibleMaterial; // 무적 상태 메테리얼로 설정
                }
                skinnedMeshRenderer.materials = newMaterials; // 메테리얼 업데이트
            }
        }

        yield return new WaitForSeconds(5f); // 5초 대기

        // 5초 후에 원래 메테리얼로 복귀
        for (int i = 0; i < body.Length; i++)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = body[i].GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.materials = originalMaterials[i]; // 원래 메테리얼 복귀
            }
        }

        if (!CatHide.activeSelf)
        {
            CatHide.SetActive(true);
        }
        if (!RabbitHide.activeSelf)
        {
            RabbitHide.SetActive(true);
        }
        isInvincibility = false;
    }

    IEnumerator SpeedUp()
    {
        isSpeedUp = true;
        walkSpeed *= 1.5f;
        runSpeed *= 1.5f;

        // 코루틴 시작 및 핸들 저장
        afterSpeedImageCoroutine = StartCoroutine(GenerateAfterImage());

        yield return new WaitForSeconds(5f); // 5초 대기

        // 속도 원상복구
        isSpeedUp = false;
        walkSpeed /= 1.5f;
        runSpeed /= 1.5f;

        // 잔상 코루틴 중지
        if (afterSpeedImageCoroutine != null)
        {
            StopCoroutine(afterSpeedImageCoroutine);
            afterSpeedImageCoroutine = null;
        }
    }

    // 잔상 생성 코루틴
    IEnumerator GenerateAfterImage()
    {
        while (true)
        {
            CreateAfterImage();
            yield return new WaitForSeconds(afterImageInterval);
        }
    }

    void CreateAfterImage()
    {
        foreach (var obj in body)
        {
            SkinnedMeshRenderer skinnedRenderer = obj.GetComponent<SkinnedMeshRenderer>();
            if (skinnedRenderer == null) continue;

            // 새로운 Mesh 복사
            Mesh bakedMesh = new Mesh();
            skinnedRenderer.BakeMesh(bakedMesh);

            // 빈 게임 오브젝트 생성 후 MeshFilter, MeshRenderer 추가
            GameObject afterImage = new GameObject("AfterImage");
            afterImage.transform.position = obj.transform.position;
            afterImage.transform.rotation = obj.transform.rotation;

            MeshFilter meshFilter = afterImage.AddComponent<MeshFilter>();
            meshFilter.mesh = bakedMesh;

            MeshRenderer meshRenderer = afterImage.AddComponent<MeshRenderer>();

            // 원본의 머티리얼 개수에 맞게 잔상 머티리얼 적용
            Material[] originalMaterials = skinnedRenderer.materials;
            Material[] afterImageMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                afterImageMaterials[i] = afterImageMaterial; // 모든 머티리얼을 잔상용 머티리얼로 변경
            }

            meshRenderer.materials = afterImageMaterials; // 머티리얼 배열 적용

            StartCoroutine(FadeAndDestroy(afterImage, afterImageDuration));
        }
    }


    IEnumerator FadeAndDestroy(GameObject afterImage, float duration)
    {
        MeshRenderer meshRenderer = afterImage.GetComponent<MeshRenderer>();
        Material mat = meshRenderer.material;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(afterImage);
    }

    // 넘어지는 애니메이션이 끝난 후 실행
    IEnumerator PushAnimFinished()
    {
        // 순간적으로 맨홀에 닿았을 때 땅에 닿은 것으로 인지함을 방지
        yield return new WaitForSeconds(0.3f);
        if (isGrounded)
        {
            yield return new WaitForSeconds(1.7f);
            isPush = false;
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

        // 달리기 이펙트 
        if (isRunning && !isStaminaDepleted && !isAttack && !isPush)
        {
            if (!isSpeedUp)
            {
                RunEffect.SetActive(true);
                SpeedUpRunEffect.SetActive(false);
            }
            else
            {
                SpeedUpRunEffect.SetActive(true);
                RunEffect.SetActive(false);
            }
        }
        else
        {
            RunEffect.SetActive(false);
            SpeedUpRunEffect.SetActive(false);
        }

        // 입력 방향을 카메라 방향 기준으로 변환
        moveVec = (camForward * vAxis + camRight * hAxis).normalized;

        if (!wallHit && !isStaminaDepleted && !isAttack && !isPush && !isShock)
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
        isRunning = !isShock && !isInvincibility && !isPush && !isStaminaDepleted && runDown && moveVec != Vector3.zero;

        // 달리는 동시에 땅에 있지 않을 때 (또한 넘어지는 중일 때)
        if ((isRunning && !isGrounded && !isAttack) || isPush || isShock)
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

        // 달리지 않을 때 스테미나 회복 (또한 넘어지는 중이 아닐 때)
        if (!isRunning && stamina < maxStamina && !isStaminaDepleted && !isPush && !isShock)
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
        NoStaminaEffect.SetActive(true);
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
        if (isStaminaDepleted || isPush || isShock) return;

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

            // 이펙트 보이기
            var emission = MonkeyPunchEffect.GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = 0;
            MonkeyPunchEffect.SetActive(true);
            MonkeyPunchEffect.GetComponent<ParticleSystem>().Play();


            startPosition = transform.position;
            yield return MoveCharacter(startPosition, transform.forward * 6.4f + Vector3.up * 1.4f, 0.8f);
            MonkeyPunchEffect.SetActive(false);
        }
        else if (characterNum == 3)
        {
            yield return MoveCharacter(startPosition, transform.forward * 3.0f, 0.5f);

            startPosition = transform.position;
            yield return MoveCharacter(startPosition, transform.forward * 5.0f + Vector3.up * 10.4f, 0.8f);

            StartCoroutine(WaitUntilGroundedForRabbit());
            startPosition = transform.position;
            yield return MoveCharacter(startPosition, transform.forward * 5.0f, 0.3f);
            yield return new WaitForSeconds(0.6f);
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
        {
            yield return new WaitForSeconds(MonkeyAttackCooldown);
        }
        else if (characterNum == 3)
        {
            yield return new WaitForSeconds(RabbitAttackCooldown);
        }
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
            if (isPush || isShock)
            {
                yield break;
            }

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

    // 토끼 전용 이펙트 대기 함수 
    private IEnumerator WaitUntilGroundedForRabbit()
    {

        while (!isGrounded)
        {
            yield return null; // 매 프레임마다 확인
        }

        if (isPush)
        {
            yield break;
        }

        if (isRabbitRespawn)
        {
            isRabbitRespawn = false;
            yield break;
        }


        // 조건이 만족되었을 때만 실행
        RabbitPunchEffect.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        RabbitPunchEffect.SetActive(false);
    }

    void Jump()
    {
        // 첫 번째 레이캐스트
        Vector3 frontPosition = transform.position + transform.right * 0.3f;
        bool raycastGrounded1 = Physics.Raycast(frontPosition, Vector3.down, raycastDistance, groundLayer);
        Debug.DrawRay(frontPosition, Vector3.down * raycastDistance, raycastGrounded1 ? Color.green : Color.red, 0.1f);

        // 두 번째 레이캐스트
        frontPosition = transform.position - transform.right * 0.3f;
        bool raycastGrounded2 = Physics.Raycast(frontPosition, Vector3.down, raycastDistance, groundLayer);
        Debug.DrawRay(frontPosition, Vector3.down * raycastDistance, raycastGrounded2 ? Color.green : Color.red, 0.1f);

        // 둘 중 하나라도 땅을 감지하면 isGrounded = true
        isGrounded = raycastGrounded1 || raycastGrounded2;


        if (isStaminaDepleted || isAttack || isPush || isShock) return;

        // float rayRadius = 0.7f; // 아이템 판독
        // isBlock = Physics.SphereCast(transform.position, rayRadius, Vector3.up, out RaycastHit hit, raycastDistance, itemLayer);

        if (isGrounded && jumpDown) // 땅에 있을 때 점프 가능
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;

            StartCoroutine(CheckJumpToFall()); // 점프 상태 확인
        }
    }

    IEnumerator CheckJumpToFall()
    {
        // 점프 후 일정시간 동안 착지하지 못하면 떨어지는 모션 실행 
        float timer = 0f;
        while (timer < 1.7f)
        {
            if (!isJump) yield break; // isJump가 false가 되면 즉시 종료
            timer += Time.deltaTime;
            yield return null;
        }

        // isJump가 true면 JumpToFall 트리거
        anim.SetBool("JumpToFall", true);
        anim.SetBool("isFall", true);
    }

    void Fall()
    {
        // 낙하 애니메이션
        // 거리 측정하여 땅과 근접하다면 낙하 애니메이션 실행 X

        if (isPush) return;

        RaycastHit hit;
        bool isNearGround = Physics.Raycast(transform.position, Vector3.down, out hit, 10f, groundLayer);

        if (!isGrounded && rigid.velocity.y < -0.1f && !isNearGround && !isFalling && !isJump && !isCollidingWithGround)
        {
            StartCoroutine(CheckFallAfterDelay());
        }
    }

    IEnumerator CheckFallAfterDelay()
    {
        // 0.5초 대기 후 아직 공중이라면 떨어지는 모션 실행
        // 땅 모서리에서 떨어지는 현상 방지
        yield return new WaitForSeconds(0.5f); 

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
        if (characterNum == 3) isRabbitRespawn = true;
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
            anim.SetBool("isPush", false);
            anim.SetBool("JumpToFall", false);
            isFalling = false;
            isJump = false;

            if (isPush)
            {
                StartCoroutine(PushAnimFinished());
            }
        }
        if (collision.gameObject.CompareTag("Stone"))
        {
            Destroy(collision.gameObject);
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
        if (collision.gameObject.CompareTag("ConveyorBlock"))
        {
            SpecialBlock conveyor = collision.gameObject.GetComponentInParent<SpecialBlock>();

            if (conveyor != null && conveyor.version == 4 && conveyor.applyForce)
            {
                // 컨베이어 블록의 힘과 방향 가져오기 (X와 Z 바꾸기)
                Vector3 forceDirection = new Vector3(conveyor.direction.z, conveyor.direction.y, -conveyor.direction.x).normalized;
                float forceAmount = conveyor.force;
                // 플레이어에 힘 적용 (중력 영향을 유지)
                rigid.velocity = new Vector3(forceDirection.x * forceAmount, rigid.velocity.y, forceDirection.z * forceAmount);
            }
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 6)
        {
            isCollidingWithGround = false;
        }
        if (collision.gameObject.CompareTag("ConveyorBlock"))
        {
            isCollidingWithGround = false;
            if (isJump)
                // 컨베이어 벨트에서 벗어나면 힘이 더 이상 적용되지 않도록 속도 초기화
                rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
        }
    }
}
