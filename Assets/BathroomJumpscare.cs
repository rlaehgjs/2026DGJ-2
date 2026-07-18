using System.Collections;
using UnityEngine;

public class BathroomJumpscare : MonoBehaviour
{
    [Header("=== 깜놀 대상 ===")]
    public GameObject ghostCanvasImage; // 화면에 꽉 채워둔 GhostImage가 들어갈 칸

    [Header("=== 타이밍 설정 ===")]
    public float delayTime = 1f;         // 열쇠 집고 귀신 나올 때까지 가만히 있을 시간 (1초)
    public float ghostDuration = 1.2f;   // 귀신 이미지가 화면에 켜져 있을 시간 (1.2초)

    private bool isPickedUp = false;     // 귀신이 두 번 나오지 않게 막아주는 안전장치

    // 플레이어가 열쇠(Trigger)에 몸을 부딪히면 유니티가 자동으로 알아채고 실행하는 함수
    private void OnTriggerEnter(Collider other)
    {
        // 부딪힌 물체의 태그가 "Player"이고, 아직 귀신이 안 나왔었다면 실행!
        if (other.CompareTag("Player") && !isPickedUp)
        {
            isPickedUp = true; // "이제 실행했어!"라고 표시

            // 열쇠를 먹은 것처럼 보이게 외형(Mesh)과 충돌창(Collider)을 먼저 꺼서 숨깁니다.
            if (GetComponent<MeshRenderer>() != null) GetComponent<MeshRenderer>().enabled = false;
            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;

            // ★ 1초 대기 타이머(코루틴) 작동 시작!
            StartCoroutine(JumpscareRoutine());
        }
    }

    // 시간 지연(타이머)을 담당하는 특수 함수
    private IEnumerator JumpscareRoutine()
    {
        // 1. 설정한 시간(1초)만큼 가만히 대기합니다. (묘한 정적 연출)
        yield return new WaitForSeconds(delayTime);

        // 2. 1초가 지나면 숨겨놨던 2D 귀신 이미지를 짜잔! 하고 켜줍니다.
        if (ghostCanvasImage != null) ghostCanvasImage.SetActive(true);

        // 3. 귀신이 깜놀시키는 시간(1.2초) 동안 유지하며 대기
        yield return new WaitForSeconds(ghostDuration);
        
        // 4. 시간이 다 되면 귀신 이미지를 다시 꺼서 숨깁니다.
        if (ghostCanvasImage != null) ghostCanvasImage.SetActive(false);

        // 5. 모든 할 일을 마친 열쇠 오브젝트를 게임 세상에서 완전히 삭제합니다.
        Destroy(gameObject);
    }
}