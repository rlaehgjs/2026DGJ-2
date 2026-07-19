using System.Collections.Generic;
using UnityEngine;

public class CardDealer : MonoBehaviour
{
    // 유니티 창에서 드래그해서 넣을 상자 5개 리스트
    public List<Box> boxes = new List<Box>();

    void Start()
    {
        // 1. 준비물 가방에 아이템 5개를 정직하게 넣습니다.
        List<Box.ItemType> items = new List<Box.ItemType>()
        {
            Box.ItemType.Hammer,  // 망치 1개
            Box.ItemType.Health,  // 체력 1개
            Box.ItemType.Empty,   // 꽝 3개
            Box.ItemType.Empty,
            Box.ItemType.Empty
        };

        // 2. 가방 안의 아이템들을 무작위로 마구 섞습니다.
        for (int i = 0; i < items.Count; i++)
        {
            int randomIndex = Random.Range(0, items.Count);
            Box.ItemType temp = items[i];
            items[i] = items[randomIndex];
            items[randomIndex] = temp;
        }

        // 3. 섞인 아이템을 상자 5개에 순서대로 한 개씩 쏙쏙 넣어줍니다.
        for (int i = 0; i < boxes.Count; i++)
        {
            boxes[i].containsItem = items[i];
        }
        
        Debug.Log("🎲 상자 5개에 아이템 무작위 분배 완료!");
    }
}