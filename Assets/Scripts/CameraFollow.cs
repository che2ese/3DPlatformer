using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject Target;               // 카메라가 따라다닐 타겟

    public float offsetX = 0.0f;            // 카메라의 x좌표
    public float offsetY = 10.0f;           // 카메라의 y좌표
    public float offsetZ = -10.0f;          // 카메라의 z좌표

    public float backOffsetX = 0.0f;            // 카메라의 확장x좌표
    public float backOffsetY = 10.0f;           // 카메라의 확장y좌표
    public float backOffsetZ = -10.0f;          // 카메라의 확장z좌표

    public float CameraSpeed = 2f;       // 카메라 기본 속도
    public float maxCameraSpeed = 6f;    // 카메라 최대 속도
    public float acceleration = 1.3f;       // 가속도

    public LayerMask cameraCollision;

    Vector3 TargetPos;              // 목표 위치
    Vector3 lastTargetPos;          // 타겟의 이전 위치

    void Start()
    {
        lastTargetPos = Target.transform.position; // 초기 위치 저장
    }

    void FixedUpdate()
    {
        // 카메라 이동 속도 다듬기 
        float targetSpeed = (Target.transform.position - lastTargetPos).magnitude / Time.deltaTime;

        if (targetSpeed > 0.01f) // 일정 거리 이상 움직일 때만 가속
        {
            CameraSpeed = Mathf.Min(CameraSpeed + acceleration * Time.deltaTime, maxCameraSpeed);
        }
        else // 타겟이 멈추면 속도 초기화
        {
            CameraSpeed = 2f;
        }

        lastTargetPos = Target.transform.position; // 현재 위치 저장

        // 타겟의 x, y, z 좌표에 카메라의 좌표를 더하여 카메라의 위치를 결정
        if (Vector3.Dot(Target.transform.forward, Vector3.back) > 0.7f)
        {
            CameraSpeed = 1.3f;

            TargetPos = new Vector3(
            Target.transform.position.x + backOffsetX,
            Target.transform.position.y + backOffsetY,
            Target.transform.position.z + backOffsetZ
            );
        }
        else
        {
            TargetPos = new Vector3(
           Target.transform.position.x + offsetX,
           Target.transform.position.y + offsetY,
           Target.transform.position.z + offsetZ
           );
        }

        TargetPos = AdjustCameraPosition(Target.transform.position, TargetPos);

        // 카메라의 움직임을 부드럽게 하는 함수(Lerp)
        transform.position = Vector3.Lerp(transform.position, TargetPos, Time.deltaTime * CameraSpeed);
    }

    Vector3 AdjustCameraPosition(Vector3 targetPosition, Vector3 desiredCameraPos)
    {
        RaycastHit hit;
        Vector3 direction = desiredCameraPos - targetPosition;

        // 플레이어에서 카메라로 향하는 Raycast 발사
        if (Physics.Raycast(targetPosition, direction.normalized, out hit, direction.magnitude, cameraCollision))
        {
            // 벽을 뚫지 않도록 충돌 지점으로 이동 (약간 여유를 둠)
            return hit.point + (hit.normal * 0.2f);
        }

        return desiredCameraPos; // 충돌이 없으면 원래 위치 유지
    }
}
