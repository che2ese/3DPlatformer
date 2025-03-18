using UnityEngine;
using UnityEngine.UI;

public class ShopID : MonoBehaviour
{
    public string type;   // 스킨 타입 (예: Hat, Wing)
    public int id;        // 고유 ID
    public int active;    // 상태 (0: Not owned, 1: Purchased but not equipped, 2: Purchased, 3: Equipped)
    public int price;
    public Text priceTxt;

    // active 상태가 바뀔 때마다 상태를 갱신하는 메서드 추가
    public void UpdateSkinStatus()
    {
        // 상태에 따라 오브젝트 활성화/비활성화
        switch (active)
        {
            case 0:
                gameObject.SetActive(false);
                break;
            case 1:
                gameObject.SetActive(true);
                break;
            case 2:
                price = 0;
                gameObject.SetActive(false);
                break;
            case 3:
                price = 0;
                gameObject.SetActive(true);
                break;
        }

        priceTxt.text = price.ToString();

        // 상태 변경 로그 출력 (디버깅용)
        Debug.Log($"{type} 스킨 {id} 상태: {active}");
    }

}
