using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelection : MonoBehaviour
{
    [Header("CharSelection")]
    public Button[] characterButtons; // ĳ���� ��ư (1, 2, 3, 4)
    public Button confirmButton;     // ���� Ȯ�� ��ư
    private Button selectedButton = null; // ���� ���õ� ��ư
    public string selectedCharacter = ""; // ���õ� ĳ���� ��ȣ

    [Header("NIckSelection")]
    public GameObject NickPanel;
    public TMP_InputField NickInput;

    public TMP_Text warning;

    const string URL = "https://script.google.com/macros/s/AKfycbyTfXkFJW5FQh_lDEg0R6T5YTef4z7yCbmXVKe7SNdpAaKncquSuyYYwBcFlPIZgZs/exec";

    void Start()
    {
        // ��ư �ʱ�ȭ
        foreach (var button in characterButtons)
        {
            button.onClick.AddListener(() => OnCharacterSelected(button));
        }

        confirmButton.onClick.AddListener(OnConfirmSelection);
        confirmButton.interactable = false; // ���� Ȯ�� ��ư ��Ȱ��ȭ
    }

    void OnCharacterSelected(Button clickedButton)
    {
        // �̹� ���õ� ��ư�� �ٽ� ������ ���� ���
        if (selectedButton == clickedButton)
        {
            ResetSelection();
            return;
        }

        // �� ��ư ����
        selectedCharacter = clickedButton.name; // ��ư �̸��� ĳ���� ��ȣ�� ���
        selectedButton = clickedButton;

        // �ٸ� ��ư ��Ȱ��ȭ
        foreach (var button in characterButtons)
        {
            button.interactable = (button == clickedButton); // ���õ� ��ư�� Ȱ��ȭ
        }

        // ���õ� ��ư ���� (��: �� ����)
        clickedButton.GetComponent<Image>().color = Color.green;

        // ���� Ȯ�� ��ư Ȱ��ȭ
        confirmButton.interactable = true;
    }

    void ResetSelection()
    {
        // ���� ���
        selectedCharacter = "";
        selectedButton = null;

        // ��� ��ư �ٽ� Ȱ��ȭ
        foreach (var button in characterButtons)
        {
            button.interactable = true;
            button.GetComponent<Image>().color = Color.white; // ���� �ʱ�ȭ
        }

        // ���� Ȯ�� ��ư ��Ȱ��ȭ
        confirmButton.interactable = false;
    }

    void OnConfirmSelection()
    {
        if (string.IsNullOrEmpty(selectedCharacter))
        {
            Debug.LogError("ĳ���Ͱ� ���õ��� �ʾҽ��ϴ�.");
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "selectCharacter");
        form.AddField("character", selectedCharacter); // ���õ� ĳ���� ��ȣ

        StartCoroutine(PostToGoogleSheet(form));

        // Panel Ȱ��ȭ
        NickPanel.SetActive(true);
    }

    IEnumerator PostToGoogleSheet(WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("ĳ���� ���� ���� �Ϸ�: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("���� ��û ����: " + www.error);
            }
        }
    }
    // �г��� ���� �Լ�
    public void OnSaveNick()
    {
        string nickName = NickInput.text.Trim();

        if (string.IsNullOrEmpty(nickName))
        {
            warning.gameObject.SetActive(true);
            warning.text = "empty";
            StartCoroutine(ServerData.instance.TextSetFalse());
            Debug.LogError("�г����� ��� �ֽ��ϴ�.");
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "saveNick");
        form.AddField("nickname", nickName);

        StartCoroutine(PostNickNameToGoogleSheet(form));
    }

    IEnumerator PostNickNameToGoogleSheet(WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string response = www.downloadHandler.text;
                Debug.Log("����: " + response);

                if (response.Contains("\"result\":\"ERROR\""))
                {
                    warning.gameObject.SetActive(true);
                    warning.text = "�ߺ�";
                    StartCoroutine(ServerData.instance.TextSetFalse());
                    Debug.LogError("�г��� �ߺ� �Ǵ� ���� ����");
                }
                else
                {
                    Debug.Log("�г��� ���� �Ϸ�");
                    SceneManager.LoadScene("MainScene");
                }
            }
            else
            {
                Debug.LogError("���� ��û ����: " + www.error);
            }
        }
    }
}
