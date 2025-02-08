using System.Collections;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class UserInfoManager : MonoBehaviour
{
    public static UserInfoManager instance;

    private string nickname;
    private int character;
    private int money;

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

        // 게임 시작 시 플레이어 정보를 불러옵니다.
        LoadUserInfoFromPlayFab();
    }

    public void SetUserInfo(string nickname, int character, int money)
    {
        this.nickname = nickname;
        this.character = character;
        this.money = money;

        // 값이 갱신될 때마다 디버그 출력
        Debug.Log($"UserInfoManager: 닉네임={nickname}, 캐릭터={character}, 돈={money}");

        // 값 저장 (PlayerPrefs 사용)
        SaveUserInfo();
    }

    public string GetNickname() => nickname;
    public int GetCharacter() => character;
    public int GetMoney() => money;

    // 유저 정보를 저장하는 메서드 (PlayerPrefs 사용)
    private void SaveUserInfo()
    {
        PlayerPrefs.SetString("Nickname", nickname);
        PlayerPrefs.SetInt("Character", character);
        PlayerPrefs.SetInt("Money", money);
        PlayerPrefs.Save();
    }

    // 유저 정보를 불러오는 메서드 (PlayFab에서 데이터를 가져오기)
    private void LoadUserInfoFromPlayFab()
    {
        var request = new GetPlayerProfileRequest();
        PlayFabClientAPI.GetPlayerProfile(request, OnGetPlayerProfileSuccess, OnGetPlayerProfileFailure);
    }

    // PlayFab에서 프로필 정보가 성공적으로 로드된 경우
    private void OnGetPlayerProfileSuccess(GetPlayerProfileResult result)
    {
        // DisplayName (닉네임) 설정
        nickname = result.PlayerProfile.DisplayName.Split('@')[0];

        // UserData에서 캐릭터와 돈 정보 가져오기
        GetUserData();
    }

    // PlayFab에서 프로필 정보 로드 실패 시 처리
    private void OnGetPlayerProfileFailure(PlayFabError error)
    {
        Debug.LogError($"프로필 로드 실패: {error.GenerateErrorReport()}");
    }

    // 유저 데이터를 가져오는 메서드 (돈과 캐릭터)
    private void GetUserData()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, OnGetUserDataSuccess, OnGetUserDataFailure);
    }

    // PlayFab에서 UserData 로드 성공 시 처리
    private void OnGetUserDataSuccess(GetUserDataResult result)
    {
        if (result.Data != null)
        {
            // 캐릭터와 돈 정보가 존재하는 경우
            if (result.Data.ContainsKey("characterSelect"))
            {
                character = int.Parse(result.Data["characterSelect"].Value);
            }
            if (result.Data.ContainsKey("money"))
            {
                money = int.Parse(result.Data["money"].Value);
            }

            // UserData 값이 로드되면 저장
            SaveUserInfo();

            // 값 확인 로그
            Debug.Log($"User Info: 닉네임={nickname}, 캐릭터={character}, 돈={money}");
        }
        else
        {
            // 기본값을 저장
            SaveUserInfo();
        }
    }

    // PlayFab에서 UserData 로드 실패 시 처리
    private void OnGetUserDataFailure(PlayFabError error)
    {
        Debug.LogError($"UserData를 가져오는 데 실패했습니다: {error.GenerateErrorReport()}");
    }
}
