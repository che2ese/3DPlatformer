using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerES : MonoBehaviour
{
    // 걷는 속도
    public float walkSpeed;
    // 달리는 속도
    public float runSpeed;
    // 점프하는 힘
    public float jumpPower;

    // 점프 횟수
    int jumpCount;

    // 플레이어 Rigidbody 컴포넌트
    Rigidbody rb;

    // 걷기 달리기 속도 변경용 변수
    private float defaultSpeed;

    // Start is called before the first frame update
    void Start()
    {
        // 플레이어 Rigidbody 컴포넌트 가져와서 저장
        rb = GetComponent<Rigidbody>();
        // 걷는 속도로 시작
        defaultSpeed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Walk();
        Jump();
    }

    void Walk()
    {
        // 방향키 + wsad 입력 받아 숫자로 저장
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // x축에 h 값, z축에 v 값 넣은 변수 생성
        Vector3 dir = new Vector3(h, 0, v);

        // 대각선 방향 이동 시 속도 동일하도록 정규화
        dir.Normalize();

        // transform.position 안 쓴 이유
        // 점프 및 충돌 등 물리 작용은 Rigidbody 컴포넌트를 통해 구현되었는데
        // 이동을 Transform 컴포넌트로 구현하면 벽에 막힐 경우 문제 발생

        // *Time.deltaTime = 30FPS (1초당 보여주는 프레임 수) 일 경우 1/30초 (모든 기기에서 동일한 속도)
        rb.MovePosition(rb.position + (dir * defaultSpeed * Time.deltaTime));

        Dash();
    }

    void Dash()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            defaultSpeed = runSpeed;
        }
        else
        {
            defaultSpeed = walkSpeed;
        }
    }

    void Jump()
    {
        // 스페이스바 누른 순간 && 점프 횟수 1보다 작으면
        // 2단 점프 넣을 경우 jumpCount < 2 로 변경
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 1)
        {
            // 위로 순간적인 힘 발생
            // AddForce(방향 + 크기 = 벡터, 힘의 방식)
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            // 점프 횟수 1 증가
            jumpCount++;
        }
    }

    // 두 오브젝트 모두 isTrigger가 체크되지 않은 경우 쓰는 OnCollision 함수 (하나라도 체크 시 OnTrigger 함수)
    // 두 함수 모두 매개변수 = 충돌한 오브젝트의 정보 (이름, 태그 둘 다 포함. but, 태그 비교 연산이 가볍다)
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트의 태그가 "Ground" 라면
        if (collision.gameObject.tag == "Ground")
        {
            // 점프 횟수 초기화
            jumpCount = 0;
        }
    }
}
