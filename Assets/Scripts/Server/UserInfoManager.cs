using System.Collections;
using UnityEngine;

public class UserInfoManager : MonoBehaviour
{
    public static UserInfoManager instance;

    public string nickname { get; private set; }
    public int character { get; private set; }
    public string role { get; private set; }

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

    public void SetUserInfo(string nickname, int character, string role)
    {
        Debug.Log($"SetUserInfo 호출됨: 닉네임={nickname}, 캐릭터={character}, 역할={role}");
        this.nickname = nickname;
        this.character = character;
        this.role = role;
    }
}