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

    [Header("=== 이펙트 세팅 (NEW) ===")]
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

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; 

        StartCoroutine(DollAIStateRoutine());
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

                    // ★ [차징 이펙트 생성]
                    // 눈(laserOrigin)의 자식으로 생성하여 인형이 고개를 돌려도 이펙트가 눈에 딱 붙어 쫓아오게 합니다.
                    GameObject currentChargeFX = null;
                    if (chargeEffectPrefab != null && laserOrigin != null)
                    {
                        currentChargeFX = Instantiate(chargeEffectPrefab, laserOrigin.position, laserOrigin.rotation, laserOrigin);
                    }

                    // 지정된 충전 시간 동안 대기
                    yield return new WaitForSeconds(chargeTime);

                    // ★ [차징 이펙트 삭제] 발사하기 직전에 충전 이펙트를 지워줍니다.
                    if (currentChargeFX != null)
                    {
                        Destroy(currentChargeFX);
                    }

                    // 레이저 발사!
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

        Vector3 spawnPosition = laserOrigin.position + (laserOrigin.forward * 3.5f);
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