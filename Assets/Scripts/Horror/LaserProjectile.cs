using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [HideInInspector]
    public GameObject shooter; 

    [Header("=== 수명 설정 ===")]
    public float maxLifetime = 10f; 

    [Header("=== 이펙트 세팅 (NEW) ===")]
    [Tooltip("벽이나 플레이어에 부딪혔을 때 터질 폭발/스파크 이펙트 프리팹")]
    public GameObject explosionEffectPrefab;

    [Header("=== 통과할 오브젝트 이름 키워드 ===")]
    public string[] ignoreKeywords = { "Floor", "dresser" }; 

    private void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 쏜 인형 본인이나 그 자식 오브젝트 무시
        if (shooter != null && (other.gameObject == shooter || other.transform.IsChildOf(shooter.transform)))
        {
            return; 
        }

        // 2. 투명한 감지 구역(Trigger) 무시
        if (other.isTrigger)
        {
            return; 
        }

        // 3. 무시할 키워드 목록 확인
        foreach (string keyword in ignoreKeywords)
        {
            if (other.name.Contains(keyword))
            {
                return; 
            }
        }

        // 4. ★ [폭발 이펙트 생성] 진짜 장애물이나 플레이어에 닿았으므로 삭제되기 직전에 이펙트를 소환합니다.
        if (explosionEffectPrefab != null)
        {
            // 레이저가 닿은 현재 위치에 폭발 생성
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            
            // 파티클이 재생 완료된 후 찌꺼기가 남지 않도록 3초 뒤 자동 삭제
            Destroy(explosion, 3f); 
        }

        Debug.Log($"레이저가 진짜 장애물에 충돌함: {other.gameObject.name}");
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("★ 플레이어 적중!");
        }

        // 레이저 총알 삭제
        Destroy(gameObject); 
    }
}