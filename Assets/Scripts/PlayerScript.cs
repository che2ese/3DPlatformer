using System.Collections;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private string playerNickname;
    private int playerCharacter;
    private string playerRole;

    private void Start()
    {
        // UserInfoManager���� ���� ���� ��������
        if (UserInfoManager.instance != null)
        {
            playerNickname = UserInfoManager.instance.nickname;
            playerCharacter = UserInfoManager.instance.character;
            playerRole = UserInfoManager.instance.role;

            // ������ �����͸� �α׷� ���
            Debug.Log($"�÷��̾� ����: �г���={playerNickname}, ĳ����={playerCharacter}, ����={playerRole}");
        }
        else
        {
            Debug.LogError("UserInfoManager �ν��Ͻ��� �����ϴ�!");
        }
    }
}
