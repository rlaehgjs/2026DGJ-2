using UnityEngine;

public class Box : MonoBehaviour
{
    public enum ItemType { Empty, Hammer, Health }
    
    [Header("상자 안의 아이템 (매니저가 자동으로 정해줌)")]
    public ItemType containsItem = ItemType.Empty;

    [Header("아이템 스폰용 프리팹 (비워두면 테스트용 기본 도형이 생성됩니다)")]
    public GameObject hammerPrefab;
    public GameObject healthPrefab;

    // 레이저에 맞았을 때 호출되는 핵심 함수
    public void OnHitByLaser()
    {
        // 1. 상자가 파괴되기 직전, 배정된 아이템을 필드에 생성합니다.
        SpawnRewardItem();

        // 2. 상자 오브젝트를 파괴합니다 (기존 기능 유지).
        Destroy(gameObject);
    }

    // 테스트용 오브젝트 생성을 포함한 아이템 스폰 로직
    private void SpawnRewardItem()
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;

        if (containsItem == ItemType.Hammer)
        {
            if (hammerPrefab != null)
            {
                Instantiate(hammerPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // [테스트 전용] 망치 프리팹이 없으면 빨간색 큐브 생성
                GameObject testHammer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                testHammer.transform.position = spawnPosition;
                testHammer.name = "[TEST] Hammer_Cube";
                
                Renderer render = testHammer.GetComponent<Renderer>();
                if (render != null) render.material.color = Color.red;
            }
            Debug.Log("🔨 [테스트] 망치(빨간 큐브)가 상자에서 나왔습니다!");
        }
        else if (containsItem == ItemType.Health)
        {
            if (healthPrefab != null)
            {
                Instantiate(healthPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // [테스트 전용] 포션 프리팹이 없으면 초록색 구체 생성
                GameObject testHealth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                testHealth.transform.position = spawnPosition;
                testHealth.name = "[TEST] Health_Sphere";
                
                Renderer render = testHealth.GetComponent<Renderer>();
                if (render != null) render.material.color = Color.green;
            }
            Debug.Log("❤️ [테스트] 체력 회복 아이템(초록 구체)이 상자에서 나왔습니다!");
        }
        else
        {
            Debug.Log("💨 꽝! 이 상자는 비어있었습니다.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 레이저 총알이나 레이저 이름을 가진 물체에 닿으면 발동
        if (other.gameObject.name.Contains("Laser") || other.CompareTag("Laser"))
        {
            OnHitByLaser();
        }
    }
}