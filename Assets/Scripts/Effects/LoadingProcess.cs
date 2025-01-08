using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingProcess : MonoBehaviour
{
    public TMP_Text ProgressIndicator; // 진행률 텍스트
    public Image LoadingBar;           // 로딩 바
    public float currentValue = 0f;   // 현재 진행 값
    private float speed = 20f;                // 진행 속도
    public GameObject Loading;         // 로딩 패널

    // Start is called before the first frame update
    void Start()
    {
        ResetLoading(); // 시작 시 로딩 상태 초기화
    }

    // Update is called once per frame
    void Update()
    {
        if (currentValue < 100)
        {
            currentValue += speed * Time.deltaTime;
            ProgressIndicator.text = ((int)currentValue).ToString() + "%";
        }
        else
        {
            ProgressIndicator.text = "Done";

            if (Loading != null && Loading.activeSelf)
            {
                ResetLoading(); // 로딩 상태 초기화
                Loading.SetActive(false); // 로딩 패널 비활성화
            }
        }

        LoadingBar.fillAmount = currentValue / 100;
    }

    // 로딩 상태 초기화
    public void ResetLoading()
    {
        currentValue = 0f; // 진행 값 초기화
        if (Loading != null)
        {
            Loading.SetActive(true); // 로딩 패널 활성화
        }

        if (ProgressIndicator != null)
        {
            ProgressIndicator.text = "0%"; // 초기 진행률 표시
        }

        if (LoadingBar != null)
        {
            LoadingBar.fillAmount = 0f; // 로딩 바 초기화
        }
    }
}
