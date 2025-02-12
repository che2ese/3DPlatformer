using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAi : MonoBehaviour
{
    //1.Mushroom
    //2.Skeleton
    public int EnemyVersion;

    //���� ���� ����
    [Header("Monster")]
    public Transform pathHolder; //��� �Ҵ�
    [HideInInspector]
    public int MonsterNumber; // Skeleton

    // �̵� ���� ����
    [Header("Move")]

    public float speed = 5;
    public float waitTime = .3f;
    public float turnSpeed = 90;

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    //�����Ÿ� ���� ����
    [Header("Sensor")]
    public Light spotlight;// ���� ����
    public LayerMask viewMask;
    public float viewDistance;
    public float viewAngle;
    public float AttackDistance;

    //���� ���� ����
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
        animator = GetComponent<Animator>(); // �ִϸ����� ��������
        SetAnimationParameters(); // �ִϸ��̼� ���� ����

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
                        //walk �۵�
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
                        //Run �۵�
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", true);
                        animator.SetFloat("DistanceToPlayer", 3);
                        if (!CanSeePlayer()) // �÷��̾ ��ġ�� �ٽ� ���� ���·� ��ȯ
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
                        animator.SetFloat("DistanceToPlayer", 3);
                        if (!CanSeePlayer())
                        {
                            spotlight.color = originalSpotlightColour;
                            currentState = MonsterState.Patrolling;
                            StartCoroutine(ReturnToNearestWaypoint());
                        }else if(Vector3.Distance(transform.position, player.position) > AttackDistance)
                        {
                            currentState = MonsterState.Chasing;
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
                        //walk �۵�
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
                        //Run �۵�
                        animator.SetBool("isWalking", true);
                        animator.SetBool("isChasing", true);
                        if (!CanSeePlayer()) // �÷��̾ ��ġ�� �ٽ� ���� ���·� ��ȯ
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
        animator.SetBool("isChasing", true); // ���� �ִϸ��̼� ����
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(player); // �÷��̾� �������� ȸ��
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
    //void TryAttack()
    //{
    //    if (Vector3.Distance(transform.position, player.position) < AttackDistance)
    //    {
    //        currentState = MonsterState.Attacking;
    //        animator.SetTrigger("Attack"); // ���� �ִϸ��̼� ����
    //    }
    //}
    void SetAnimationParameters()
    {
        //if (EnemyVersion == 1) // Mushroom
        //{
        //    animator.SetFloat("SpeedMultiplier", 1.0f); // �⺻ �ӵ�
        //}
        //else if (EnemyVersion == 2) // Skeleton
        //{
        //    animator.SetFloat("SpeedMultiplier", 1.5f); // �� �� ���� �ӵ�
        //}
    }

    IEnumerator FollowPathFromIndex(int startIndex)
    {
        int targetWaypointIndex = (startIndex + 1) % waypoints.Length;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        while (currentState == MonsterState.Patrolling)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);

            if (transform.position == targetWaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint));
            }

            if (CanSeePlayer()) // �÷��̾ �����ϸ� ���� ���·� ��ȯ
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
            Debug.LogWarning(" waypoints �迭�� ��� �ֽ��ϴ�.");
            return 0; // �⺻�� ��ȯ
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
    IEnumerator ReturnToNearestWaypoint()
    {
        int nearestIndex = GetNearestWaypointIndex();
        Vector3 nearestWaypoint = waypoints[nearestIndex];


        // ������ �̸� ����
        yield return StartCoroutine(TurnToFace(nearestWaypoint));

        while (Vector3.Distance(transform.position, nearestWaypoint) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, nearestWaypoint, speed * Time.deltaTime);
            yield return null;
        }
        //���� ��ƾ�� �ֱ� waypoint���� ����
        StartCoroutine(FollowPathFromIndex(nearestIndex));
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        //������ �ְ� �ִϸ��̼� ����
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
        // ���� ���� (�ʷϻ�)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // ���� ���� (������)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackDistance);

        // �þ߰� ǥ�� (�Ķ���)
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewDistance;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }

}
