using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 이 스크립트가 붙은 오브젝트에 AudioSource 컴포넌트가 없다면 자동으로 추가해주는 안전장치
[RequireComponent(typeof(AudioSource))]
public class StairDoorSkeleton : MonoBehaviour
{
    [Header("=== 드래그 앤 드롭 연결 ===")]
    [Tooltip("씬에 놓여있는 '냉대캡슐' 오브젝트를 여기에 드래그해서 넣으세요.")]
    [SerializeField] private GameObject capsuleObject;

    [Tooltip("문 앞에 나타날 모든 '3D 해골' 오브젝트들을 여기에 드래그해서 넣으세요.")]
    [SerializeField] private GameObject[] ghostSkeletons;

    [Header("=== 사운드 설정 ===")]
    [Tooltip("소리 음원이 담긴 자기 자신의 AudioSource 컴포넌트를 여기에 연결하세요.")]
    [SerializeField] private AudioSource jumpscareAudioSource;

    [Header("=== 타이밍 설정 ===")]
    [Tooltip("해골들이 멈춰서 실체가 있을 시간 (3초)")]
    [SerializeField] private float ghostDuration = 3.0f;

    [Tooltip("실체가 사라진 후 스르르 투명해지는 시간 (1.5초)")]
    [SerializeField] private float fadeDuration = 1.5f;

    private bool isTriggered = false; // 중복 발동 방지용 안전장치
    private List<Renderer> skeletonRenderers = new List<Renderer>();
    private List<Collider> skeletonColliders = new List<Collider>();

    private void Start()
    {
        // --- 1. 필수 컴포넌트 체크 ---
        if (jumpscareAudioSource == null)
        {
            jumpscareAudioSource = GetComponent<AudioSource>();
        }

        // --- 2. 해골 컴포넌트 수집 및 초기화 ---
        if (ghostSkeletons != null && ghostSkeletons.Length > 0)
        {
            foreach (GameObject skel in ghostSkeletons)
            {
                if (skel == null) continue;

                // 각 해골의 하위 렌더러 수집
                Renderer[] rends = skel.GetComponentsInChildren<Renderer>();
                skeletonRenderers.AddRange(rends);

                // 각 해골의 하위 콜라이더 수집
                Collider[] cols = skel.GetComponentsInChildren<Collider>();
                skeletonColliders.AddRange(cols);

                // 시작 시 모든 해골 비활성화
                skel.SetActive(false);
            }
        }
    }

    // 플레이어가 계단 문 앞 트리거에 진입했을 때 실행
    private void OnTriggerEnter(Collider other)
    {
        // 부딪힌 물체가 플레이어인지 확인
        if (other.transform.root.CompareTag("Player") && !isTriggered)
        {
            // 캡슐을 먹었는지 확인 (오브젝트가 꺼져있으면 획득한 것으로 판단)
            bool isCapsuleCollected = (capsuleObject == null) || !capsuleObject.activeSelf;

            if (isCapsuleCollected)
            {
                isTriggered = true; // 이벤트 작동!
                Debug.Log("[StairDoorSkeleton] 캡슐 획득 확인! 효과음과 함께 해골 이벤트를 시작합니다.");
                StartCoroutine(SkeletonJumpscareRoutine());
            }
        }
    }

    // 실체 등장 및 소리 재생 -> 대기 -> 실체 사라짐 -> 투명화 -> 소리 끔 코루틴
    private IEnumerator SkeletonJumpscareRoutine()
    {
        // 1. 모든 해골의 투명도를 100%(불투명)로 맞추고 활성화
        SetSkeletonsAlpha(1.0f);
        SetSkeletonsActive(true);

        // 2. 소리 재생 시작! (오타 수정됨)
        if (jumpscareAudioSource != null)
        {
            jumpscareAudioSource.Stop(); // 혹시 재생 중이면 멈추고
            jumpscareAudioSource.Play(); // 처음부터 재생
        }

        // 3. 모든 해골의 물리 충돌창(Collider)을 켜서 길을 막음
        SetSkeletonsPhysics(true);

        // [대기] 지정한 시간(3초) 동안 실체 유지
        yield return new WaitForSeconds(ghostDuration);

        // 4. 서서히 사라지기 직전에 물리 충돌창을 먼저 꺼서 통과 가능하게 만듦
        SetSkeletonsPhysics(false);

        // 5. [투명화 연출] 지정한 시간(1.5초) 동안 알파값을 줄여 동시에 스르르 사라지게 함
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime / fadeDuration);
            SetSkeletonsAlpha(alpha);
            yield return null;
        }

        // 6. 완전히 사라지면 모든 해골을 비활성화
        SetSkeletonsActive(false);

        // 7. 연출이 다 끝나는 시점에 소리를 확실하게 끕니다.
        if (jumpscareAudioSource != null)
        {
            jumpscareAudioSource.Stop();
            Debug.Log("[StairDoorSkeleton] 이벤트가 종료되어 소리를 끕니다.");
        }
    }

    // 모든 해골 오브젝트 활성화 / 비활성화 함수
    private void SetSkeletonsActive(bool active)
    {
        if (ghostSkeletons == null) return;

        foreach (GameObject skel in ghostSkeletons)
        {
            if (skel != null) skel.SetActive(active);
        }
    }

    // 모든 해골 머티리얼의 알파값(투명도) 일괄 변경 함수
    private void SetSkeletonsAlpha(float alpha)
    {
        if (skeletonRenderers == null) return;

        foreach (Renderer rend in skeletonRenderers)
        {
            if (rend == null) continue;

            foreach (Material mat in rend.materials)
            {
                // URP Lit / Unlit 셰이더 기준 프로퍼티 (_BaseColor)
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = alpha;
                    mat.SetColor("_BaseColor", c);
                }
                // Built-in Standard 셰이더 기준 프로퍼티 (_Color)
                else if (mat.HasProperty("_Color"))
                {
                    Color c = mat.GetColor("_Color");
                    c.a = alpha;
                    mat.SetColor("_Color", c);
                }
            }
        }
    }

    // 모든 해골의 물리 충돌창(Collider) 일괄 켜기 / 끄기 함수
    private void SetSkeletonsPhysics(bool enable)
    {
        if (skeletonColliders == null) return;

        foreach (Collider col in skeletonColliders)
        {
            if (col == null) continue;

            // 트리거 오브젝트 본인의 콜라이더는 제외하고, 해골 자식들의 콜라이더만 변경
            if (col.gameObject != this.gameObject)
            {
                col.enabled = enable;
            }
        }
    }
}