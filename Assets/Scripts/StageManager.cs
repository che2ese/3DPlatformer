using UnityEngine;
using System.Collections;

public class StageManager : MonoBehaviour
{
    public int clearStage = 0; // 현재 클리어된 스테이지 값
    public GameObject[] objectsToAppear; // 등장할 오브젝트 배열
    public GameObject[] objectsToDisappear; // 특정 스테이지에서 제거될 오브젝트 배열
    public float appearSpeed = 1.0f; // 등장 애니메이션 속도
    public float appearHeight = 2.0f; // 밑에서 올라오는 높이

    void Start()
    {
        UpdateObjectsVisibility();
    }

    void Update()
    {
        // 0번 키를 누르면 클리어 스테이지를 1 증가
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            clearStage++;
            UpdateObjectsVisibility();
            Debug.Log("ClearStage 증가: " + clearStage);
        }
    }

    public void UpdateObjectsVisibility()
    {
        // 등장하는 오브젝트 처리
        for (int i = 0; i < objectsToAppear.Length; i++)
        {
            if (i < clearStage)
            {
                // 처음 등장하는 경우 부드럽게 나타나도록 애니메이션 실행
                if (!objectsToAppear[i].activeSelf)
                {
                    objectsToAppear[i].SetActive(true);
                    StartCoroutine(AppearEffect(objectsToAppear[i]));
                }
            }
            else
            {
                objectsToAppear[i].SetActive(false);
            }
        }

        // 특정 스테이지에서 사라질 오브젝트 처리
        if (clearStage < objectsToDisappear.Length)
        {
            if (objectsToDisappear[clearStage] != null)
            {
                objectsToDisappear[clearStage].SetActive(false);
                Debug.Log("오브젝트 제거됨: " + objectsToDisappear[clearStage].name);
            }
        }
    }

    IEnumerator AppearEffect(GameObject obj)
    {
        Vector3 originalPos = obj.transform.position;
        Vector3 startPos = new Vector3(originalPos.x, originalPos.y - appearHeight, originalPos.z);
        obj.transform.position = startPos;

        float elapsedTime = 0f;
        while (elapsedTime < appearSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / appearSpeed;
            obj.transform.position = Vector3.Lerp(startPos, originalPos, t);
            yield return null;
        }

        obj.transform.position = originalPos; // 최종 위치 보정
    }
}
