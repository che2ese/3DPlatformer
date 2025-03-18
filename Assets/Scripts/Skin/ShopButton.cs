using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    [Header("Button Info")]
    public string type; // 스킨의 타입 (예: Hat, Wing)
    public int id;      // 고유 ID (예: 0, 1, 2, ...)

    private Button button;
    private Color originalColor;
    private Color pressedColor = new Color(0.7f, 0.7f, 0.7f); // 눌린 상태 색상
    private bool isPressed = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalColor = button.colors.normalColor;
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        Debug.Log($"Button Clicked! Type: {type}, ID: {id}");

        // ShopManager에서 버튼을 처리하도록 호출
        ShopManager.Instance.OnItemButtonClicked(this);

        // 다른 버튼이 눌리면 이전 버튼은 원래 색으로 돌아감
        if (!isPressed)
        {
            SetPressed(true);
        }
    }

    public void ResetButton()
    {
        SetPressed(false);
    }

    public bool IsPressed()
    {
        return isPressed;
    }

    public void SetPressed(bool pressed)
    {
        isPressed = pressed;
        button.image.color = pressed ? pressedColor : originalColor;
    }
}
