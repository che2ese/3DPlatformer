using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomAi : MonoBehaviour
{
    //Speed&DelayTime
    public float speed = 5;
    public float waitTime = .3f;
    public float turnSpeed = 90;
    public float attackRange = 2f;

    public Transform pathHolder; //경로 할당
    Transform player;
    Color originalSpotlightColour;

    public Light spotlight;
    public float viewDistance;
    public LayerMask viewMask;
    float viewAngle;

    private bool hasSpottedPlayer = false; // 플레이어 발견 여부
    Animator anim;


    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();
        viewAngle = spotlight.spotAngle;
        originalSpotlightColour = spotlight.color;

        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0;i<waypoints.Length;i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x , transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(waypoints));//코루틴 시작
    }
    void Update()
    {
        bool canSee = CanSeePlayer();

        if (canSee)
        {
            spotlight.color = Color.red;
            hasSpottedPlayer = true;
        }
        else
        {
            spotlight.color = originalSpotlightColour;
            hasSpottedPlayer = false;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        anim.SetFloat("distanceToPlayer", distanceToPlayer);

        if (distanceToPlayer < attackRange)
        {
            anim.SetBool("isAttacking", true);
            anim.SetBool("isMoving", false);
        }
        else if (hasSpottedPlayer)
        {
            anim.SetBool("isAttacking", false);
            anim.SetBool("isMoving", true);
            FollowPlayer(); // 여기에서 이동 실행
        }
        else
        {
            anim.SetBool("isMoving", true);
            anim.SetBool("isAttacking", false);
        }
    }

    void FollowPlayer()
    {
        if (!hasSpottedPlayer) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < attackRange)
        {
            anim.SetBool("isAttacking", true);
            anim.SetBool("isMoving", false);
            return;
        }

        // 방향 계산
        Vector3 direction = (player.position - transform.position).normalized;

        // 현재 높이에서 점프 애니메이션 적용
        float baseHeight = transform.position.y;
        float jumpHeight = 0.5f; // 점프 높이 (조정 가능)
        float jumpSpeed = 3f; // 점프 속도 (조정 가능)

        float yOffset = Mathf.Sin(Time.time * jumpSpeed) * jumpHeight; // 포물선 이동

        // 이동 적용
        Vector3 movePos = transform.position + direction * speed * Time.deltaTime;
        movePos.y = baseHeight + yOffset; // 기본 높이에 점프 값 추가

        transform.position = movePos;
        transform.LookAt(player.position);
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

    IEnumerator FollowPath(Vector3[] waypoints)
    {
        transform.position = waypoints[0];
        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        float jumpHeight = 0.5f; // 점프 높이
        float jumpSpeed = 3f; // 점프 속도
        float baseHeight = transform.position.y; // 기본 높이 유지
        //float jumpdistance = 2f;

        while (true)
        {
            float yOffset = Mathf.Sin(Time.time * jumpSpeed) * jumpHeight; // 부드러운 점프

            Vector3 nextPos = Vector3.Lerp(transform.position, targetWaypoint, speed * Time.deltaTime);
            nextPos.y = baseHeight + yOffset; // y값 고정된 범위에서 점프 적용

            transform.position = nextPos;

            if (Vector3.Distance(transform.position, targetWaypoint) < 0.1f)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint));
            }
            yield return null;
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x)*Mathf.Rad2Deg;

        while(Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle))>0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y,targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;
        foreach(Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f); // Make sphere on waypoint
            Gizmos.DrawLine(previousPosition, waypoint.position); // draw line which start as prePos to waypointPosition
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition); // make line where last waypoint to start position


        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
}
