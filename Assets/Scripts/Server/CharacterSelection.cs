using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class CharacterSelection : MonoBehaviour
{
    public Button[] characterButtons; // 캐릭터 버튼 (1, 2, 3, 4)
    public GameObject[] characterObjects; // 캐릭터 오브젝트
    public Button confirmButton;     // 선택 확인 버튼
    private Button selectedButton = null; // 현재 선택된 버튼
    private GameObject selectedCharacterObject = null; // 현재 선택된 캐릭터 오브젝트
    public string selectedCharacter = ""; // 선택된 캐릭터 번호

    void Start()
    {
        // 버튼 초기화
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i; // 람다 캡처 문제 방지
            characterButtons[i].onClick.AddListener(() => OnCharacterSelected(index));
        }

        confirmButton.onClick.AddListener(OnConfirmSelection);
        confirmButton.interactable = false; // 선택 확인 버튼 비활성화
    }

    void OnCharacterSelected(int index)
    {
        Button clickedButton = characterButtons[index];
        GameObject characterObject = characterObjects[index];

        // 이미 선택된 버튼을 다시 누르면 선택 취소
        if (selectedButton == clickedButton)
        {
            ResetSelection();
            return;
        }

        // 이전 선택 해제
        if (selectedCharacterObject != null)
        {
            selectedCharacterObject.GetComponent<Animator>().SetBool("isSelected", false);
        }

        // 새 버튼 선택
        selectedCharacter = clickedButton.name;
        selectedButton = clickedButton;
        selectedCharacterObject = characterObject;

        // 캐릭터 애니메이션 활성화
        selectedCharacterObject.GetComponent<Animator>().SetBool("isSelected", true);

        // 다른 버튼 비활성화
        foreach (var button in characterButtons)
        {
            button.interactable = (button == clickedButton);
        }

        // 선택된 버튼 강조
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
            button.GetComponent<Image>().color = Color.white;
        }

        // 애니메이션 해제
        if (selectedCharacterObject != null)
        {
            selectedCharacterObject.GetComponent<Animator>().SetBool("isSelected", false);
            selectedCharacterObject = null;
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

        // 캐릭터 선택 정보 PlayFab에 저장
        var updateDataRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "characterSelect", selectedCharacter }
            }
        };

        PlayFabClientAPI.UpdateUserData(updateDataRequest,
            result => {
                Debug.Log("캐릭터 선택이 저장되었습니다.");

                // 메인 씬으로 이동
                SceneManager.LoadScene("MainScene");
            },
            error => {
                Debug.LogError("캐릭터 선택 저장 실패: " + error.GenerateErrorReport());
            });
    }
}

