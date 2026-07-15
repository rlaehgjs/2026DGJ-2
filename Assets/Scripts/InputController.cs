using UnityEngine;

public class InputController : MonoBehaviour
{
    private MovementController movementController;
    private ViewController viewController;
    // 나중에 인벤토리 변수 추가할 곳

    void Start()
    {
        movementController = GetComponent<MovementController>();
        viewController = GetComponent<ViewController>();
        // 인벤토리 가져오기
    }

    void Update()
    {
        // 스페이스 바 입력
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("점프 시도!");
            movementController.Jump();
        }

        // WASD 입력
        float vertical_axis = Input.GetAxis("Vertical");
        float horizontal_axis = Input.GetAxis("Horizontal");
        movementController.SetMoveDirection(new Vector3(horizontal_axis, 0, vertical_axis));

        // 시점 제어 입력 추가
        // 마우스의 움직임(이동량) 받아오기
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        viewController.LookAtMouse(mouseX, mouseY);

        // 인벤토리 상호작용 입력 추가
    }
}
