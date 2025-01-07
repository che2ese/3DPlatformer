using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;

    void Awake()
    {
        // 싱글톤 패턴으로 객체 유지
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 삭제되지 않음
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }
    void OnEnable()
    {
        // 씬 로드 이벤트에 메서드 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 씬 로드 이벤트에서 메서드 제거
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 현재 씬이 MenuScene일 때 StartBtn 초기화
        if (scene.name == "MainScene")
        {
            GameObject startButton = GameObject.Find("StartBtn");
            if (startButton != null)
            {
                Button btn = startButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners(); // 기존 이벤트 제거
                    btn.onClick.AddListener(() => OnStartButtonClicked("MenuScene")); // 새로운 이벤트 추가
                }
            }
        }
    }
    void Update()
    {
        // ESC 키 입력 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "MenuScene")
            {
                SceneManager.LoadScene("MainScene"); // MainScene으로 전환
            }
        }
    }
    void OnStartButtonClicked(string SceneName)
    {
        // StartBtn이 눌렸을 때 MenuScene으로 전환
        SceneManager.LoadScene(SceneName);
    }
}
