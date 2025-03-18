using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Shop Button Setup")]
    public Button[] itemButtons;
    public GameObject[] playerAry; // 플레이어 오브젝트
    public GameObject player; // 플레이어 오브젝트
    public int playerNum;
    public int totalPrice;
    public Text totalPriceTxt;
    public Button buyButton; // 구매 버튼

    [Header("Shop Panel Button")]
    public Button[] panelBtn;
    private Button selectedPaenlButton = null;  // 현재 선택된 버튼

    [Header("Player Rotate")]
    private bool rotating;
    private float rotateSpeed = 50.0f;
    private Vector3 mousePos;

    private ShopButton selectedButton;

    private int[] hat = new int[8];
    private int[] wing = new int[8];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        switch(playerNum)
        {
            case 0:
                playerAry[0].SetActive(true);
                player = playerAry[0];
                break;
            case 1:
                playerAry[1].SetActive(true);
                player = playerAry[1];
                break;
            case 2:
                playerAry[2].SetActive(true);
                player = playerAry[2];
                break;
            case 3:
                playerAry[3].SetActive(true);
                player = playerAry[3];
                break;
        }
        OnPaenlButtonClick(panelBtn[0]);
        foreach (Button button in itemButtons)
        {
            ShopButton shopButton = button.GetComponent<ShopButton>();
            if (shopButton != null)
            {
                button.onClick.AddListener(() => OnItemButtonClicked(shopButton));
            }
        }

        foreach (Button btn in panelBtn)
        {
            btn.onClick.AddListener(() => OnPaenlButtonClick(btn));
        }

        // Buy 버튼에 클릭 리스너 추가
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    void OnMouseDown()
    {
        rotating = true;
        mousePos = Input.mousePosition;
    }

    void OnMouseUp()
    {
        rotating = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 클릭 시작
        {
            rotating = true;
            mousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) // 마우스 클릭 종료
        {
            rotating = false;
        }

        if (rotating)
        {
            Vector3 offset = Input.mousePosition - mousePos;
            float rotationY = -offset.x * rotateSpeed * Time.deltaTime;

            if (player != null) // player가 할당되었는지 확인
            {
                player.transform.Rotate(0, rotationY, 0, Space.World); // Y축 회전
            }

            mousePos = Input.mousePosition; // 마우스 위치 업데이트
        }
    }

    void OnPaenlButtonClick(Button clickedButton)
    {
        if (selectedPaenlButton == clickedButton) return; // 이미 선택된 버튼이면 무시

        if (selectedPaenlButton != null)
        {
            // 이전 버튼의 상태 변경 (네가 원하는 코드 추가 가능)
            selectedPaenlButton.GetComponent<Image>().sprite = selectedPaenlButton.GetComponent<PanelButton>().buttonImage[0];
            selectedPaenlButton.GetComponent<PanelButton>().panel.SetActive(false);
            selectedPaenlButton.GetComponent<PanelButton>().item.SetActive(false);
        }

        selectedPaenlButton = clickedButton;
        // 새 버튼의 상태 변경 (네가 원하는 코드 추가 가능)
        selectedPaenlButton.GetComponent<Image>().sprite = selectedPaenlButton.GetComponent<PanelButton>().buttonImage[1];
        selectedPaenlButton.GetComponent<PanelButton>().panel.SetActive(true);
        selectedPaenlButton.GetComponent<PanelButton>().item.SetActive(true);
    }

    public void OnItemButtonClicked(ShopButton clickedButton)
    {
        // 다른 버튼 클릭 시 이전 버튼 초기화
        if (selectedButton != null && selectedButton != clickedButton)
        {
            DeactivateAllSkinsByType(selectedButton.type);
            selectedButton.ResetButton();
        }

        selectedButton = clickedButton;
        selectedButton.SetPressed(true);

        // 스킨 활성화 (해당 타입 + ID)
        ToggleSkinOnPlayer(clickedButton.type, clickedButton.id);

        // 가격 갱신
        UpdateTotalPrice();  // 가격 갱신
    }

    private void ToggleSkinOnPlayer(string type, int id)
    {
        // 기존 스킨 비활성화
        DeactivateAllSkinsByType(type);

        // 선택한 스킨 활성화
        Transform typeGroup = FindChildWithTag(player.transform, type);
        if (typeGroup != null)
        {
            foreach (Transform skin in typeGroup)
            {
                ShopID shopID = skin.GetComponent<ShopID>();
                if (shopID != null)
                {
                    // 활성화 상태를 1로 설정
                    if (shopID.active == 1 || shopID.active == 0)
                        shopID.active = (shopID.id == id) ? 1 : 0;
                    else
                        shopID.active = (shopID.id == id) ? 3 : 2;

                    shopID.UpdateSkinStatus();

                    if (typeGroup.tag == "Hat")
                    // hat 배열 업데이트
                        hat[shopID.id] = shopID.active;
                    else if (typeGroup.tag == "Wing")
                        wing[shopID.id] = shopID.active;
                }
            }
        }

        PrintStatusByType(selectedButton.type); // 구매 후 상태 출력
    }

    private void DeactivateAllSkinsByType(string type)
    {
        foreach (Transform child in player.transform)
        {
            if (child.CompareTag(type))
            {
                foreach (Transform grandChild in child)
                {
                    ShopID skin = grandChild.GetComponent<ShopID>();
                    if (skin != null)
                    {
                        if (skin.active == 3)
                            skin.active = 2;
                        if (skin.active == 1)
                            skin.active = 0;
                        skin.UpdateSkinStatus();
                        Debug.Log($"비활성화됨: {grandChild.name} (type: {type})");
                    }
                }
            }
        }
    }

    private Transform FindChildWithTag(Transform parent, string tag)
    {
        if (parent.CompareTag(tag))
        {
            return parent;
        }

        foreach (Transform child in parent)
        {
            Transform found = FindChildWithTag(child, tag);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    public void OnBuyButtonClicked()
    {
        if (selectedButton != null)
        {
            // 선택된 아이템을 구매하는 처리
            Transform typeGroup = FindChildWithTag(player.transform, selectedButton.type);
            if (typeGroup != null)
            {
                foreach (Transform skin in typeGroup)
                {
                    ShopID shopID = skin.GetComponent<ShopID>();
                    if (shopID != null && shopID.id == selectedButton.id && shopID.active == 1)
                    {
                        shopID.active = 3;
                        shopID.UpdateSkinStatus();

                        if (typeGroup.tag == "Hat")
                            hat[shopID.id] = shopID.active;
                        else if (typeGroup.tag == "Wing")
                            wing[shopID.id] = shopID.active;
                    }
                }
            }

            // 모든 배열을 돌면서 active가 1인 것들을 모두 3으로 바꿈
            UpdateAllItemsToEquipped("Hat");
            UpdateAllItemsToEquipped("Wing");
            // 필요한 경우 다른 타입들도 추가할 수 있음 (예: "Shirt", "Pants" 등)
        }

        PrintStatusByType(selectedButton.type); // 구매 후 상태 출력
        UpdateTotalPrice();
    }

    // 주어진 타입에 해당하는 모든 아이템을 확인하여 active가 1인 경우 3으로 바꿈
    private void UpdateAllItemsToEquipped(string type)
    {
        Transform typeGroup = FindChildWithTag(player.transform, type);
        if (typeGroup != null)
        {
            foreach (Transform skin in typeGroup)
            {
                ShopID shopID = skin.GetComponent<ShopID>();
                if (shopID != null && shopID.active == 1) // active가 1인 경우
                {
                    shopID.active = 3; // 3으로 변경 (구매 완료, 장착됨)
                    shopID.UpdateSkinStatus();

                    // 해당 타입의 배열을 업데이트
                    if (type == "Hat")
                        hat[shopID.id] = shopID.active;
                    else if (type == "Wing")
                        wing[shopID.id] = shopID.active;
                }
            }
        }
    }


    // 각 타입별 active 값을 출력
    private void PrintStatusByType(string type)
    {
        int[] targetArray = null;

        if (type == "Hat")
            targetArray = hat;
        else if (type == "Wing")
            targetArray = wing;

        if (targetArray != null)
        {
            string status = $"{type} Active Values: ";
            for (int i = 0; i < targetArray.Length; i++)
            {
                status += $"[{i}]: {targetArray[i]} ";
            }
            Debug.Log(status);
        }
        else
        {
            Debug.Log($"Unknown type: {type}");
        }
    }

    private void UpdateTotalPrice()
    {
        totalPrice = 0; // totalPrice 초기화

        // Hat 배열에서 active가 1인 것들의 price를 합산
        for (int i = 0; i < hat.Length; i++)
        {
            if (hat[i] == 1) // 활성화된 (구매 가능한) 스킨만 더하기
            {
                Transform skin = FindChildWithTag(player.transform, "Hat").GetChild(i);
                ShopID shopID = skin.GetComponent<ShopID>();
                if (shopID != null)
                {
                    totalPrice += shopID.price;
                }
            }
        }

        // Wing 배열에서 active가 1인 것들의 price를 합산
        for (int i = 0; i < wing.Length; i++)
        {
            if (wing[i] == 1) // 활성화된 (구매 가능한) 스킨만 더하기
            {
                Transform skin = FindChildWithTag(player.transform, "Wing").GetChild(i);
                ShopID shopID = skin.GetComponent<ShopID>();
                if (shopID != null)
                {
                    totalPrice += shopID.price;
                }
            }
        }

        Debug.Log($"[ShopManager] 총 합계 가격: {totalPrice}");
        totalPriceTxt.text = totalPrice.ToString();
    }

}
