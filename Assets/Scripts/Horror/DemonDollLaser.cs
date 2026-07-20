using System.Collections;
using UnityEngine;

public class DemonDollLaser : MonoBehaviour
{
    [Header("=== 연결할 오브젝트들 ===")]
    public Transform player;          
    public Transform dollHead;        
    public Transform laserOrigin;     

    [Header("=== 총알 세팅 ===")]
    public GameObject bulletPrefab;   
    public float bulletSpeed = 35f;   

    [Header("=== 이펙트 세팅 ===")]
    [Tooltip("인형이 기를 모을 때 눈에 생성될 이펙트 프리팹")]
    public GameObject chargeEffectPrefab;

    [Header("=== AI 탐지 및 패턴 설정 ===")]
    public float detectionRange = 200f; 
    public float chargeTime = 1.5f;     
    public float attackCooldown = 3f;   

    [Header("=== 회전 속도 ===")]
    public float bodyRotationSpeed = 6f; 
    public float headRotationSpeed = 6f;  

    [Header("=== 각도 및 축 보정 ===")]
    public float maxLookDownAngle = 60f; 
    public float maxLookUpAngle = -20f;  
    public float bodyAngleOffset = 0f; 

    private bool isTracking = false;   
    private bool isSearching = true;   

    // 실행 중인 AI 코루틴을 안전하게 끄고 켜기 위해 저장하는 변수
    private Coroutine aiCoroutine;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; 
    }

    // ★ [핵심 수정] 오브젝트가 SetActive(true)로 켜질 때마다 매번 실행됩니다.
    void OnEnable()
    {
        // 상태를 초기 탐지 상태로 깨끗하게 리셋합니다.
        isSearching = true;
        isTracking = false;

        // 혹시 이미 돌고 있을지 모르는 코루틴을 방어적으로 한 번 끄고 새로 시작합니다.
        if (aiCoroutine != null)
        {
            StopCoroutine(aiCoroutine);
        }
        aiCoroutine = StartCoroutine(DollAIStateRoutine());
        Debug.Log("🤖 인형 활성화: 레이저 AI 루프 시동 완료!");
    }

    // ★ [핵심 수정] 오브젝트가 SetActive(false)로 꺼질 때 실행됩니다.
    void OnDisable()
    {
        // 백그라운드에 좀비처럼 남아있을 수 있는 코루틴을 완전히 박멸합니다.
        if (aiCoroutine != null)
        {
            StopCoroutine(aiCoroutine);
            aiCoroutine = null;
            Debug.Log("🔒 인형 비활성화: 레이저 AI 루프 안전 정지 완료.");
        }
    }

    void LateUpdate()
    {
        if (player == null || dollHead == null || laserOrigin == null) return;

        if (isTracking)
        {
            // 1. 몸통 수평 회전
            Vector3 bodyTargetDir = player.position - transform.position;
            bodyTargetDir.y = 0f; 
            if (bodyTargetDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetBodyRot = Quaternion.LookRotation(bodyTargetDir);
                targetBodyRot *= Quaternion.Euler(0f, bodyAngleOffset, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetBodyRot, Time.deltaTime * bodyRotationSpeed);
            }

            // 2. 머리 상하 조준
            Vector3 dirToPlayer = player.position - laserOrigin.position;
            if (dirToPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetLaserWorldRot = Quaternion.LookRotation(dirToPlayer, transform.up);
                Quaternion targetHeadWorldRot = targetLaserWorldRot * Quaternion.Inverse(laserOrigin.localRotation);
                Quaternion targetHeadLocalRot = Quaternion.Inverse(dollHead.parent.rotation) * targetHeadWorldRot;

                Vector3 localEuler = targetHeadLocalRot.eulerAngles;
                float pitch = localEuler.x;
                if (pitch > 180f) pitch -= 360f;
                pitch = Mathf.Clamp(pitch, maxLookUpAngle, maxLookDownAngle);

                Quaternion filteredTargetLocalRot = Quaternion.Euler(pitch, 0f, 0f);
                dollHead.localRotation = Quaternion.Slerp(dollHead.localRotation, filteredTargetLocalRot, Time.deltaTime * headRotationSpeed);
            }
        }
    }

    IEnumerator DollAIStateRoutine()
    {
        while (true)
        {
            if (isSearching && player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer <= detectionRange)
                {
                    isSearching = false; 
                    isTracking = true;   

                    // 차징 이펙트 생성
                    GameObject currentChargeFX = null;
                    if (chargeEffectPrefab != null && laserOrigin != null)
                    {
                        currentChargeFX = Instantiate(chargeEffectPrefab, laserOrigin.position, laserOrigin.rotation, laserOrigin);
                    }

                    yield return new WaitForSeconds(chargeTime);

                    // 차징 이펙트 삭제
                    if (currentChargeFX != null)
                    {
                        Destroy(currentChargeFX);
                    }

                    // 레이저 발사
                    FireLaserBullet(); 

                    yield return new WaitForSeconds(attackCooldown);

                    isSearching = true;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void FireLaserBullet()
    {
        if (bulletPrefab == null || laserOrigin == null || player == null) return;

        // 오프셋을 0.5f로 조절하여 인형 바로 앞에서 스폰 (벽 너머 발사 버그 해결)
        Vector3 spawnPosition = laserOrigin.position + (laserOrigin.forward * 0.5f);
        Vector3 perfectFireDirection = (player.position - spawnPosition).normalized;

        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(perfectFireDirection));
        
        bullet.transform.localScale = new Vector3(0.5f, 0.5f, 4.0f);

        LaserProjectile bulletScript = bullet.GetComponent<LaserProjectile>();
        if (bulletScript == null)
        {
            bulletScript = bullet.AddComponent<LaserProjectile>(); 
        }
        bulletScript.shooter = this.gameObject; 

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false; 
            rb.linearVelocity = perfectFireDirection * bulletSpeed;
        }

        Collider bulletCollider = bullet.GetComponent<Collider>();
        if (bulletCollider != null)
        {
            bulletCollider.isTrigger = true; 
        }

        Destroy(bullet, 10f); 
    }
}