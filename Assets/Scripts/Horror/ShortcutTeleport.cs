using UnityEngine;

public class ShortcutTeleport : MonoBehaviour
{
    [Header("순간이동할 목적지 좌표 (빈 오브젝트)")]
    public Transform destination;

    [Header("★미로 탈출구(출구) 발판이라면 이 체크박스를 켜세요!")]
    public bool isExit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 출구 발판일 때만 매니저를 찾아 개미 무리를 정리합니다.
            if (isExit)
            {
                AntManager manager = FindAnyObjectByType<AntManager>();
                if (manager != null)
                {
                    manager.DisableAllAnts();
                }
            }

            // 2. 플레이어 텔레포트 (CharacterController 간섭 에러 방지)
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            other.transform.position = destination.position;

            if (cc != null) cc.enabled = true;
            
            Debug.Log(isExit ? "🎯 출구 탈출 및 개미 정리 완료!" : "🎯 지름길 입구 진입 완료!");
        }
    }
}