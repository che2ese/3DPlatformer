using System.Collections;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private string playerNickname;
    private int playerCharacter;
    private string playerRole;

    private void Start()
    {
        // UserInfoManager에서 유저 정보 가져오기
        if (UserInfoManager.instance != null)
        {
            playerNickname = UserInfoManager.instance.nickname;
            playerCharacter = UserInfoManager.instance.character;
            playerRole = UserInfoManager.instance.role;

            Debug.Log($"플레이어 정보: 닉네임={playerNickname}, 캐릭터={playerCharacter}, 역할={playerRole}");
        }
        else
        {
            Debug.LogError("UserInfoManager 인스턴스가 없습니다!");
        }
    }
}