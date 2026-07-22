using System.Collections;
using UnityEngine;

public class StairDoorSkeleton : MonoBehaviour
{
    [Header("=== 드래그 앤 드롭 연결 ===")]
    [Tooltip("씬에 놓여있는 '냉대캡슐' 오브젝트를 여기에 드래그해서 넣으세요.")]
    [SerializeField] private GameObject capsuleObject;

    [Tooltip("문 앞에 나타날 '3D 해골' 오브젝트를 여기에 드래그해서 넣으세요.")]
    [SerializeField] private GameObject ghostSkeleton;

    [Header("=== 타이밍 설정 ===")]
    [Tooltip("해골이 멈춰서 서있을 시간 (3초)")]
    [SerializeField] private float ghostDuration = 3.0f;

    [Tooltip("스르르 투명해지며 사라지는 시간 (1.5초)")]
    [SerializeField] private float fadeDuration = 1.5f;

    private bool isTriggered = false; // 중복 발동 방지용 안전장치
    private Renderer[] skeletonRenderers;

    private void Start()
    {
        if (ghostSkeleton != null)
        {
            // 해골의 투명도를 조절하기 위해 하위 렌더러들을 미리 챙겨두고, 처음엔 숨겨둡니다.
            skeletonRenderers = ghostSkeleton.GetComponentsInChildren<Renderer>();
            ghostSkeleton.SetActive(false);
        }
    }

    // 플레이어가 계단 문 앞 트리거에 부딪혔을 때 실행
    private void OnTriggerEnter(Collider other)
    {
        // 1. 부딪힌 물체가 플레이어인지 확인 (태그 또는 Rigidbody 체크)
        Rigidbody playerBody = other.attachedRigidbody;
        bool isPlayer = (playerBody != null && playerBody.CompareTag("Player")) || other.CompareTag("Player");

        if (isPlayer && !isTriggered)
        {
            // 2. 캡슐을 먹었는지 확인
            // PickupInteractable이 캡슐을 파밍하면 SetActive(false)로 끄기 때문에, 
            // 캡슐이 꺼져있거나(activeSelf == false) 파괴되었다면 "먹었다"고 판단합니다.
            bool isCapsuleCollected = (capsuleObject == null) || !capsuleObject.activeSelf;

            if (isCapsuleCollected)
            {
                isTriggered = true; // 이벤트 작동!
                Debug.Log("[StairDoorSkeleton] 냉대캡슐 획득 확인 완료! 해골 이벤트를 시작합니다.");
                StartCoroutine(SkeletonJumpscareRoutine());
            }
            else
            {
                Debug.Log("[StairDoorSkeleton] 플레이어가 다가왔지만, 아직 냉대캡슐을 집지 않았습니다.");
            }
        }
    }

    // 3초 대기 후 서서히 사라지는 코루틴
    private IEnumerator SkeletonJumpscareRoutine()
    {
        // 1. 해골 투명도를 100%(불투명)로 맞추고 짠! 하고 나타납니다.
        SetSkeletonAlpha(1.0f);
        if (ghostSkeleton != null) ghostSkeleton.SetActive(true);

        // 2. 지정한 시간(3초) 동안 그대로 유지합니다.
        yield return new WaitForSeconds(ghostDuration);

        // 3. 지정한 시간(1.5초) 동안 알파값을 줄여 스르르 사라지게 합니다.
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime / fadeDuration);
            SetSkeletonAlpha(alpha);
            yield return null; // 다음 프레임까지 대기
        }

        // 4. 완전히 사라지면 해골을 비활성화합니다.
        if (ghostSkeleton != null) ghostSkeleton.SetActive(false);
    }

    // 해골 머티리얼의 알파값(투명도)을 변경하는 함수
    private void SetSkeletonAlpha(float alpha)
    {
        if (skeletonRenderers == null) return;

        foreach (Renderer rend in skeletonRenderers)
        {
            foreach (Material mat in rend.materials)
            {
                // URP Lit / Unlit 셰이더 기준 (_BaseColor)
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = alpha;
                    mat.SetColor("_BaseColor", c);
                }
                // Built-in Standard 셰이더 기준 (_Color)
                else if (mat.HasProperty("_Color"))
                {
                    Color c = mat.GetColor("_Color");
                    c.a = alpha;
                    mat.SetColor("_Color", c);
                }
            }
        }
    }
}