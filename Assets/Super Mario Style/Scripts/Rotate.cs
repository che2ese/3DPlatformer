using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public bool applyForce;
    public float force = 2;
    public Vector3 direction;

    private void Start()
    {
        if (applyForce)
        {
            var rb = this.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        transform.Rotate(direction * Time.deltaTime);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (applyForce && collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // 현재 오브젝트의 Y축 회전 각도 가져오기
                float yRotation = transform.eulerAngles.y;

                Vector3 forceDir;

                // Y축 회전이 90도 또는 270도에 가까우면 X축으로 힘 적용, 그렇지 않으면 Z축으로 힘 적용
                if (Mathf.Abs(yRotation % 180 - 90) < 10) // 90도 또는 270도 근처
                {
                    forceDir = new Vector3(-direction.x, 0, 0); // X축으로 힘 적용
                }
                else // 0도 또는 180도 근처
                {
                    forceDir = new Vector3(0, 0, -direction.x); // Z축으로 힘 적용
                }

                // 힘 적용
                playerRb.AddForce(forceDir * force, ForceMode.Acceleration);
            }
        }
    }
}