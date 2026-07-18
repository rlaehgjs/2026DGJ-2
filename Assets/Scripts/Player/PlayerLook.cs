using TMPro;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    private GameInputReader gameInputReader;
    private float mouseSensitivity = 500f; // 마우스 감도
    private float xRotation = 0f; // 위아래 회전값을 저장할 변수

    private void Awake()
    {
        gameInputReader = GetComponent<GameInputReader>();
    }

    void Start()
    {
        // 마우스 커서를 화면 중앙에 고정하고 숨김
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Vector2 lookInput = gameInputReader.LookInput;
        LookAtMouse(lookInput.x, lookInput.y);
    }

    public void LookAtMouse(float mouseX, float mouseY)
    {
        float degreeX = mouseX * mouseSensitivity * Time.deltaTime;
        float degreeY = mouseY * mouseSensitivity * Time.deltaTime;
        RotateVertical(degreeY);
        RotateHorizontal(degreeX);
    }

    private void RotateVertical(float degreeY)
    {
        // 마우스 Y축 움직임으로 위아래(X축 기준) 회전값 누적 (음수를 더해야 마우스를 올릴 때 위를 봄)
        xRotation -= degreeY;
        // 위아래로 지나치게 꺾이지 않도록 90도 제한 (목 꺾임 방지)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 카메라의 위아래(X축) 회전 적용
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void RotateHorizontal(float degreeX)
    {
        // 마우스 X축 움직임으로 캐릭터 몸통을 좌우(Y축 기준)로 회전
        transform.Rotate(Vector3.up * degreeX);
    }

    
    // Setter
    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = value;
    }
}
