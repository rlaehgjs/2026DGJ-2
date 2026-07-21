using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [HideInInspector]
    public GameObject shooter; 

    [Header("=== 수명 설정 ===")]
    public float maxLifetime = 10f; 

    [Header("=== 이펙트 세팅 ===")]
    public GameObject explosionEffectPrefab;

    [Header("=== 통과할 오브젝트 이름 키워드 ===")]
    public string[] ignoreKeywords = { "Floor", "dresser", "Glass", "Transparent" }; 

    private void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 쏜 인형 본인이나 인형의 부속품 무시
        if (shooter != null && (other.gameObject == shooter || other.transform.IsChildOf(shooter.transform)))
        {
            return; 
        }

        // 2. 투명한 트리거 감지 구역 무시
        if (other.isTrigger)
        {
            return; 
        }

        // 3. 이름에 특정 단어가 포함된 오브젝트 무시 (태그 에러 방지를 위해 통합)
        foreach (string keyword in ignoreKeywords)
        {
            if (other.name.Contains(keyword))
            {
                return; 
            }
        }

        // 4. ★ [수정] 태그(Tag) 대신 'Box' 스크립트가 붙어있는지 직접 확인하여 충돌 처리!
        // 이 방식 덕분에 유니티 에디터에서 "Box" 태그를 따로 등록하지 않아도 됩니다.
        Box boxScript = other.GetComponent<Box>();
        if (boxScript != null)
        {
            boxScript.OnHitByLaser();
        }

        // 5. 진짜 장애물 충돌 시 폭발 이펙트 생성 및 삭제
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3f); 
        }

        Debug.Log($"레이저가 장애물에 충돌함: {other.gameObject.name}");
        
        // Player 태그는 유니티 기본 내장 태그라 에러가 나지 않습니다.
        if (other.CompareTag("Player"))
        {
            Debug.Log("★ 플레이어 적중!");
        }

        Destroy(gameObject); 
    }
}