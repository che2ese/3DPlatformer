using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerES : MonoBehaviour
{
    // �ȴ� �ӵ�
    public float walkSpeed;
    // �޸��� �ӵ�
    public float runSpeed;
    // �����ϴ� ��
    public float jumpPower;

    // ���� Ƚ��
    int jumpCount;

    // �÷��̾� Rigidbody ������Ʈ
    Rigidbody rb;

    // �ȱ� �޸��� �ӵ� ����� ����
    private float defaultSpeed;

    // Start is called before the first frame update
    void Start()
    {
        // �÷��̾� Rigidbody ������Ʈ �����ͼ� ����
        rb = GetComponent<Rigidbody>();
        // �ȴ� �ӵ��� ����
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
        // ����Ű + wsad �Է� �޾� ���ڷ� ����
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // x�࿡ h ��, z�࿡ v �� ���� ���� ����
        Vector3 dir = new Vector3(h, 0, v);

        // �밢�� ���� �̵� �� �ӵ� �����ϵ��� ����ȭ
        dir.Normalize();

        // transform.position �� �� ����
        // ���� �� �浹 �� ���� �ۿ��� Rigidbody ������Ʈ�� ���� �����Ǿ��µ�
        // �̵��� Transform ������Ʈ�� �����ϸ� ���� ���� ��� ���� �߻�

        // *Time.deltaTime = 30FPS (1�ʴ� �����ִ� ������ ��) �� ��� 1/30�� (��� ��⿡�� ������ �ӵ�)
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
        // �����̽��� ���� ���� && ���� Ƚ�� 1���� ������
        // 2�� ���� ���� ��� jumpCount < 2 �� ����
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 1)
        {
            // ���� �������� �� �߻�
            // AddForce(���� + ũ�� = ����, ���� ���)
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            // ���� Ƚ�� 1 ����
            jumpCount++;
        }
    }

    // �� ������Ʈ ��� isTrigger�� üũ���� ���� ��� ���� OnCollision �Լ� (�ϳ��� üũ �� OnTrigger �Լ�)
    // �� �Լ� ��� �Ű����� = �浹�� ������Ʈ�� ���� (�̸�, �±� �� �� ����. but, �±� �� ������ ������)
    private void OnCollisionEnter(Collision collision)
    {
        // �浹�� ������Ʈ�� �±װ� "Ground" ���
        if (collision.gameObject.tag == "Ground")
        {
            // ���� Ƚ�� �ʱ�ȭ
            jumpCount = 0;
        }
    }
}
