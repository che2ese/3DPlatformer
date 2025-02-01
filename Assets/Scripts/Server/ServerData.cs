using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // EventSystem 추가
using TMPro;

[System.Serializable]
public class ServerResponse
{
    public string order;       // 요청 명령
    public string result;      // 결과 상태 (OK/ERROR)
    public string msg;         // 메시지
}
[System.Serializable]
public class UserInfoData
{
    public string nickname; // 닉네임
    public int character; // 캐릭터 (숫자)
    public string role; // 역할
    public int money;
}

public class ServerData : MonoBehaviour
{
    public static ServerData instance; // **싱글톤 인스턴스**

    public ServerResponse server;

    const string URL = "https://script.google.com/macros/s/AKfycbyTfXkFJW5FQh_lDEg0R6T5YTef4z7yCbmXVKe7SNdpAaKncquSuyYYwBcFlPIZgZs/exec";
    public TMP_InputField IDInput, PassInput;
    string id, pass;

    public TMP_Text warning;
    public GameObject LoadingBar;

    // 읽기 전용 프로퍼티
    public string Id
    {
        get { return id; }
    }

    // 읽기/쓰기 가능한 프로퍼티
    public string ModifiableId
    {
        get { return id; }
        set { id = value; }
    }

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

    // Tab 키 입력 처리
    private void HandleTabKey()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (IDInput.isFocused)
            {
                // IDInput에서 Tab 키를 누르면 PassInput으로 이동
                PassInput.Select();
            }
            else if (PassInput.isFocused)
            {
                // PassInput에서 Tab 키를 누르면 IDInput으로 돌아가게 (순환 구조)
                IDInput.Select();
            }
        }
    }

    bool SetIDPass()
    {
        id = IDInput.text.Trim();
        pass = PassInput.text.Trim();

        if (id == "" || pass == "") return false;
        else return true;
    }

    public void Register()
    {
        if (!SetIDPass())
        {
            warning.gameObject.SetActive(true);
            warning.text = "아이디 또는 비밀번호가 비어있습니다";
            StartCoroutine(TextSetFalse());
            return;
        }

        LoadingBar.SetActive(true);
        WWWForm form = new WWWForm();
        form.AddField("order", "register");
        form.AddField("id", id);
        form.AddField("pass", pass);
        form.AddField("role", "User"); // E열에 추가될 역할 지정

        StartCoroutine(Post(form));
    }

    public void Login()
    {
        if (!SetIDPass())
        {
            warning.gameObject.SetActive(true);
            warning.text = "아이디 또는 비밀번호가 비어있습니다";
            StartCoroutine(TextSetFalse());
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "login");
        form.AddField("id", id);
        form.AddField("pass", pass);

        StartCoroutine(LoginAndSwitchScene(form));
    }

    IEnumerator LoginAndSwitchScene(WWWForm form)
    {
        LoadingBar.SetActive(true);
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string responseText = www.downloadHandler.text;

                // JSON 응답을 파싱하여 확인
                var response = JsonUtility.FromJson<ServerResponse>(responseText);

                if (response.result == "OK")
                {
                    PlayerPrefs.SetString("userId", id); // 로그인한 유저 ID 저장
                    PlayerPrefs.Save();

                    // C열과 D열 값 확인
                    if (response.msg.Contains("C") && response.msg.Contains("D"))
                    {
                        Debug.Log("모든 값이 저장됨: MainScene으로 이동");
                        SceneManager.LoadScene("MainScene"); // MainScene으로 전환
                    }
                    else
                    {
                        Debug.Log("C열 또는 D열 값이 없음: SelectScene으로 이동");
                        SceneManager.LoadScene("SelectScene"); // SelectScene으로 전환
                    }
                }
                else
                {
                    // 로그인 실패 메시지 표시
                    warning.gameObject.SetActive(true);
                    warning.text = response.msg;
                    StartCoroutine(TextSetFalse());
                    print(response.msg);
                }
            }
            else
            {
                Debug.LogError("서버 요청 실패: " + www.error);
            }
        }
    }

    void OnApplicationQuit()
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "logout");

        StartCoroutine(Post(form));
    }

    IEnumerator Post(WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form)) // 반드시 using을 써야한다
        {
            yield return www.SendWebRequest();

            if (www.isDone) Response(www.downloadHandler.text);
            else print("웹의 응답이 없습니다.");
        }
    }
    void Response(string json)
    {
        if (string.IsNullOrEmpty(json)) return; 

        server = JsonUtility.FromJson<ServerResponse>(json);

        if (server.result == "ERROR")
        {
            warning.gameObject.SetActive(true);
            warning.text = server.msg;
            StartCoroutine(TextSetFalse());
            print(server.order + "을 실행할 수 없습니다. 에러 메시지 : " + server.msg);
            return;
        }

        print(server.order + "을 실행했습니다. 메시지 : " + server.msg);
    }
    public void UpdateMoney(int newMoney)
    {
        if (string.IsNullOrEmpty(UserInfoManager.instance.nickname))
        {
            Debug.LogError("유저 정보가 없습니다.");
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "updateMoney");
        form.AddField("id", PlayerPrefs.GetString("userId")); // 저장된 ID 가져오기
        form.AddField("money", newMoney.ToString()); // 업데이트할 Money 값

        StartCoroutine(SendMoneyUpdate(form));
    }

    IEnumerator SendMoneyUpdate(WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string responseText = www.downloadHandler.text;
                Debug.Log($"Money 업데이트 응답: {responseText}");

                var serverResponse = JsonUtility.FromJson<ServerResponse>(responseText);

                if (serverResponse.result == "OK")
                {
                    Debug.Log($"Money 업데이트 성공: {serverResponse.msg}");
                }
                else
                {
                    Debug.LogError($"Money 업데이트 실패: {serverResponse.msg}");
                }
            }
            else
            {
                Debug.LogError($"서버 요청 실패: {www.error}");
            }
        }
    }


    public void RequestUserInfo(string userId)
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "getUserInfo");
        form.AddField("id", userId);
        StartCoroutine(GetUserInfoFromGoogleSheet(form));
    }

    IEnumerator GetUserInfoFromGoogleSheet(WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string responseText = www.downloadHandler.text;
                Debug.Log($"유저 정보 응답: {responseText}");

                var serverResponse = JsonUtility.FromJson<ServerResponse>(responseText);

                if (serverResponse.result == "OK")
                {
                    var userInfo = JsonUtility.FromJson<UserInfoData>(serverResponse.msg);
                    Debug.Log($"유저 정보 가져오기 성공: 닉네임={userInfo.nickname}, 캐릭터={userInfo.character}, 역할={userInfo.role}, 재화={userInfo.money}");

                    // UserInfoManager에 데이터 설정
                    UserInfoManager.instance.SetUserInfo(userInfo.nickname, userInfo.character, userInfo.role, userInfo.money);
                }
                else
                {
                    Debug.LogError($"유저 정보를 가져오는 데 실패: {serverResponse.msg}");
                }
            }
            else
            {
                Debug.LogError($"서버 요청 실패: {www.error}");
            }
        }
    }

    public IEnumerator TextSetFalse()
    {
        yield return new WaitForSeconds(3f);
        warning.gameObject.SetActive(false);
    }
}