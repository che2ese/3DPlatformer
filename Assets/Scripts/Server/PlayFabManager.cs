using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager instance; // **싱글톤 인스턴스**

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
            PlayFabSettings.staticSettings.TitleId = "C528C";  // 여기에 올바른 Title ID 입력

        // 씬이 변경될 때 코루틴을 취소하도록 이벤트 리스너 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 전환될 때, 현재 코루틴이 진행 중이라면 취소하고 null로 초기화
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        // errorText가 null이 아니면 숨기기
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
                // IDInput에서 Tab 키를 누르면 PassInput으로 이동
                pwInput.Select();
            }
            else if (pwInput.isFocused)
            {
                // PassInput에서 Tab 키를 누르면 IDInput으로 돌아가게 (순환 구조)
                idInput.Select();
            }
        }
    }

    // 로그인 버튼 클릭 시 호출
    public void Login()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = idInput.text,
            Password = pwInput.text
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    // 회원가입 버튼 클릭 시 호출
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
        errorText.text = "회원가입 성공! 자동 로그인 중...";
        currentCoroutine = StartCoroutine(HideErrorTextAfterDelay());  // 3초 후에 에러 메시지 숨기기

        Login();

        string nickname = idInput.text; // 이메일에서 닉네임 추출

        var updatePlayerProfileRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nickname
        };

    }


    private void OnLoginSuccess(LoginResult result)
    {
        errorText.gameObject.SetActive(true);
        errorText.text = "로그인 성공!";
        currentCoroutine = StartCoroutine(HideErrorTextAfterDelay());  // 3초 후에 에러 메시지 숨기기

        // 이메일 주소에서 닉네임 추출 (이메일의 @ 앞 부분)
        string nickname = idInput.text;

        // 플레이어 프로필에 닉네임 설정
        var updatePlayerProfileRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nickname
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(updatePlayerProfileRequest,
            result => print("닉네임 설정 성공!"),
            error => print("닉네임 설정 실패: " + error.GenerateErrorReport()));

        // 로그인 후 플레이어 프로필 정보 가져오기
        GetPlayerProfile();
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        // 에러 보고서를 출력하여 정확한 오류 메시지 확인
        string errorMessage = error.GenerateErrorReport();
        print(errorMessage);

        errorText.gameObject.SetActive(true);

        // 이메일 주소가 이미 존재하는지 체크
        if (errorMessage.Contains("Email address already exists"))
        {
            errorText.text = "이미 가입된 이메일 주소입니다.";
        }
        // 이메일 주소가 유효하지 않은지 체크
        else if (errorMessage.Contains("Email address is not valid"))
        {
            errorText.text = "이메일 형식을 확인해 주세요.";
        }
        else if (errorMessage.Contains("Password must be between 6 and 100 characters."))
        {
            errorText.text = "6 자리 이상의 비밀번호를 설정해 주세요.";
        }
        else
        {
            errorText.text = $"회원가입 실패: {errorMessage}";
            print($"회원가입 실패: {errorMessage}");
        }

        currentCoroutine = StartCoroutine(HideErrorTextAfterDelay());  // 3초 후에 에러 메시지 숨기기
    }

    private IEnumerator HideErrorTextAfterDelay()
    {
        yield return new WaitForSeconds(3f);  // 3초 대기

        if (errorText != null) // errorText가 존재하는지 확인 후 접근
        {
            errorText.gameObject.SetActive(false);
        }
    }


    private void OnLoginFailure(PlayFabError error)
    {
        errorText.gameObject.SetActive(true);
        errorText.text = "아이디와 비밀번호를 확인하세요!";
        currentCoroutine = StartCoroutine(HideErrorTextAfterDelay());  // 3초 후에 에러 메시지 숨기기
    }

    // 로그인 후 플레이어 프로필 정보 가져오기
    private void GetPlayerProfile()
    {
        var request = new GetPlayerProfileRequest();
        PlayFabClientAPI.GetPlayerProfile(request, OnGetPlayerProfileSuccess, OnGetPlayerProfileFailure);
    }

    private void OnGetPlayerProfileSuccess(GetPlayerProfileResult result)
    {
        if (result.PlayerProfile != null)
        {
            // 기본값 설정
            string characterSelect = "0";  // 기본값: 0

            // 사용자 데이터를 별도로 가져오는 함수 호출
            GetUserData();  // 이 함수에서 UserData를 가져옵니다.
        }
        else
        {
            // PlayerProfile이 없을 경우 SelectScene으로 이동
            Debug.Log("PlayerProfile 데이터가 없습니다.");
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

        // 디버그 로그: 값을 확인하기 위한 출력
        Debug.Log($"CharacterSelect: {characterSelect}");

        // 기본값인지를 확인하여 씬 전환
        if (characterSelect == "0")
        {
            // characterSelect가 0이면 SelectScene으로 이동
            SceneManager.LoadScene("SelectScene");
        }
        else
        {
            // characterSelect가 0이 아니면 MainScene으로 이동
            SceneManager.LoadScene("MainScene");
        }
    }

    private void OnGetUserDataFailure(PlayFabError error)
    {
        Debug.LogError("UserData를 가져오는 데 실패했습니다: " + error.GenerateErrorReport());
    }

    private void OnGetPlayerProfileFailure(PlayFabError error)
    {
        print($"프로필 로드 실패: {error.GenerateErrorReport()}");
    }
}