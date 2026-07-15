using UnityEngine;

public class ViewController : MonoBehaviour
{
    public float mouseSensitivity = 500f; // 마우스 감도
    // 나중에 스크립터블 오브젝트로 빼서 UI 쪽에서 다루기 쉽게 하자

    private float xRotation = 0f; // 위아래 회전값을 저장할 변수

    void Start()
    {
        // 마우스 커서를 화면 중앙에 고정하고 숨김
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void LookAtMouse(float mouseX, float mouseY)
    {
        mouseX *= mouseSensitivity * Time.deltaTime;
        mouseY *= mouseSensitivity * Time.deltaTime;
        RotateVertical(mouseY);
        RotateHorizontal(mouseX);
    }

    private void RotateVertical(float mouseY)
    {
        // 마우스 Y축 움직임으로 위아래(X축 기준) 회전값 누적 (음수를 더해야 마우스를 올릴 때 위를 봄)
        xRotation -= mouseY;
        // 위아래로 지나치게 꺾이지 않도록 90도 제한 (목 꺾임 방지)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        // 카메라의 위아래(X축) 회전 적용
        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void RotateHorizontal(float mouseX)
    {
        // 마우스 X축 움직임으로 캐릭터 몸통을 좌우(Y축 기준)로 회전
        transform.Rotate(Vector3.up * mouseX);
    }
}
