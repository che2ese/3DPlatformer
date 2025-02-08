using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager instance; // **�̱��� �ν��Ͻ�**

    public TMP_InputField idInput, pwInput;
    public TMP_Text errorText;

    private Coroutine currentCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = "C528C";  // ���⿡ �ùٸ� Title ID �Է�

        // ���� ����� �� �ڷ�ƾ�� ����ϵ��� �̺�Ʈ ������ ���
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� ��ȯ�� ��, ���� �ڷ�ƾ�� ���� ���̶�� ����ϰ� null�� �ʱ�ȭ
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        // errorText�� null�� �ƴϸ� �����
        if (errorText != null && errorText.gameObject.activeSelf)
        {
            errorText.gameObject.SetActive(false);
        }
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (SceneManager.GetActiveScene().name == "LoginScene")
            {
                Login();
            }
        }
        HandleTabKey();
    }

    private void HandleTabKey()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (idInput.isFocused)
            {
                // IDInput���� Tab Ű�� ������ PassInput���� �̵�
                pwInput.Select();
            }
            else if (pwInput.isFocused)
            {
                // PassInput���� Tab Ű�� ������ IDInput���� ���ư��� (��ȯ ����)
                idInput.Select();
            }
        }
    }

    // �α��� ��ư Ŭ�� �� ȣ��
    public void Login()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = idInput.text,
            Password = pwInput.text
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    // ȸ������ ��ư Ŭ�� �� ȣ��
    public void Register()
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = idInput.text,
            Password = pwInput.text,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        errorText.gameObject.SetActive(true);
        errorText.text = "ȸ������ ����! �ڵ� �α��� ��...";
        currentCoroutine = StartCoroutine(HideErrorTextAfterDelay());  // 3�� �Ŀ� ���� �޽��� �����

        Login();

        string nickname = idInput.text; // �̸��Ͽ��� �г��� ����

        var updatePlayerProfileRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nickname
        };

    }


    private void OnLoginSuccess(LoginResult result)
    {
        errorText.gameObject.SetActive(true);
        errorText.text = "�α��� ����!";
        currentCoroutine = StartCoroutine(HideErrorTextAfterDelay());  // 3�� �Ŀ� ���� �޽��� �����

        // �̸��� �ּҿ��� �г��� ���� (�̸����� @ �� �κ�)
        string nickname = idInput.text;

        // �÷��̾� �����ʿ� �г��� ����
        var updatePlayerProfileRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nickname
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(updatePlayerProfileRequest,
            result => print("�г��� ���� ����!"),
            error => print("�г��� ���� ����: " + error.GenerateErrorReport()));

        // �α��� �� �÷��̾� ������ ���� ��������
        GetPlayerProfile();
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        // ���� ������ ����Ͽ� ��Ȯ�� ���� �޽��� Ȯ��
        string errorMessage = error.GenerateErrorReport();
        print(errorMessage);

        errorText.gameObject.SetActive(true);

        // �̸��� �ּҰ� �̹� �����ϴ��� üũ
        if (errorMessage.Contains("Email address already exists"))
        {
            errorText.text = "�̹� ���Ե� �̸��� �ּ��Դϴ�.";
        }
        // �̸��� �ּҰ� ��ȿ���� ������ üũ
        else if (errorMessage.Contains("Email address is not valid"))
        {
            errorText.text = "�̸��� ������ Ȯ���� �ּ���.";
        }
        else if (errorMessage.Contains("Password must be between 6 and 100 characters."))
        {
            errorText.text = "6 �ڸ� �̻��� ��й�ȣ�� ������ �ּ���.";
        }
        else
        {
            errorText.text = $"ȸ������ ����: {errorMessage}";
            print($"ȸ������ ����: {errorMessage}");
        }

        currentCoroutine = StartCoroutine(HideErrorTextAfterDelay());  // 3�� �Ŀ� ���� �޽��� �����
    }

    private IEnumerator HideErrorTextAfterDelay()
    {
        yield return new WaitForSeconds(3f);  // 3�� ���

        if (errorText != null) // errorText�� �����ϴ��� Ȯ�� �� ����
        {
            errorText.gameObject.SetActive(false);
        }
    }


    private void OnLoginFailure(PlayFabError error)
    {
        errorText.gameObject.SetActive(true);
        errorText.text = "���̵�� ��й�ȣ�� Ȯ���ϼ���!";
        currentCoroutine = StartCoroutine(HideErrorTextAfterDelay());  // 3�� �Ŀ� ���� �޽��� �����
    }

    // �α��� �� �÷��̾� ������ ���� ��������
    private void GetPlayerProfile()
    {
        var request = new GetPlayerProfileRequest();
        PlayFabClientAPI.GetPlayerProfile(request, OnGetPlayerProfileSuccess, OnGetPlayerProfileFailure);
    }

    private void OnGetPlayerProfileSuccess(GetPlayerProfileResult result)
    {
        if (result.PlayerProfile != null)
        {
            // �⺻�� ����
            string characterSelect = "0";  // �⺻��: 0

            // ����� �����͸� ������ �������� �Լ� ȣ��
            GetUserData();  // �� �Լ����� UserData�� �����ɴϴ�.
        }
        else
        {
            // PlayerProfile�� ���� ��� SelectScene���� �̵�
            Debug.Log("PlayerProfile �����Ͱ� �����ϴ�.");
            SceneManager.LoadScene("SelectScene");
        }
    }

    private void GetUserData()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, OnGetUserDataSuccess, OnGetUserDataFailure);
    }

    private void OnGetUserDataSuccess(GetUserDataResult result)
    {
        string characterSelect = "0";

        if (result.Data != null && result.Data.ContainsKey("characterSelect"))
        {
            characterSelect = result.Data["characterSelect"].Value;
        }

        // ����� �α�: ���� Ȯ���ϱ� ���� ���
        Debug.Log($"CharacterSelect: {characterSelect}");

        // �⺻�������� Ȯ���Ͽ� �� ��ȯ
        if (characterSelect == "0")
        {
            // characterSelect�� 0�̸� SelectScene���� �̵�
            SceneManager.LoadScene("SelectScene");
        }
        else
        {
            // characterSelect�� 0�� �ƴϸ� MainScene���� �̵�
            SceneManager.LoadScene("MainScene");
        }
    }

    private void OnGetUserDataFailure(PlayFabError error)
    {
        Debug.LogError("UserData�� �������� �� �����߽��ϴ�: " + error.GenerateErrorReport());
    }

    private void OnGetPlayerProfileFailure(PlayFabError error)
    {
        print($"������ �ε� ����: {error.GenerateErrorReport()}");
    }
}
