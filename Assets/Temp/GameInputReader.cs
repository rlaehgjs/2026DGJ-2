using UnityEngine;

public class GameInputReader : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    // 나중에 인벤토리 변수 추가할 곳

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        // 인벤토리 가져오기
    }

    void Update()
    {
        // 점프
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("점프 시도!");
            playerMovement.Jump();
        }

        // WASD 입력
        float vertical_axis = Input.GetAxis("Vertical");
        float horizontal_axis = Input.GetAxis("Horizontal");
        playerMovement.SetMoveDirection(new Vector3(horizontal_axis, 0, vertical_axis));

        // 마우스 시점
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        playerLook.LookAtMouse(mouseX, mouseY);

        // E 상호작용
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E 눌림!");
        }

        // ESC 일시 정지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC 눌림!");
        }

    }
}
