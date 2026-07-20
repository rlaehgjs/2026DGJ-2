using UnityEngine;

public class DollZoneTrigger : MonoBehaviour
{
    [Header("작동을 켜고 끌 인형 오브젝트(GameObject)를 넣어주세요")]
    public GameObject dollObject; 

    private void Start()
    {
        // 게임이 시작될 때는 인형 오브젝트를 아예 꺼둡니다.
        if (dollObject != null)
        {
            dollObject.SetActive(false);
            Debug.Log("🔒 플레이어가 방 밖에 있으므로 인형 오브젝트를 비활성화합니다.");
        }
    }

    // 플레이어가 감지 구역(방) 안으로 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (dollObject != null)
            {
                dollObject.SetActive(true); // 인형 오브젝트를 완전히 켭니다.
                Debug.Log("👤 플레이어 방 진입! 인형을 활성화합니다.");
            }
        }
    }

    // 플레이어가 감지 구역(방) 밖으로 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (dollObject != null)
            {
                dollObject.SetActive(false); // 인형 오브젝트를 완전히 끕니다.
                Debug.Log("🚪 플레이어 방 퇴장. 인형을 비활성화합니다.");
            }
        }
    }
}