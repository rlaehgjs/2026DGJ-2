using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private List<float> moveSpeedMultipliers = new List<float>()
    {0.8f, 1.0f, 1.1f};
    private List<float> linearDampings = new List<float>()
    {1.0f, 0.5f, 0f};
    private List<float> gravityMultipliers = new List<float>()
    {1.0f, 1.5f, 2.0f};
    private Rigidbody rigidBody;
    private Vector3 moveDirection;
    private float baseMoveSpeed = 5f;
    private float baseGravity;
    private float baseJumpForce = 10f;
    private float freezeStateMoveSpeedMultiplier = 1.0f;
    private float environmentMoveSpeedMultiplier = 1.0f;
    private float scaledMoveSpeed;
    private float scaledJumpForce;
    private bool isGrounded = false;

    private PlayerMeltSystem playerMeltSystem;

    // 추가된 부분: OnEnable보다 먼저 실행됨
    void Awake()
    {
        playerMeltSystem = GetComponent<PlayerMeltSystem>();
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        baseGravity = Physics.gravity.y;
        scaledMoveSpeed = baseMoveSpeed;
        scaledJumpForce = baseJumpForce;
        ApplyFreezeState(FreezeState.Frozen);
    }


    // WASD 이동
    void Update()
    {
        if (!moveDirection.Equals(Vector3.zero))
        {
            Debug.Log("움직임 시도! " + moveDirection);
            Move();
        }
    }

    public void Move()
    {
        Vector3 originalVelocity = rigidBody.linearVelocity;
        Vector3 newVelocity = scaledMoveSpeed * moveDirection;
        // y축 속도는 점프 중일 수도 있으니까 그대로 둠
        rigidBody.linearVelocity = new Vector3(newVelocity.x, originalVelocity.y, newVelocity.z);
        Debug.Log("현재 속도: " + rigidBody.linearVelocity);
    }

    // 수평 이동 방향 설정
    public void SetMoveDirection(Vector3 direction)
    {
        ApplyLookDirection(direction);
    }

    // 보는 방향을 기준으로 이동 방향 조정
    private void ApplyLookDirection(Vector3 direction)
    {
        // 위아래로 보고 있을 때 이동 방향이 잘못되지 않도록 수평 회전량만 남김
        Vector3 lookDirection = transform.forward;
        lookDirection.y = 0;
        lookDirection.Normalize(); // 크기를 다시 1로 맞춤
        
        Quaternion horizontalRotation = Quaternion.LookRotation(lookDirection);
        moveDirection = horizontalRotation * direction.normalized;
    }


    // 점프
    public void Jump()
    {
        if (isGrounded)
        {
            Debug.Log("점프 성공!");
            isGrounded = false;
            rigidBody.AddForce(Vector3.up * scaledJumpForce, ForceMode.Impulse);
        }
    }

    // 착지
    private void OnCollisionEnter(Collision collision)
    {
        // 땅(Ground) 태그를 가진 오브젝트와 부딪혔을 때
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log("착지 완료!");
        }
    }


    // 이벤트 처리
    void OnEnable()
    {
        // 이벤트 구독 (+= 연산자 사용)
        playerMeltSystem.OnFreezeStateChanged += ApplyFreezeState;
    }

    void OnDisable()
    {
        // 이벤트 구독 해제 (-= 연산자 필수)
        playerMeltSystem.OnFreezeStateChanged -= ApplyFreezeState;
    }

    private void ApplyFreezeState(FreezeState state)
    {
        int state_index = (int)state;
        freezeStateMoveSpeedMultiplier = moveSpeedMultipliers[state_index];
        scaledMoveSpeed = baseMoveSpeed * environmentMoveSpeedMultiplier * freezeStateMoveSpeedMultiplier;
        rigidBody.linearDamping = linearDampings[state_index];
        Physics.gravity = new Vector3(0f, baseGravity * gravityMultipliers[state_index], 0f);
    }


    // Setter
    public void SetEnvironmentSpeedMultiplier(float multiplier)
    {
        environmentMoveSpeedMultiplier = multiplier;
        scaledMoveSpeed = baseMoveSpeed * environmentMoveSpeedMultiplier * freezeStateMoveSpeedMultiplier;
    }

    public void SetMeltStageJumpMultiplier(float multiplier)
    {
        scaledJumpForce = baseJumpForce * multiplier;
    }
}
