using System.Collections.Generic;
using UnityEngine;

public class CardDealer : MonoBehaviour
{
    [Header("유니티 창에서 드래그해서 넣을 상자 5개")]
    public List<Box> boxes = new List<Box>();

    void Start()
    {
        // 1. 준비물 가방에 아이템 5개 세팅 (망치 1, 포션 1, 꽝 3)
        List<Box.ItemType> items = new List<Box.ItemType>()
        {
            Box.ItemType.Hammer,  
            Box.ItemType.Health,  
            Box.ItemType.Empty,   
            Box.ItemType.Empty,
            Box.ItemType.Empty
        };

        // 2. 가방 안의 아이템 순서를 무작위로 섞기 (셔플)
        for (int i = 0; i < items.Count; i++)
        {
            int randomIndex = Random.Range(0, items.Count);
            Box.ItemType temp = items[i];
            items[i] = items[randomIndex];
            items[randomIndex] = temp;
        }

        // 3. 씬에 배치된 상자들에 순서대로 주입
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i] != null)
            {
                boxes[i].containsItem = items[i];
            }
        }
        
        Debug.Log("🎲 상자 5개에 아이템 무작위 분배 완료!");
    }
}