using UnityEngine;

// [디버그 버전] 원인을 찾기 위해 로그를 강화했습니다.
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
            // 흰색 로그: 정상 작동 중
            Debug.Log($"<color=white>[DollZone] 🔒 게임 시작: {dollObject.name}을 비활성화했습니다.</color>");
        }
        else
        {
            // ★ 빨간색 에러 로그: 인스펙터에서 dollObject가 연결되지 않았습니다! (가장 유력한 원인)
            Debug.LogError("<color=red>[DollZone] 🚨 에러: 인스펙터에서 dollObject가 연결되지 않았습니다! 깃허브 합병 시 끊겼을 확률이 높습니다.</color>");
        }
    }

    // 플레이어가 감지 구역(방) 안으로 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        // ★ 노란색 로그: 무언가 들어옴 (충돌 자체는 작동함)
        Debug.Log($"<color=yellow>[DollZone] ⏩ 트리거 진입 감지: {other.name} (Tag: {other.tag})</color>");

        if (other.CompareTag("Player"))
        {
            if (dollObject != null)
            {
                dollObject.SetActive(true); // 인형 오브젝트를 완전히 켭니다.
                // ★ 초록색 로그: 성공
                Debug.Log("<color=green>[DollZone] ✅ ★ 플레이어 확인! 인형을 활성화했습니다.</color>");
            }
            else
            {
                Debug.LogError("<color=red>[DollZone] 🚨 에러: 활성화할 인형 오브젝트가 연결되어 있지 않습니다!</color>");
            }
        }
    }

    // 플레이어가 감지 구역(방) 밖으로 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 노란색 로그: 플레이어가 나가는 것을 감지함
            Debug.Log($"<color=yellow>[DollZone] ⏪ 트리거 퇴장 감지: {other.name}</color>");

            if (dollObject != null)
            {
                dollObject.SetActive(false); // 인형 오브젝트를 완전히 끕니다.
                Debug.Log("<color=white>[DollZone] 🚪 플레이어 퇴장. 인형을 다시 비활성화합니다.</color>");
            }
        }
    }
}