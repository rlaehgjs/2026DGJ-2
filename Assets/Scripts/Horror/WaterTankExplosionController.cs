using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class WaterTankExplosionController : MonoBehaviour
{
    [SerializeField] private Transform waterVolume;
    private float waterVolumeOriginalY;
    [SerializeField] private ParticleSystem[] outletParticles;
    [SerializeField] private AudioSource floodAudioSource;
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField] private Collider escapeTrigger;
    [SerializeField] private Collider damageVolume;
    [SerializeField] private PlayerMeltSystem playerMeltSystem;

    [SerializeField] private float riseSpeed = 1f;
    [SerializeField] private float maxHeight = 8f;
    [SerializeField] private float damagePerSecond = 10.0f;

    [Header("Underwater Effects")]
    [SerializeField] private Volume underwaterVolume;
    private Collider waterCollider; // 큐브의 높이를 계산하기 위해 콜라이더 캐싱
    [SerializeField] private float effectTransitionSpeed = 5f;
    private bool isCurrentlyUnderwater = false;

    private bool isRunning;
    private bool isFinished;
    private Camera mainCamera;

    // 셰이더 글로벌 변수 ID 캐싱 (문자열 검색보다 성능이 훨씬 좋습니다)
    private static readonly int UnderwaterWeightId = Shader.PropertyToID("_UnderwaterWeight");

    private void Start()
    {
        mainCamera = Camera.main;

        // waterVolume 오브젝트에서 Collider를 가져옵니다.
        if (waterVolume != null)
        {
            waterCollider = waterVolume.GetComponent<Collider>();
        }

        waterVolumeOriginalY = waterVolume.position.y;

        // 시작할 때 효과 수치 0으로 초기화
        if (underwaterVolume != null) underwaterVolume.weight = 0f;
        Shader.SetGlobalFloat(UnderwaterWeightId, 0f);
        RenderSettings.fogDensity = 0f;
    }

    private void Update()
    {
        UpdateUnderwaterEffect();
    }

    private void UpdateUnderwaterEffect()
    {
        if (mainCamera == null || waterVolume == null) return;

        float waterSurfaceY = waterCollider.bounds.max.y;
        float cameraY = mainCamera.transform.position.y;

        // 카메라가 물아래 있는지 판별
        isCurrentlyUnderwater = cameraY < waterSurfaceY;
        
        float targetWeight = isCurrentlyUnderwater ? 1f : 0f;

        if (underwaterVolume != null)
        {
            // 포스트 프로세싱 볼륨이 지정된 속도에 맞춰 부드럽게 0 <-> 1로 전환됨
            underwaterVolume.weight = Mathf.Lerp(underwaterVolume.weight, targetWeight, Time.deltaTime * effectTransitionSpeed);

            // 🔥 추가된 부분: 값이 0.01 이하로 떨어지면 그냥 깔끔하게 0으로 덮어씌웁니다.
            if (targetWeight == 0f && underwaterVolume.weight < 0.01f)
            {
                underwaterVolume.weight = 0f;
            }

            // 셰이더에도 이 부드러운 Weight 값을 그대로 전달! 
            // (물 밖으로 나오면 서서히 일렁임이 멈춤)
            Shader.SetGlobalFloat(UnderwaterWeightId, underwaterVolume.weight);
        
            // 추가된 부분: Volume의 Weight 값에 맞춰 안개의 농도(Density)도 부드럽게 조절합니다.
            // (만약 Exponential 모드가 아니라면 RenderSettings.fog = checkUnderwater; 로 단순히 켜고 끄셔도 됩니다.)
            RenderSettings.fogDensity = Mathf.Lerp(0f, 0.02f, underwaterVolume.weight); // 0.05f는 원래 세팅하셨던 안개 농도값으로 변경하세요
        }
    }

    public void BeginFlood()
    {
        Debug.Log("홍수!!!");
        if (isRunning || isFinished) return;

        isRunning = true;
        StartCoroutine(RunFlood());
    }

    private IEnumerator RunFlood()
    {
        if (impactClips.Length > 0)
        {
            if (floodAudioSource != null)
            {
                floodAudioSource.PlayOneShot(impactClips[0]);
                yield return new WaitForSeconds(0.45f);
                floodAudioSource.PlayOneShot(impactClips[1]);
            }
        }

        foreach (var ps in outletParticles)
        {
            if (ps != null) ps.Play();
        }

        while (isRunning && !isFinished)
        {
            // 플레이어가 탈출 트리거 안에 들어가면 루프 종료
            if (escapeTrigger != null && escapeTrigger.bounds.Contains(playerMeltSystem.transform.position))
            {
                break;
            }

            Vector3 pos = waterVolume.position;
            pos.y = Mathf.Min(pos.y + riseSpeed * Time.deltaTime, maxHeight);
            waterVolume.position = pos;

            if (damageVolume != null && playerMeltSystem != null)
            {
                if (damageVolume.bounds.Contains(playerMeltSystem.transform.position))
                {
                    playerMeltSystem.Damage(damagePerSecond * Time.deltaTime);
                }
            }

            if (pos.y >= maxHeight)
            {
                break;
            }

            yield return null;
        }

        // 파티클 멈추기
        foreach (var ps in outletParticles)
        {
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // 소리 멈추기
        if (floodAudioSource != null)
        {
            floodAudioSource.Stop();
        }

        isFinished = true;
        isRunning = false;

        // 포스트 프로세싱 볼륨과 셰이더 글로벌 변수 초기화
        if (underwaterVolume != null) underwaterVolume.weight = 0f;
        Shader.SetGlobalFloat(UnderwaterWeightId, 0f);
        RenderSettings.fogDensity = 0f;

        // 물 높이 초기화
        waterVolume.position = new Vector3(waterVolume.position.x, waterVolumeOriginalY, waterVolume.position.z);
    }

    // 이벤트 처리
    void OnEnable()
    {
        // 이벤트 구독 (+= 연산자 사용)
        playerMeltSystem.OnHpDepleted += HandlePlayerHpDepleted;
    }

    void OnDisable()
    {
        // 이벤트 구독 해제 (-= 연산자 필수)
        playerMeltSystem.OnHpDepleted -= HandlePlayerHpDepleted;
    }

    private void HandlePlayerHpDepleted()
    {
        // 플레이어 HP가 0이 되면 홍수 루프를 종료
        isRunning = false;
    }

}