using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAi : MonoBehaviour
{
    //1.Mushroom
    //2.Skeleton
    //3.Ghost y축 값 0.6으로 설정해야함
    public int EnemyVersion;

    //몬스터 선택 변수
    [Header("Monster")]
    public Transform pathHolder; //경로 할당
    [HideInInspector]
    public int MonsterNumber; // Skeleton

    // 이동 관련 변수
    [Header("Move")]
    public float speed = 5;
    public float waitTime = 0.1f;
    public float turnSpeed = 90;
    public float rotateSpeed = 1f; // 회전 속도 (높을수록 빠름)

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    //감지거리 관련 변수
    [Header("Sensor")]
    public Light spotlight;
    public LayerMask viewMask;
    public float viewDistance;
    public float viewAngle;
    public float AttackDistance;
    //이펙트
    [Header("Effects")]
    //[HideInInspector]
    public GameObject Mush_jump_Effect;
    //[HideInInspector]
    public GameObject Mush_Attack_Effect;
    public GameObject Skull_Find_Effect;
    public GameObject Skull_Run_Effect;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f; // 공격 후 대기 시간
    private bool canAttack = true; // 공격 가능 여부
    private bool isAttacking = false; // 공격 중인지 확인하는 변수
    [Header("Chase Settings")]
    public float lostPlayerTime = 0.5f; // 플레이어를 놓친 후 Patrolling으로 전환되는 시간
    private float lostPlayerTimer = 0;  // 타이머
    private Vector3 lastSeenPlayerPosition;

    private int frameCounter = 0;
    private GameObject activeMushJumpEffect; // 활성화된 이펙트 인스턴스
    private GameObject activeMushAttackEffect; // 공격 이펙트 인스턴스
    //몬스터 상태 변수
    public enum MonsterState { Patrolling, Chasing, Attacking }
    public MonsterState currentState = MonsterState.Patrolling;

    public GameObject AttackMarkerPrefab; // 씬에 표시할 마커 프리팹
    Transform player;
    Animator animator;
    Color originalSpotlightColour;

    Vector3[] waypoints;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spotlight.spotAngle = viewAngle;
        originalSpotlightColour = spotlight.color;
        animator = GetComponent<Animator>(); // 애니메이터 가져오기
        SetAnimationParameters(); // 애니메이션 설정 적용

        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        // 이펙트를 미리 생성하고 비활성화
        if (Mush_jump_Effect != null)
        {
            Vector3 effectPosition = transform.position;
            effectPosition.y -= 1f; // Y축을 -1만큼 낮춤
            activeMushJumpEffect = Instantiate(Mush_jump_Effect, effectPosition, Quaternion.identity);
            activeMushJumpEffect.transform.SetParent(transform); // 몬스터에 붙여 이동 동기화
            activeMushJumpEffect.SetActive(false); // 처음에는 비활성화
        }
        // 공격 이펙트 생성 및 비활성화
        if (Mush_Attack_Effect != null)
        {
            activeMushAttackEffect = Instantiate(Mush_Attack_Effect, transform.position, Quaternion.identity);
            activeMushAttackEffect.transform.SetParent(transform);
            activeMushAttackEffect.SetActive(false);
        }
        StartCoroutine(FollowPathFromIndex(0));
    }

    void Update()
    {
        switch (EnemyVersion)
        {
            case 1: //mushroom
                switch (currentState)
                {
                    case MonsterState.Patrolling:
                        if (isAttacking) break; // 공격 중이면 이동 로직 무시
                        spotlight.color = originalSpotlightColour;
                        //walk 작동
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", false);
                        if (CanSeePlayer())
                        {
                            spotlight.color = Color.red;
                            currentState = MonsterState.Chasing;
                            StopAllCoroutines();
                            frameCounter = 0; // Chasing으로 전환 시 초기화
                        }
                        SetMushJumpEffectActive(false); // 점프 이펙트 비활성화
                        SetMushAttackEffectActive(false); // 공격 이펙트 비활성화
                        // 프레임 카운터 리셋
                        frameCounter = 0;
                        break;
                    case MonsterState.Chasing:
                        if (isAttacking) break; // 공격 중이면 이동 로직 무시
                        spotlight.color = Color.red;
                        //Run 작동
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", true);

                        if (frameCounter % 75 == 0) // 1초마다 이펙트 활성화/재생
                        {
                            Debug.Log("이펙트 활성화/재생 시도");
                            SetMushJumpEffectActive(true);
                            PlayJumpParticle();
                        }
                        if (CanSeePlayer())
                        {
                            lostPlayerTimer = 0; // 플레이어를 보면 타이머 리셋
                            ChasePlayer();
                        }
                        else
                        {
                            lostPlayerTimer += Time.deltaTime;

                            if (lostPlayerTimer >= lostPlayerTime) // 일정 시간 동안 플레이어를 못 찾으면
                            {
                                spotlight.color = originalSpotlightColour;
                                currentState = MonsterState.Patrolling;
                                StartCoroutine(ReturnToNearestWaypoint()); // 가장 가까운 waypoint로 이동
                            }
                        }
                        frameCounter++;
                        if (!CanSeePlayer()) // 플레이어를 놓치면 다시 순찰 상태로 전환
                        {
                            spotlight.color = originalSpotlightColour;
                            Debug.Log("Chasing -> Patrolling 전환");
                            currentState = MonsterState.Patrolling;
                            SetMushJumpEffectActive(false); // 점프 이펙트 비활성화
                            SetMushAttackEffectActive(false); // 공격 이펙트 비활성화
                            StartCoroutine(ReturnToNearestWaypoint());
                        }
                        else if (Vector3.Distance(transform.position, player.position) < AttackDistance)
                        {
                            Debug.Log("Chasing -> Attacking 전환");
                            currentState = MonsterState.Attacking;
                            SetMushJumpEffectActive(false); // 점프 이펙트 비활성화
                            SetMushAttackEffectActive(true); // 공격 이펙트 활성화
                            frameCounter = 0; // Attacking으로 전환 시 초기화
                        }
                        else
                        {
                            ChasePlayer();
                        }
                        break;
                    case MonsterState.Attacking:
                        if (!canAttack) break; // 쿨타임 중이면 공격 불가
                        if (!isAttacking)
                        {
                            canAttack = false;
                            animator.SetBool("isAttack", true);
                            StartCoroutine(AttackJumpSequence());
                        }
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", true);
                        animator.SetBool("isAttack", true);
                        if (frameCounter % 75 == 0) // 1초마다 공격 이펙트 활성화/재생
                        {
                            Debug.Log("공격 이펙트 활성화/재생 시도");
                            SetMushAttackEffectActive(true);
                            StartCoroutine(AttackJumpSequence());
                            PlayAttackParticle();
                        }
                        frameCounter++;
                        if (!CanSeePlayer())
                        {
                            spotlight.color = originalSpotlightColour;
                            animator.SetBool("isAttack", false);
                            currentState = MonsterState.Patrolling;
                            SetMushJumpEffectActive(false); // 점프 이펙트 비활성화
                            SetMushAttackEffectActive(false); // 공격 이펙트 비활성화
                            StartCoroutine(ReturnToNearestWaypoint());
                        }
                        else if (Vector3.Distance(transform.position, player.position) > AttackDistance)
                        {
                            currentState = MonsterState.Chasing;
                            animator.SetBool("isAttack", false);
                            SetMushAttackEffectActive(false); // 공격 이펙트 비활성화
                            SetMushJumpEffectActive(true); // 점프 이펙트 활성화
                            frameCounter = 0; // Chasing으로 돌아올 때 초기화
                        }
                        else
                        {
                            ChasePlayer();
                        }
                        break;
                }
                break;
            case 2: //skeleton
                switch (currentState)
                {
                    case MonsterState.Patrolling:
                        spotlight.color = originalSpotlightColour;
                        //walk 작동
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", false);
                        speed = 3f;
                        if (CanSeePlayer())
                        {
                            spotlight.color = Color.red;
                            currentState = MonsterState.Chasing;
                            StopAllCoroutines();
                            frameCounter = 0; // Chasing으로 전환 시 초기화
                        }
                        frameCounter = 0;
                        break;
                    case MonsterState.Chasing:
                        spotlight.color = Color.red;
                        //Run 작동
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", true);
                        speed = 5f;
                        if (CanSeePlayer())
                        {
                            ChasePlayer();
                        }
                        else
                        {
                            spotlight.color = originalSpotlightColour;
                            currentState = MonsterState.Patrolling;
                            StartCoroutine(ReturnToNearestWaypoint());
                        }
                        break;
                }
                break;
            case 3://Ghost
                switch (currentState)
                {
                    case MonsterState.Patrolling:
                        spotlight.color = originalSpotlightColour;
                        
                        speed = 2f;
                        if (CanSeePlayer())
                        {
                            spotlight.color = Color.red;
                            currentState = MonsterState.Chasing;
                            StopAllCoroutines();
                            frameCounter = 0; // Chasing으로 전환 시 초기화
                        }
                        frameCounter = 0;
                        break;
                    case MonsterState.Chasing:
                        spotlight.color = Color.red;
                        
                        speed = 4f;
                        if (CanSeePlayer())
                        {
                            ChasePlayer();
                        }
                        else
                        {
                            spotlight.color = originalSpotlightColour;
                            currentState = MonsterState.Patrolling;
                            StartCoroutine(ReturnToNearestWaypoint());
                        }
                        break;
                }
                break;
        }

    }
    void ChasePlayer()
    {
        animator.SetBool("isChasing", true); // 추적 애니메이션 실행

        // y축 값을 고정
        float fixedY = transform.position.y; // 현재 y축 값을 고정
        Vector3 targetPosition = new Vector3(player.position.x, fixedY, player.position.z);

        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // XZ 평면에서만 회전하도록 설정
        Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
        if (lookDirection != Vector3.zero) // 방향이 0이 아닐 때만 회전
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if (angleBetweenGuardAndPlayer < viewAngle / 2f)
            {
                if (!Physics.Linecast(transform.position, player.position, viewMask))
                {
                    return true;
                }
            }
        }
        return false;
    }
    void SetAnimationParameters()
    {
        //    if (EnemyVersion == 1) // Mushroom
        //    {
        //        speed = 8f;
        //    }
        //    else if (EnemyVersion == 2) // Skeleton
        //    {
        //        speed = 8f;
        //    }
    }

    IEnumerator FollowPathFromIndex(int startIndex)
    {
        yield return StartCoroutine(LookAround()); //두리번 거리는 효과
        int targetWaypointIndex = (startIndex + 1) % waypoints.Length;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        yield return StartCoroutine(TurnToFace(targetWaypoint));

        while (currentState == MonsterState.Patrolling)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);

            if (transform.position == targetWaypoint)
            {
                switch (EnemyVersion)
                {
                    case 1://Mush
                        targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                        targetWaypoint = waypoints[targetWaypointIndex];
                        yield return new WaitForSeconds(waitTime);
                        yield return StartCoroutine(LookAround()); //두리번 거리는 효과
                        yield return StartCoroutine(TurnToFace(targetWaypoint));
                        break;
                    case 2: //skeleton
                        targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                        targetWaypoint = waypoints[targetWaypointIndex];
                        yield return new WaitForSeconds(waitTime);
                        yield return StartCoroutine(LookAround()); //두리번 거리는 효과
                        yield return StartCoroutine(TurnToFace(targetWaypoint));
                        break;
                    case 3://Mush
                        targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                        targetWaypoint = waypoints[targetWaypointIndex];
                        yield return new WaitForSeconds(waitTime);
                        yield return StartCoroutine(LookAround()); //두리번 거리는 효과
                        yield return StartCoroutine(TurnToFace(targetWaypoint));
                        break;
                }

            }

            if (CanSeePlayer()) // 플레이어를 감지하면 추적 상태로 전환
            {
                spotlight.color = Color.red;
                currentState = MonsterState.Chasing;
                yield break;
            }

            yield return null;
        }
    }
    int GetNearestWaypointIndex()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning(" waypoints 배열이 비어 있습니다.");
            return 0; // 기본값 반환
        }

        int nearestIndex = 0;
        float minDistance = Vector3.Distance(transform.position, waypoints[0]);

        for (int i = 1; i < waypoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, waypoints[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }
    IEnumerator LookAround()
    {
        // 좌우로 두리번거리는 효과를 위한 회전 각도 설정
        float lookAngle = 30f; // 좌우로 회전할 각도
        float lookSpeed = turnSpeed * 1.5f; // 회전 속도 (기본 회전 속도의 절반)

        // 오른쪽으로 회전
        float targetAngle = transform.eulerAngles.y + lookAngle;
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, lookSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }

        // 잠시 대기
        yield return new WaitForSeconds(0.5f);

        // 왼쪽으로 회전
        targetAngle = transform.eulerAngles.y - lookAngle * 2; // 왼쪽으로 더 회전
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, lookSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }

        // 잠시 대기
        yield return new WaitForSeconds(0.5f);

        // 원래 방향으로 복귀
        targetAngle = transform.eulerAngles.y + lookAngle; // 오른쪽으로 다시 회전
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, lookSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    IEnumerator ReturnToNearestWaypoint()
    {
        int nearestIndex = GetNearestWaypointIndex();
        Vector3 nearestWaypoint = waypoints[nearestIndex];

        // 방향을 천천히 조절하기 위한 목표 회전 설정
        Vector3 dirToWaypoint = (nearestWaypoint - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dirToWaypoint);

        // 회전하면서 waypoint로 이동
        while (Vector3.Distance(transform.position, nearestWaypoint) > 0.1f)
        {
            // 현재 회전에서 목표 회전으로 부드럽게 보정
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

            // waypoint로 부드럽게 이동
            transform.position = Vector3.MoveTowards(transform.position, nearestWaypoint, speed * Time.deltaTime);
            yield return null;
        }

        // 최종적으로 waypoint 방향을 정확하게 바라보도록 설정
        transform.rotation = targetRotation;

        // 순찰 루틴 재개
        StartCoroutine(FollowPathFromIndex(nearestIndex));
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isChasing", false);

        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dirToLookTarget);

        float elapsedTime = 0f;
        float rotationDuration = 0.5f; // 회전에 걸리는 시간 (0.5초)

        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime * rotateSpeed;
            yield return null;
        }

        // 최종적으로 목표 회전값 설정 (완벽히 정렬되도록)
        transform.rotation = targetRotation;
    }

    void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
    void OnDrawGizmosSelected()
    {
        // 감지 범위 (초록색)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // 공격 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackDistance);

        // 시야각 표시 (파란색)
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewDistance;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
    void SetMushJumpEffectActive(bool active)
    {
        if (activeMushJumpEffect != null)
        {
            activeMushJumpEffect.SetActive(active);
            if (active)
            {
                PlayJumpParticle();
                Debug.Log("Mush_jump_Effect 활성화");
            }
            else
            {
                Debug.Log("Mush_jump_Effect 비활성화");
            }
        }
    }

    void SetMushAttackEffectActive(bool active)
    {
        if (activeMushAttackEffect != null)
        {
            activeMushAttackEffect.SetActive(active);
            if (active)
            {
                PlayAttackParticle();
                Debug.Log("Mush_Attack_Effect 활성화");
            }
            else
            {
                Debug.Log("Mush_Attack_Effect 비활성화");
            }
        }
    }

    void PlayJumpParticle()
    {
        if (activeMushJumpEffect != null)
        {
            ParticleSystem ps = activeMushJumpEffect.GetComponent<ParticleSystem>();
            if (ps != null && !ps.isPlaying)
            {
                ps.Play();
                Debug.Log("Mush_jump_Effect 재생");
            }
        }
    }

    void PlayAttackParticle()
    {
        if (activeMushAttackEffect != null)
        {
            ParticleSystem ps = activeMushAttackEffect.GetComponent<ParticleSystem>();
            if (ps != null && !ps.isPlaying)
            {
                ps.Play();
                Debug.Log("Mush_Attack_Effect 재생");
            }
        }
    }
    IEnumerator AttackJumpSequence()
    {
         isAttacking = true; // 공격 시작

        // 1. 점프 전 대기 및 공격 위치 표시
        yield return new WaitForSeconds(0.5f);

        // 2. 공격 위치 계산 및 마커 표시, 마커위치를 바라보게 하는 변수 추가
        Vector3 attackPosition = new Vector3(player.position.x, 0.7f, player.position.z);
        Vector3 markerPosition = attackPosition+ new Vector3(0, 0.25f, 0);

        yield return StartCoroutine(TurnToFace(markerPosition));

        // AttackMarkerPrefab을 공격 지점에 생성
        GameObject attackMarker = Instantiate(AttackMarkerPrefab, attackPosition, Quaternion.identity);
        attackMarker.SetActive(true);

        // 3. 경고 마커가 보이는 시간 (1초 유지)
        yield return new WaitForSeconds(0.5f);

        // 4. 점프 애니메이션 실행
        SetMushJumpEffectActive(true);

        // 5. 포물선 점프 구현
        float jumpHeight = 4.0f;
        float slowUpDuration = 0.3f;  // 천천히 상승하는 시간
        float peakHoldTime = 0.15f;    // 정점에서 멈추는 시간
        float fastDownDuration = 0.1f; // 빠르게 하강하는 시간
        float elapsedTime = 0;

        Vector3 startPosition = transform.position;
        Vector3 peakPosition = (startPosition + attackPosition) / 2 + new Vector3(0, jumpHeight, 0);


        // 천천히 상승 (Ease-in 적용)
        while (elapsedTime < slowUpDuration)
        {
            float t = elapsedTime / slowUpDuration;
            t = t * t; // Ease-in 적용 (느리게 시작)
            transform.position = Vector3.Lerp(startPosition, peakPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 정점에서 잠깐 머무름
        yield return new WaitForSeconds(peakHoldTime);

        elapsedTime = 0;

        // 빠르게 하강 (Ease-out 적용)
        while (elapsedTime < fastDownDuration)
        {
            float t = elapsedTime / fastDownDuration;
            t = 1 - (1 - t) * (1 - t); // Ease-out 적용 (빠르게 끝남)
            transform.position = Vector3.Lerp(peakPosition, attackPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 착지 후 이펙트 및 공격 처리
        SetMushJumpEffectActive(false);
        Destroy(attackMarker); // 마커 제거

        // 7. 공격 후 대기 (쿨타임 적용)
        StartCoroutine(ResetAttackCooldown());
        currentState = MonsterState.Chasing;
        animator.SetBool("isAttack", false);
    }

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true; // 쿨타임 종료 후 다시 공격 가능
        isAttacking = false;
    }
}