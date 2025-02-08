using System.Collections;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private string playerNickname;
    private int playerCharacter;
    private int playerMoney;

    private void Start()
    {
        // UserInfoManager에서 유저 정보 가져오기
        if (UserInfoManager.instance != null)
        {
            // 최초 정보 로드
            LoadPlayerInfo();
        }
        else
        {
            Debug.LogError("UserInfoManager 인스턴스가 없습니다!");
        }
    }

    // 유저 정보를 최신으로 갱신하기 위한 메서드
    private void LoadPlayerInfo()
    {
        playerNickname = UserInfoManager.instance.GetNickname();
        playerCharacter = UserInfoManager.instance.GetCharacter();
        playerMoney = UserInfoManager.instance.GetMoney();

        // 디버그 로그로 정보 확인
        Debug.Log($"플레이어 정보: 닉네임={playerNickname}, 캐릭터={playerCharacter}, 재화={playerMoney}");
    }
}
