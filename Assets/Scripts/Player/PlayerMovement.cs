using System.Collections.Generic;
using UnityEngine;
using System;


public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private List<float> moveSpeedMultipliers = new List<float>()
    {0.8f, 1.0f, 1.1f};
    [SerializeField]
    private List<float> extraGravityMultipliers = new List<float>()
    {0f, 2.0f, 3.0f};
    [SerializeField]
    private List<float> frictionsPerStage = new List<float>()
    {20.0f, 10.0f, 0f};

    private Rigidbody rigidBody;
    private Vector3 moveDirection;
    private bool isGrounded = false;
    [SerializeField, Range(0f, 1f)]
    private float groundNormalThreshold = 0.65f;

    [SerializeField]
    private float baseMoveSpeed = 5f;
    [SerializeField]
    private float baseJumpForce = 10f;

    private float freezeStateMoveSpeedMultiplier = 1.0f;
    private float environmentMoveSpeedMultiplier = 1.0f;
    private float scaledMoveSpeed;
    private float scaledJumpForce;
    private float friction;
    private float extraGravity;
    private FreezeState currentState = FreezeState.Frozen;

    private PlayerMeltSystem playerMeltSystem;
    private GameInputReader gameInputReader;

    // 추가된 부분: OnEnable보다 먼저 실행됨
    void Awake()
    {
        playerMeltSystem = GetComponent<PlayerMeltSystem>();
        gameInputReader = GetComponent<GameInputReader>();
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        scaledMoveSpeed = baseMoveSpeed;
        scaledJumpForce = baseJumpForce;
        ApplyFreezeState(FreezeState.Frozen);
    }

    private void FixedUpdate()
    {
        // 현재 물리 프레임의 접촉 정보는 OnCollisionStay에서 다시 채운다.
        isGrounded = false;
    }


    // WASD 이동
    void Update()
    {
        Vector2 moveInput = gameInputReader.MoveInput;
        SetMoveDirection(new Vector3(
            moveInput.x,
            0f,
            moveInput.y));

        if (moveDirection != Vector3.zero)
        {
            Move();
        }

        AdjustJump();
        AdjustMove();
    }

    public void Move()
    {
        Vector3 originalVelocity = rigidBody.linearVelocity;
        Vector3 newVelocity = scaledMoveSpeed * moveDirection;

        // y축 속도는 점프 중일 수도 있으니까 그대로 둠
        rigidBody.linearVelocity = new Vector3(newVelocity.x, originalVelocity.y, newVelocity.z);
        // Debug.Log("현재 속도: " + rigidBody.linearVelocity);
        // Debug.Log("isGrounded" + isGrounded);
    }

    // 마찰 적용
    private void AdjustMove()
    {
        int numState = Enum.GetNames(typeof(FreezeState)).Length;
        for (int i = 0; i < numState; ++i)
        {
            FreezeState state = (FreezeState)i;
            float frictionToApply = friction * Time.deltaTime;
            if (currentState == state)
            {
                ApplyFriction(frictionToApply);
            }
        }
    }

    private void ApplyFriction(float value)
    {
        // Lerp에서 y값이 영향을 주지 않도록 x, z만 가져옴
        Vector3 currentPlanarVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);
        Vector3 zeroPlanarVelocity = Vector3.zero;
        Vector3 smoothedVelocity = Vector3.Lerp(currentPlanarVelocity, zeroPlanarVelocity, value);
       
        // 최종 속도 적용 (중력 Y값은 그대로 유지)
        rigidBody.linearVelocity = new Vector3(smoothedVelocity.x, rigidBody.linearVelocity.y, smoothedVelocity.z);
    }


    // 수평 이동 방향 설정
    private void SetMoveDirection(Vector3 direction)
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
    private void Jump()
    {
        if (isGrounded)
        {
            // Debug.Log("점프 성공!");
            isGrounded = false;
            rigidBody.AddForce(Vector3.up * scaledJumpForce, ForceMode.Impulse);
        }
    }

    // 저중력 적용
    private void AdjustJump()
    {
        if (!isGrounded)
        {
            rigidBody.AddForce(extraGravity * Time.deltaTime * Vector3.down, ForceMode.Impulse);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 contactNormal = collision.GetContact(i).normal;
            if (IsGroundNormal(contactNormal, groundNormalThreshold))
            {
                isGrounded = true;
            }
        }
    }

    // 땅에 붙어 있는지 판단
    private static bool IsGroundNormal(Vector3 normal, float threshold)
    {
        return normal.y >= threshold;
    }


    // 이벤트 처리
    void OnEnable()
    {
        // 이벤트 구독 (+= 연산자 사용)
        playerMeltSystem.OnFreezeStateChanged += ApplyFreezeState;
        gameInputReader.JumpPressed += Jump;
    }

    void OnDisable()
    {
        // 이벤트 구독 해제 (-= 연산자 필수)
        playerMeltSystem.OnFreezeStateChanged -= ApplyFreezeState;
        gameInputReader.JumpPressed -= Jump;
    }


    private void ApplyFreezeState(FreezeState state)
    {
        currentState = state;
        int state_index = (int)currentState;
        
        freezeStateMoveSpeedMultiplier = moveSpeedMultipliers[state_index];
        scaledMoveSpeed = baseMoveSpeed * environmentMoveSpeedMultiplier * freezeStateMoveSpeedMultiplier;
        
        friction = frictionsPerStage[state_index];
        extraGravity = extraGravityMultipliers[state_index];
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