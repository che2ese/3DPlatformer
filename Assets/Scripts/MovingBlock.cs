using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    public int version;

    [Header("Movement Settings")]
    [HideInInspector] // 버전 1에서만 사용되는 변수는 기본적으로 숨깁니다.
    public Vector3 pos1;
    [HideInInspector]
    public Vector3 pos2;
    [HideInInspector]
    public float speed;
    bool movingToPos2 = true;

    [Header("Scale Settings")]
    [HideInInspector] // 버전 2에서만 사용되는 변수는 기본적으로 숨깁니다.
    public bool changeX = true;
    [HideInInspector]
    public bool changeZ = true;
    [HideInInspector]
    public float scaleDuration = 2f;  // 작아지고 커지는 시간
    [HideInInspector]
    public float minScale = 0.2f;  // 최소 크기 비율 (1/5 크기)
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>(); // 자식 오브젝트들의 원래 크기 저장

    [Header("Visible Settings")]
    [HideInInspector]
    public float initialDisappearDelay = 1f;  // 게임 시작 후 몇 초 뒤에 처음으로 사라질지 설정
    [HideInInspector]
    public float disappearTime = 2f;  // 블록이 사라지는 시간
    [HideInInspector]
    public float reappearTime = 3f;  // 다시 나타나는 시간

    private Renderer blockRenderer;  // 블록의 렌더러

    void Start()
    {
        blockRenderer = GetComponentInChildren<Renderer>();  // 렌더러 컴포넌트를 가져옵니다.

        if (version == 2)
        {
            StoreOriginalScales();
        }
        if (version == 3)
        {
            StartCoroutine(HandleVisibility(initialDisappearDelay));  // 초기 딜레이를 넣어서 Coroutine 시작
        }
    }


    void Update()
    {
        switch (version)
        {
            case 1:
                MoveBetweenTwoPoints();
                break;
            case 2:
                ChangeScale();
                break;
            case 3:
                break;
            default:
                break;
        }
    }

    // 버전 1 : pos1 ↔ pos2 왕복 이동
    void MoveBetweenTwoPoints()
    {
        Vector3 target = movingToPos2 ? pos2 : pos1;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            movingToPos2 = !movingToPos2;
        }
    }

    // 버전 2 : 점점 작아지고, 쉬고, 다시 돌아오는 효과
    void StoreOriginalScales()
    {
        originalScales.Clear();
        foreach (Transform child in transform)
        {
            originalScales[child] = child.localScale;
        }
    }

    void ChangeScale()
    {
        float scaleFactor = Mathf.PingPong(Time.time / scaleDuration, 1f);

        foreach (Transform child in transform)
        {
            if (originalScales.ContainsKey(child))
            {
                Vector3 newScale = originalScales[child]; 
                if (changeX && !changeZ)
                {
                    newScale.x = Mathf.Lerp(originalScales[child].x * minScale, originalScales[child].x, scaleFactor);
                }
                else if(!changeX && changeZ)
                {
                    newScale.z = Mathf.Lerp(originalScales[child].z * minScale, originalScales[child].z, scaleFactor);
                }
                else if(changeX && changeZ)
                {
                    newScale.x = Mathf.Lerp(originalScales[child].x * minScale, originalScales[child].x, scaleFactor);
                    newScale.z = Mathf.Lerp(originalScales[child].z * minScale, originalScales[child].z, scaleFactor);
                }
                child.localScale = newScale;
            }
        }
    }

    // Coroutine으로 블록의 가시성 처리 (게임 시작 후 몇 초 뒤에 사라짐)
    private IEnumerator HandleVisibility(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);  // 게임 시작 후 일정 시간 대기

        while (true)  // 무한 반복
        {
            blockRenderer.enabled = false;  // 블록을 사라지게 함
            yield return new WaitForSeconds(disappearTime);  // 사라지는 시간만큼 대기

            blockRenderer.enabled = true;  // 블록을 다시 나타나게 함
            yield return new WaitForSeconds(reappearTime);  // 나타나는 시간만큼 대기
        }
    }

    // 플레이어가 블록 따라가는 기능 (버전 1에서만 실행)
    private void OnTriggerEnter(Collider other)
    {
        if (version == 1 && other.CompareTag("Player"))
        {
            other.transform.parent = transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (version == 1 && other.CompareTag("Player"))
        {
            other.transform.parent = null;
        }
    }
}
