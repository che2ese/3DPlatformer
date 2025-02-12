using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : MonoBehaviour
{
    [SerializeField]
    private float directionalForce = 100;
    [SerializeField]
    private Vector3 extraForce = new Vector3(0, 100, 0);
    [SerializeField]
    private float torque = 10.0f;
    [SerializeField]
    private List<Rigidbody> elements = new List<Rigidbody>();
    [SerializeField]
    private float lifeTime = 2;
    [SerializeField]
    private AudioSource audioSource; // 오디오 추가
    [SerializeField]
    private GameObject particle;

    // 다른 오브젝트의 BoxCollider를 참조할 변수
    public GameObject targetObject;  // 다른 오브젝트
    private BoxCollider targetBoxCollider; // 해당 오브젝트의 BoxCollider

    private bool hasApplied = false; // 중복 실행 방지

    private void Start()
    {
        // targetObject에서 BoxCollider를 가져옴
        if (targetObject != null)
        {
            targetBoxCollider = targetObject.GetComponent<BoxCollider>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasApplied) return; // 이미 실행되었으면 무시
        if (collision.gameObject.CompareTag("Player")) // 플레이어 태그와 충돌했을 때만 실행
        {
            Apply();
        }
    }
    public void Apply()
    {
        hasApplied = true;

        // 오디오가 있으면 재생
        if (audioSource)
        {
            audioSource.Play();
        }

        foreach (var e in elements)
        {
            e.AddForce((e.transform.position - transform.position) * directionalForce + extraForce, ForceMode.Impulse);
            Vector3 randomRotation = Random.insideUnitSphere;
            e.AddTorque(randomRotation * torque, ForceMode.Impulse);
            e.useGravity = true;
            particle.SetActive(true);
        }

        StartCoroutine(DestroyElements());

        BoxCollider collider = GetComponent<BoxCollider>();
        collider.enabled = false;
        targetBoxCollider.enabled = false;

        Destroy(gameObject, lifeTime * 2);
    }

    IEnumerator DestroyElements()
    {
        yield return new WaitForSeconds(lifeTime);

        float scale = 1;
        List<Vector3> scales = new List<Vector3>();
        foreach (var e in elements)
        {
            scales.Add(e.transform.localScale);
        }

        while (scale > 0.01f)
        {
            scale = Mathf.MoveTowards(scale, 0, Time.deltaTime * 2);
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].transform.localScale = scales[i] * scale;
            }

            yield return null;
        }

        scale = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            Destroy(elements[i]);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }
}
