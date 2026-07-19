using UnityEngine;

public class Box : MonoBehaviour
{
    // 상자가 가질 수 있는 아이템 종류 정의
    public enum ItemType { Empty, Hammer, Health }
    
    [Header("상자 안의 아이템 (매니저가 자동으로 정해줌)")]
    public ItemType containsItem = ItemType.Empty;

    // 테스트용으로 아이템이 나올 때 띄울 큐브 프리팹 (나중에 에셋으로 교체 가능)
    [Header("아이템 스폰용 프리팹")]
    public GameObject hammerPrefab;
    public GameObject healthPrefab;

    // 레이저에 맞았을 때 호출될 함수
    public void OnHitByLaser()
    {
        // 아이템 종류에 따라 동적 생성 (꽝이 아니면)
        if (containsItem == ItemType.Hammer && hammerPrefab != null)
        {
            Instantiate(hammerPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Debug.Log("망치를 발견했습니다!");
        }
        else if (containsItem == ItemType.Health && healthPrefab != null)
        {
            Instantiate(healthPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Debug.Log("체력 회복 아이템을 발견했습니다!");
        }
        else
        {
            Debug.Log("꽝! 아무것도 없습니다.");
        }

        // 상자 오브젝트 파괴 (사라짐)
        Destroy(gameObject);
    }
// Box 스크립트 맨 아래 (종료 중괄호 바로 앞)에 추가
private void OnTriggerEnter(Collider other)
{
    // 만약 나한테 부딪힌 물체의 이름이 "Laser" 라면?
    if (other.gameObject.name.Contains("Laser") || other.CompareTag("Laser"))
    {
        OnHitByLaser(); // 상자 터지는 함수 실행!
    }
}}