using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // EventSystem �߰�
using TMPro;

[System.Serializable]
public class ServerResponse
{
    public string order, result, msg;
}
public class ServerData : MonoBehaviour
{
    public static ServerData instance; // **�̱��� �ν��Ͻ�**

    public ServerResponse server;

    const string URL = "https://script.google.com/macros/s/AKfycbyTfXkFJW5FQh_lDEg0R6T5YTef4z7yCbmXVKe7SNdpAaKncquSuyYYwBcFlPIZgZs/exec";
    public TMP_InputField IDInput, PassInput;
    string id, pass;

    public TMP_Text warning;

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
            Login();
        }
        HandleTabKey();
    }

    // Tab Ű �Է� ó��
    private void HandleTabKey()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (IDInput.isFocused)
            {
                // IDInput���� Tab Ű�� ������ PassInput���� �̵�
                PassInput.Select();
            }
            else if (PassInput.isFocused)
            {
                // PassInput���� Tab Ű�� ������ IDInput���� ���ư��� (��ȯ ����)
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
            warning.text = "empty";
            StartCoroutine(TextSetFalse());
            print("���̵� �Ǵ� ��й�ȣ�� ����ֽ��ϴ�");
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "register");
        form.AddField("id", id);
        form.AddField("pass", pass);
        form.AddField("role", "User"); // E���� �߰��� ���� ����

        StartCoroutine(Post(form));
    }

    public void Login()
    {
        if (!SetIDPass())
        {
            warning.gameObject.SetActive(true);
            warning.text = "empty";
            StartCoroutine(TextSetFalse());
            print("���̵� �Ǵ� ��й�ȣ�� ����ֽ��ϴ�");
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
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string responseText = www.downloadHandler.text;

                // JSON ������ �Ľ��Ͽ� Ȯ��
                var response = JsonUtility.FromJson<ServerResponse>(responseText);

                if (response.result == "OK")
                {
                    PlayerPrefs.SetString("userId", id); // �α����� ���� ID ����
                    PlayerPrefs.Save();

                    // C���� D�� �� Ȯ��
                    if (response.msg.Contains("C") && response.msg.Contains("D"))
                    {
                        Debug.Log("��� ���� �����: MainScene���� �̵�");
                        SceneManager.LoadScene("MainScene"); // MainScene���� ��ȯ
                    }
                    else
                    {
                        Debug.Log("C�� �Ǵ� D�� ���� ����: SelectScene���� �̵�");
                        SceneManager.LoadScene("SelectScene"); // SelectScene���� ��ȯ
                    }
                }
                else
                {
                    // �α��� ���� �޽��� ǥ��
                    warning.gameObject.SetActive(true);
                    warning.text = response.msg;
                    StartCoroutine(TextSetFalse());
                    print(response.msg);
                }
            }
            else
            {
                Debug.LogError("���� ��û ����: " + www.error);
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
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form)) // �ݵ�� using�� ����Ѵ�
        {
            yield return www.SendWebRequest();

            if (www.isDone) Response(www.downloadHandler.text);
            else print("���� ������ �����ϴ�.");
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
            print(server.order + "�� ������ �� �����ϴ�. ���� �޽��� : " + server.msg);
            return;
        }

        print(server.order + "�� �����߽��ϴ�. �޽��� : " + server.msg);
    }

    public IEnumerator TextSetFalse()
    {
        yield return new WaitForSeconds(3f);
        warning.gameObject.SetActive(false);
    }
}