using System.Collections;
using UnityEngine;

public class UserInfoManager : MonoBehaviour
{
    public static UserInfoManager instance;

    public string nickname { get; private set; }
    public int character { get; private set; }
    public string role { get; private set; }
    public int money { get; private set; }

    [Header("Character Prefabs")]
    public GameObject[] characterPrefabs; // 캐릭터 프리팹 배열
    private GameObject spawnedCharacter; // 생성된 캐릭터 오브젝트

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

    public void SetUserInfo(string nickname, int character, string role, int money)
    {
        Debug.Log($"SetUserInfo 호출됨: 닉네임={nickname}, 캐릭터={character}, 역할={role}, 재화={money}");
        this.nickname = nickname;
        this.character = character;
        this.role = role;
        this.money = money;

        SpawnCharacter(); // 유저 정보 설정 후 캐릭터 생성
    }

    private void SpawnCharacter()
    {
        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogError("캐릭터 프리팹이 설정되지 않았습니다.");
            return;
        }

        int characterIndex = character - 1; // 배열 인덱스 변환

        if (characterIndex < 0 || characterIndex >= characterPrefabs.Length)
        {
            Debug.LogError("잘못된 캐릭터 인덱스: " + characterIndex);
            return;
        }

        // 기존 캐릭터 제거
        if (spawnedCharacter != null)
        {
            Destroy(spawnedCharacter);
        }

        // 캐릭터 생성 및 비활성화
        spawnedCharacter = Instantiate(characterPrefabs[characterIndex], Vector3.zero, Quaternion.identity);
        spawnedCharacter.SetActive(false);

        DontDestroyOnLoad(spawnedCharacter);
    }
}