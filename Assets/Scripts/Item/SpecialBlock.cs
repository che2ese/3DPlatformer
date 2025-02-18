using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialBlock : MonoBehaviour
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
    public bool changeY = true;
    [HideInInspector]
    public float scaleStartDelay = 0f; // 크기 변화 시작 전 대기 시간 (기본값: 0초)
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
    private BoxCollider blockCollider;

    [Header("Conveyor Settings")]
    [HideInInspector]
    public bool applyForce;
    [HideInInspector]
    public float force = 2;
    [HideInInspector]
    public Vector3 direction;
    [HideInInspector]
    public float changeDir = 3f;

    [Header("ManHole Settings")]
    [HideInInspector]
    public GameObject cover;
    private Animator coverAnim;

    [Header("Stone Settings")]
    [HideInInspector]
    public GameObject stonePrefab;
    [HideInInspector]
    public float createTime;
    [HideInInspector]
    public float deleteTime;

    void Start()
    {
        blockRenderer = GetComponentInChildren<Renderer>();  // 렌더러 컴포넌트를 가져옵니다.
        blockCollider = GetComponentInChildren<BoxCollider>();

        if (version == 2)
        {
            StoreOriginalScales();
            StartCoroutine(StartScaleAfterDelay()); // 일정 시간 후 크기 변경 시작
        }
        if (version == 3)
        {
            StartCoroutine(HandleVisibility(initialDisappearDelay));  // 초기 딜레이를 넣어서 Coroutine 시작
        }
        if (version == 4)
        {
            if (applyForce)
            {
                var rb = this.gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }
            StartCoroutine(ChangeDirectionRoutine()); // 3초마다 방향 반전 코루틴 실행
        }
        if(version == 6)
        {
            StartCoroutine(CreateStoneRoutine());
        }
    }


    void Update()
    {
        switch (version)
        {
            case 1:
                MoveBetweenTwoPoints();
                break;
            case 4:
                transform.Rotate(direction * Time.deltaTime);
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
        float timeOffset = scaleDuration; // 시작할 때 큰 상태 유지
        float scaleFactor = Mathf.PingPong((Time.time + timeOffset) / scaleDuration, 1f);
        float clampedScaleFactor = Mathf.Clamp(scaleFactor, minScale, 1f); // 최소 크기 보장

        foreach (Transform child in transform)
        {
            if (originalScales.ContainsKey(child))
            {
                Vector3 newScale = originalScales[child];

                if (changeX)
                    newScale.x = Mathf.Lerp(originalScales[child].x * minScale, originalScales[child].x, clampedScaleFactor);
                if (changeZ)
                    newScale.z = Mathf.Lerp(originalScales[child].z * minScale, originalScales[child].z, clampedScaleFactor);
                if (changeY)
                    newScale.y = Mathf.Lerp(originalScales[child].y * minScale, originalScales[child].y, clampedScaleFactor);

                child.localScale = newScale;
            }
        }
    }


    IEnumerator StartScaleAfterDelay()
    {
        yield return new WaitForSeconds(scaleStartDelay); // 설정된 시간만큼 대기
        while (true)
        {
            ChangeScale();
            yield return null; // 매 프레임 실행
        }
    }

    // Coroutine으로 블록의 가시성 처리 (게임 시작 후 몇 초 뒤에 사라짐)
    private IEnumerator HandleVisibility(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);  // 게임 시작 후 일정 시간 대기

        while (true)  // 무한 반복
        {
            blockRenderer.enabled = false;  // 블록을 사라지게 함
            blockCollider.enabled = false;
            yield return new WaitForSeconds(disappearTime);  // 사라지는 시간만큼 대기

            blockRenderer.enabled = true;  // 블록을 다시 나타나게 함
            blockCollider.enabled = true;
            yield return new WaitForSeconds(reappearTime);  // 나타나는 시간만큼 대기
        }
    }

    private IEnumerator ChangeDirectionRoutine()
    {
        while (version == 4)
        {
            yield return new WaitForSeconds(changeDir); // 3초 대기
            direction = -direction; // 방향 반전
        }
    }

    // 플레이어가 블록 따라가는 기능 (버전 1에서만 실행)
    private void OnTriggerEnter(Collider other)
    {
        if (version == 1 && other.CompareTag("Player"))
        {
            other.transform.parent = transform;
        }
        if (version == 5 && other.CompareTag("Player"))
        {
            coverAnim = cover.GetComponent<Animator>();

            if (coverAnim != null && !coverAnim.GetBool("isUp")) // 이미 실행된 경우 다시 실행되지 않도록 체크
            {
                coverAnim.SetBool("isUp", true); // 애니메이션 실행
                StartCoroutine(ResetCoverAnimation(coverAnim)); // 애니메이션이 끝나면 자동으로 false 처리
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (version == 1 && other.CompareTag("Player"))
        {
            other.transform.parent = null;
        }
    }

    // 애니메이션이 끝나면 자동으로 `false`로 변경하는 코루틴
    IEnumerator ResetCoverAnimation(Animator anim)
    {
        // 애니메이션 길이만큼 대기 후 자동으로 false 변경
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        anim.SetBool("isUp", false);
    }

    IEnumerator CreateStoneRoutine()
    {
        while (true)
        {
            GameObject newStone = Instantiate(stonePrefab, transform.position, Quaternion.identity, this.transform);
            StartCoroutine(DeleteStoneAfterTime(newStone, deleteTime)); // 생성된 스톤을 deleteTime 후 삭제
            yield return new WaitForSeconds(createTime);
        }
    }

    IEnumerator DeleteStoneAfterTime(GameObject stone, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (stone != null)
        {
            Destroy(stone);
        }
    }
}
