using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelection : MonoBehaviour
{
    [Header("CharSelection")]
    public Button[] characterButtons; // 캐릭터 버튼 (1, 2, 3, 4)
    public Button confirmButton;     // 선택 확인 버튼
    private Button selectedButton = null; // 현재 선택된 버튼
    public string selectedCharacter = ""; // 선택된 캐릭터 번호

    [Header("NIckSelection")]
    public GameObject NickPanel;
    public TMP_InputField NickInput;

    public TMP_Text warning;

    const string URL = "https://script.google.com/macros/s/AKfycbyTfXkFJW5FQh_lDEg0R6T5YTef4z7yCbmXVKe7SNdpAaKncquSuyYYwBcFlPIZgZs/exec";

    void Start()
    {
        // 버튼 초기화
        foreach (var button in characterButtons)
        {
            button.onClick.AddListener(() => OnCharacterSelected(button));
        }

        confirmButton.onClick.AddListener(OnConfirmSelection);
        confirmButton.interactable = false; // 선택 확인 버튼 비활성화
    }

    void OnCharacterSelected(Button clickedButton)
    {
        // 이미 선택된 버튼을 다시 누르면 선택 취소
        if (selectedButton == clickedButton)
        {
            ResetSelection();
            return;
        }

        // 새 버튼 선택
        selectedCharacter = clickedButton.name; // 버튼 이름을 캐릭터 번호로 사용
        selectedButton = clickedButton;

        // 다른 버튼 비활성화
        foreach (var button in characterButtons)
        {
            button.interactable = (button == clickedButton); // 선택된 버튼만 활성화
        }

        // 선택된 버튼 강조 (예: 색 변경)
        clickedButton.GetComponent<Image>().color = Color.green;

        // 선택 확인 버튼 활성화
        confirmButton.interactable = true;
    }

    void ResetSelection()
    {
        // 선택 취소
        selectedCharacter = "";
        selectedButton = null;

        // 모든 버튼 다시 활성화
        foreach (var button in characterButtons)
        {
            button.interactable = true;
            button.GetComponent<Image>().color = Color.white; // 색상 초기화
        }

        // 선택 확인 버튼 비활성화
        confirmButton.interactable = false;
    }

    void OnConfirmSelection()
    {
        if (string.IsNullOrEmpty(selectedCharacter))
        {
            Debug.LogError("캐릭터가 선택되지 않았습니다.");
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "selectCharacter");
        form.AddField("character", selectedCharacter); // 선택된 캐릭터 번호

        StartCoroutine(PostToGoogleSheet(form));

        // Panel 활성화
        NickPanel.SetActive(true);
    }

    IEnumerator PostToGoogleSheet(WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("캐릭터 선택 저장 완료: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("서버 요청 실패: " + www.error);
            }
        }
    }
    // 닉네임 저장 함수
    public void OnSaveNick()
    {
        string nickName = NickInput.text.Trim();

        if (string.IsNullOrEmpty(nickName))
        {
            warning.gameObject.SetActive(true);
            warning.text = "empty";
            StartCoroutine(ServerData.instance.TextSetFalse());
            Debug.LogError("닉네임이 비어 있습니다.");
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
                Debug.Log("응답: " + response);

                if (response.Contains("\"result\":\"ERROR\""))
                {
                    warning.gameObject.SetActive(true);
                    warning.text = "중복";
                    StartCoroutine(ServerData.instance.TextSetFalse());
                    Debug.LogError("닉네임 중복 또는 저장 실패");
                }
                else
                {
                    Debug.Log("닉네임 저장 완료");
                    SceneManager.LoadScene("MainScene");
                }
            }
            else
            {
                Debug.LogError("서버 요청 실패: " + www.error);
            }
        }
    }
}
