using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAi : MonoBehaviour
{
    //1.Mushroom
    //2.Skeleton
    public int EnemyVersion;

    //몬스터 선택 변수
    [Header("Monster")]
    public Transform pathHolder; //경로 할당
    [HideInInspector]
    public int MonsterNumber; // Skeleton

    // 이동 관련 변수
    [Header("Move")]
    public float speed = 5;
    public float waitTime = .3f;
    public float turnSpeed = 90;

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    //감지거리 관련 변수
    [Header("Sensor")]
    public Light spotlight;// 버섯 전용
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
    

    //몬스터 상태 변수
    public enum MonsterState { Patrolling, Chasing, Attacking }
    public MonsterState currentState = MonsterState.Patrolling;

    Transform player;
    Animator animator;
    Color originalSpotlightColour;

    Vector3[] waypoints;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotlight.spotAngle;
        originalSpotlightColour = spotlight.color;
        animator = GetComponent<Animator>(); // 애니메이터 가져오기
        SetAnimationParameters(); // 애니메이션 설정 적용

        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPathFromIndex(0));

    }

    void Update()
    {
        switch (EnemyVersion) {
            case 1: //mushroom
                switch (currentState)
                {
                    case MonsterState.Patrolling:
                        spotlight.color = originalSpotlightColour;
                        //walk 작동
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", false);
                        if (CanSeePlayer())
                        {
                            spotlight.color = Color.red;
                            currentState = MonsterState.Chasing;
                            StopAllCoroutines();
                        }
                        break;
                    case MonsterState.Chasing:
                        spotlight.color = Color.red;
                        //Run 작동
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", true);
                        if (!CanSeePlayer()) // 플레이어를 놓치면 다시 순찰 상태로 전환
                        {
                            spotlight.color = originalSpotlightColour;
                            currentState = MonsterState.Patrolling;
                            StartCoroutine(ReturnToNearestWaypoint());
                        }
                        else if (Vector3.Distance(transform.position, player.position) < AttackDistance)
                        {
                            currentState = MonsterState.Attacking;
                        }
                        else
                        {
                            ChasePlayer();
                        }
                        break;
                    case MonsterState.Attacking:
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", true);
                        animator.SetBool("isAttack", true);
                        if (!CanSeePlayer())
                        {
                            spotlight.color = originalSpotlightColour;
                            animator.SetBool("isAttack", false);
                            currentState = MonsterState.Patrolling;
                            StartCoroutine(ReturnToNearestWaypoint());
                        }else if(Vector3.Distance(transform.position, player.position) > AttackDistance)
                        {
                            currentState = MonsterState.Chasing;
                            animator.SetBool("isAttack", false);
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
                        if (CanSeePlayer())
                        {
                            spotlight.color = Color.red;
                            currentState = MonsterState.Chasing;
                            StopAllCoroutines();
                        }
                        break;
                    case MonsterState.Chasing:
                        spotlight.color = Color.red;
                        //Run 작동
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", true);
                        if (!CanSeePlayer()) // 플레이어를 놓치면 다시 순찰 상태로 전환
                        {
                            spotlight.color = originalSpotlightColour;
                            currentState = MonsterState.Patrolling;
                            StartCoroutine(ReturnToNearestWaypoint());
                        }
                        else
                        {
                            ChasePlayer();
                        }
                        break;
                }
                break ;
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
        transform.LookAt(targetPosition); // 플레이어 방향으로 회전 (y축 무시)
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
        transform.LookAt(targetWaypoint);

        while (currentState == MonsterState.Patrolling)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);

            if (transform.position == targetWaypoint)
            {
                switch (EnemyVersion)
                {
                    case 1:
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
        float lookAngle = 45f; // 좌우로 회전할 각도
        float lookSpeed = turnSpeed * 0.5f; // 회전 속도 (기본 회전 속도의 절반)

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


        // 방향을 미리 설정
        yield return StartCoroutine(TurnToFace(nearestWaypoint));

        while (Vector3.Distance(transform.position, nearestWaypoint) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, nearestWaypoint, speed * Time.deltaTime);
            yield return null;
        }
        //순찰 루틴을 최근 waypoint부터 시작
        StartCoroutine(FollowPathFromIndex(nearestIndex));
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        //가만히 있게 애니메이션 수정
        animator.SetBool("isWalking", false);
        animator.SetBool("isChasing", false);

        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
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

}
