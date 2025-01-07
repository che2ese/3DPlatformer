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
        // �̱��� �������� ��ü ����
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� �������� ����
        }
        else
        {
            Destroy(gameObject); // �ߺ� ����
        }
    }
    void OnEnable()
    {
        // �� �ε� �̺�Ʈ�� �޼��� ���
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // �� �ε� �̺�Ʈ���� �޼��� ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� ���� MenuScene�� �� StartBtn �ʱ�ȭ
        if (scene.name == "MainScene")
        {
            GameObject startButton = GameObject.Find("StartBtn");
            if (startButton != null)
            {
                Button btn = startButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners(); // ���� �̺�Ʈ ����
                    btn.onClick.AddListener(() => OnStartButtonClicked("MenuScene")); // ���ο� �̺�Ʈ �߰�
                }
            }
        }
        if (scene.name == "MenuScene")
        {
            GameObject onlineButton = GameObject.Find("Online");
            GameObject stageButton = GameObject.Find("Stage");
            GameObject arcadeButton = GameObject.Find("Arcade");
            if (onlineButton != null)
            {
                Button btn = onlineButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners(); // ���� �̺�Ʈ ����
                    btn.onClick.AddListener(() => OnStartButtonClicked("OnlineScene")); // ���ο� �̺�Ʈ �߰�
                }
            }
            if (stageButton != null)
            {
                Button btn = stageButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners(); // ���� �̺�Ʈ ����
                    btn.onClick.AddListener(() => OnStartButtonClicked("StageScene")); // ���ο� �̺�Ʈ �߰�
                }
            }
            if (arcadeButton != null)
            {
                Button btn = arcadeButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners(); // ���� �̺�Ʈ ����
                    btn.onClick.AddListener(() => OnStartButtonClicked("ArcadeScene")); // ���ο� �̺�Ʈ �߰�
                }
            }
        }
    }
    void Update()
    {
        // ESC Ű �Է� ó��
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "MenuScene")
            {
                SceneManager.LoadScene("MainScene"); // MainScene���� ��ȯ
            }
            if (SceneManager.GetActiveScene().name == "OnlineScene" || SceneManager.GetActiveScene().name == "StageScene" || SceneManager.GetActiveScene().name == "ArcadeScene")
            {
                SceneManager.LoadScene("MenuScene"); // MainScene���� ��ȯ
            }
        }
    }
    void OnStartButtonClicked(string SceneName)
    {
        // StartBtn�� ������ �� MenuScene���� ��ȯ
        SceneManager.LoadScene(SceneName);
    }
}
