using UnityEngine;

public class ShortcutTeleport : MonoBehaviour
{
    [Header("순간이동할 목적지 좌표 (빈 오브젝트)")]
    public Transform destination;

    [Header("미로 탈출구(출구) 발판이라면 이 체크박스를 켜세요!")]
    public bool isExit = false;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody playerBody = other.attachedRigidbody;
        if (playerBody == null || !playerBody.CompareTag("Player"))
        {
            Debug.Log($"[ShortcutTeleport] {name}: '{other.name}'은(는) Player Rigidbody가 아니어서 무시했습니다.");
            return;
        }

        if (destination == null)
        {
            Debug.LogWarning($"[ShortcutTeleport] {name}: destination이 연결되지 않았습니다.");
            return;
        }

        // Icecream 자식 Collider가 들어와도 attachedRigidbody로 Player 루트를 이동합니다.
        playerBody.position = destination.position;
        playerBody.linearVelocity = Vector3.zero;
        playerBody.angularVelocity = Vector3.zero;

        if (isExit)
        {
            AntManager manager = FindAnyObjectByType<AntManager>();
            if (manager != null)
            {
                manager.DisableAllAnts();
            }
        }

        Debug.Log(isExit ? "[ShortcutTeleport] 출구로 이동하고 개미 무리를 정리했습니다." : "[ShortcutTeleport] 지름길 입구로 이동했습니다.");
    }
}
