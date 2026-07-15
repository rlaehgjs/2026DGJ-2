using UnityEngine;

public class InputController : MonoBehaviour
{
    private PlayerMovement movementController;
    // 나중에 인벤토리 변수 추가할 곳

    void Start()
    {
        movementController = GetComponent<PlayerMovement>();
        // 인벤토리 가져오기
    }

    void Update()
    {
        bool is_to_stop = true;

        // 스페이스 바 입력
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("점프 시도!");
            movementController.Jump();
        }
        // WASD 입력
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("앞으로 이동 시도!");
            movementController.SetForward();
            is_to_stop = false;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Debug.Log("뒤로 이동 시도!");
            movementController.SetBackward();
            is_to_stop = false;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Debug.Log("왼쪽으로 이동 시도!");
            movementController.SetLeft();
            is_to_stop = false;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Debug.Log("오른쪽으로 이동 시도!");
            movementController.SetRight();
            is_to_stop = false;
        }
        if (is_to_stop)
        {
            movementController.Stop();
        }
        // 인벤토리 상호작용 입력 추가
        // 시점 제어 입력 추가
    }
}
