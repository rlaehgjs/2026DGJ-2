using UnityEngine;

public class AntManager : MonoBehaviour
{
    [Header("개미들의 부모 오브젝트 (AntGroup)")]
    public GameObject antGroup;
    private bool hasTriggered = false;

    void Start()
    {
        // 게임 시작 시 개미 무리를 자동으로 꺼둡니다.
        if (antGroup != null) antGroup.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody playerBody = other.attachedRigidbody;
        if (!hasTriggered && playerBody != null && playerBody.CompareTag("Player"))
        {
            hasTriggered = true;
            if (antGroup != null)
            {
                antGroup.SetActive(true); 
                
                // 자식 개미들의 새 내비메쉬 AI들에게 일제히 추격 시작 명령
                AntNavMeshAI[] ants = antGroup.GetComponentsInChildren<AntNavMeshAI>();
                foreach (AntNavMeshAI ant in ants)
                {
                    ant.StartChase();
                }
                Debug.Log("🐜 개미 떼 출현! 내비메쉬 추격을 시작합니다.");
            }
        }
    }

    // 플레이어가 탈출하면 텔레포트 스크립트가 이 함수를 원격 호출합니다.
    public void DisableAllAnts()
    {
        if (antGroup != null)
        {
            AntNavMeshAI[] ants = antGroup.GetComponentsInChildren<AntNavMeshAI>();
            foreach (AntNavMeshAI ant in ants)
            {
                ant.StopChase();
            }
            antGroup.SetActive(false); // 개미 무리 통째로 게임에서 증발
        }
        Debug.Log("🛑 플레이어가 탈출하여 개미 무리가 소멸했습니다.");
    }
}
