using UnityEngine;
using System;

public class GameInputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public event Action JumpPressed;
    public event Action InteractPressed;
    public event Action PausePressed;
    [SerializeField]
    private bool isJumpEnabled = true;
    [SerializeField]
    private bool isMoveEnabled = true;
    [SerializeField]
    private bool isLookEnabled = true;
    [SerializeField]
    private bool isInteractionEnabled = true;

    void Update()
    {
        MoveInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"));

        LookInput = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y"));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            InteractPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PausePressed?.Invoke();
        }

    }


    // 입력 활성화/비활성화
    public void EnableJump() => isJumpEnabled = true;
    public void EnableMove() => isMoveEnabled = true;
    public void EnableLook() => isLookEnabled = true;
    public void EnableInteraction() => isInteractionEnabled = true;
    public void DisableJump() => isJumpEnabled = false;
    public void DisableMove() => isMoveEnabled = false;
    public void DisableLook() => isLookEnabled = false;
    public void DisableInteraction() => isInteractionEnabled = false;
}
