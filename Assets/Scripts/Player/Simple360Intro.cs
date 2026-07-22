using UnityEngine;
using System.Collections;

public class Simple360Intro : MonoBehaviour
{
    [Header("Target & Camera Settings")]
    public Transform playerTransform;      // 아이스크림 플레이어
    public Camera cutsceneCamera;          // ★ 360도 연출용 임시 카메라
    public Camera mainGameplayCamera;      // ★ 게임 플레이용 메인 카메라

    [Header("Transform Settings")]
    public float distance = 2.5f;          // 플레이어와의 거리
    public float height = 0.8f;            // 카메라 높이
    public float rotationSpeed = 90f;      // 초당 회전 각도

    [Header("Angle Settings")]
    public float startAngle = 120f;        // 시작 각도 (120도)
    public float maxRotationAngle = 270f;  // 시계 방향으로 회전할 제한 각도 (270도)

    [Header("Player Control")]
    public MonoBehaviour playerControlScript; // 플레이어 이동 스크립트

    private void Start()
    {
        StartCoroutine(RotateAroundPlayer());
    }

    private IEnumerator RotateAroundPlayer()
    {
        // 1. 초기 상태 설정: 연출용 카메라만 켜고, 메인 카메라 및 플레이어 조작은 비활성화
        if (cutsceneCamera != null) cutsceneCamera.gameObject.SetActive(true);
        if (mainGameplayCamera != null) mainGameplayCamera.gameObject.SetActive(false);
        if (playerControlScript != null) playerControlScript.enabled = false;

        float progressAngle = 0f;

        while (progressAngle < maxRotationAngle)
        {
            progressAngle += rotationSpeed * Time.deltaTime;

            // 시계 방향 회전 (startAngle - progressAngle)
            float currentAngle = startAngle - progressAngle;
            float rad = currentAngle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Sin(rad) * distance, height, -Mathf.Cos(rad) * distance);

            // 임시(컷씬) 카메라의 위치와 회전 변경
            if (cutsceneCamera != null)
            {
                cutsceneCamera.transform.position = playerTransform.position + offset;
                cutsceneCamera.transform.LookAt(playerTransform.position + Vector3.up * (height * 0.5f));
            }

            yield return null;
        }

        // 2. 연출 완료: 메인 플레이용 카메라 켜고, 컷씬 카메라 끄기 & 조작 복구
        if (cutsceneCamera != null) cutsceneCamera.gameObject.SetActive(false);
        if (mainGameplayCamera != null) mainGameplayCamera.gameObject.SetActive(true);
        if (playerControlScript != null) playerControlScript.enabled = true;
    }
}