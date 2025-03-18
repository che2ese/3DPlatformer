using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skin : MonoBehaviour
{
    private ShopID[] childSkins;

    private void Start()
    {
        // 자식 오브젝트의 ShopID 컴포넌트를 배열로 저장
        childSkins = GetComponentsInChildren<ShopID>(true); // 비활성화된 자식들도 포함

        // 장착(active == 2)된 스킨 활성화
        foreach (ShopID skin in childSkins)
        {
            skin.UpdateSkinStatus(); // 처음 상태를 갱신
        }
    }
}
