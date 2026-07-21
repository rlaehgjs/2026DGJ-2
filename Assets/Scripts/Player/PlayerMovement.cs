using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private List<float> moveSpeedMultipliers = new List<float>()
    { 0.8f, 1.0f, 1.1f };

    [SerializeField]
    private List<float> extraGravityMultipliers = new List<float>()
    { 0f, 2.0f, 3.0f };

    [SerializeField]
    private List<float> frictionsPerStage = new List<float>()
    { 20.0f, 10.0f, 0f };

    [SerializeField]
    private float baseMoveSpeed = 5f;

    [SerializeField]
    private float baseJumpForce = 10f;

    [SerializeField, Range(0f, 1f)]
    private float groundNormalThreshold = 0.65f;

    [SerializeField, Range(0f, 1f)]
    private float wallNormalMaxY = 0.2f;

    private Rigidbody rigidBody;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool isTouchingWall;
    private Vector3 wallEscapeNormal;
    private bool escapedFromWallInAir;
    private bool wallEscapeInputPendingRelease;
    private bool wasGroundedLastPhysicsStep;

    private float freezeStateMoveSpeedMultiplier = 1.0f;
    private float environmentMoveSpeedMultiplier = 1.0f;
    private float scaledMoveSpeed;
    private float scaledJumpForce;
    private float friction;
    private float extraGravity;
    private FreezeState currentState = FreezeState.Frozen;

    private PlayerMeltSystem playerMeltSystem;
    private GameInputReader gameInputReader;

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
        wasGroundedLastPhysicsStep = isGrounded;
        isGrounded = false;
        isTouchingWall = false;
        wallEscapeNormal = Vector3.zero;
    }

    void Update()
    {
        Vector2 moveInput = gameInputReader.MoveInput;

        bool hasMoveInput = moveInput.sqrMagnitude > 0f;
        if (ShouldBlockWallEscapeInput(wallEscapeInputPendingRelease, hasMoveInput))
        {
            moveDirection = Vector3.zero;
        }
        else
        {
            wallEscapeInputPendingRelease = false;
            SetMoveDirection(new Vector3(
                moveInput.x,
                0f,
                moveInput.y));
        }

        // 벽에 닿아 입력이 없어도 기존 수평 속도를 제거해야 한다.
        if (moveDirection != Vector3.zero || IsAirborneAgainstWall())
        {
            Move();
        }

        AdjustJump();
        AdjustMove();
    }

    public void Move()
    {
        Vector3 originalVelocity = rigidBody.linearVelocity;
        Vector3 allowedMoveDirection = moveDirection;

        if (IsAirborneAgainstWall())
        {
            allowedMoveDirection = GetWallEscapeDirection(moveDirection, wallEscapeNormal);
            if (allowedMoveDirection.sqrMagnitude > 0f)
            {
                escapedFromWallInAir = true;
            }
        }

        Vector3 newVelocity = scaledMoveSpeed * allowedMoveDirection;
        rigidBody.linearVelocity = new Vector3(newVelocity.x, originalVelocity.y, newVelocity.z);
        Debug.Log("현재 속도: " + rigidBody.linearVelocity);
    }

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
        Vector3 currentPlanarVelocity = new Vector3(
            rigidBody.linearVelocity.x,
            0f,
            rigidBody.linearVelocity.z);
        Vector3 smoothedVelocity = Vector3.Lerp(currentPlanarVelocity, Vector3.zero, value);

        rigidBody.linearVelocity = new Vector3(
            smoothedVelocity.x,
            rigidBody.linearVelocity.y,
            smoothedVelocity.z);
    }

    private void SetMoveDirection(Vector3 direction)
    {
        ApplyLookDirection(direction);
    }

    private void ApplyLookDirection(Vector3 direction)
    {
        Vector3 lookDirection = transform.forward;
        lookDirection.y = 0f;
        lookDirection.Normalize();

        Quaternion horizontalRotation = Quaternion.LookRotation(lookDirection);
        moveDirection = horizontalRotation * direction.normalized;
    }

    private void Jump()
    {
        if (isGrounded)
        {
            Debug.Log("점프 성공!");
            isGrounded = false;
            rigidBody.AddForce(Vector3.up * scaledJumpForce, ForceMode.Impulse);
        }
    }

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
                if (ShouldStartWallEscapeInputLock(escapedFromWallInAir, !wasGroundedLastPhysicsStep))
                {
                    escapedFromWallInAir = false;
                    wallEscapeInputPendingRelease = true;
                    rigidBody.linearVelocity = StopPlanarVelocity(rigidBody.linearVelocity);
                }
                continue;
            }

            if (!IsWallNormal(contactNormal, wallNormalMaxY))
            {
                continue;
            }

            Vector3 horizontalNormal = Vector3.ProjectOnPlane(contactNormal, Vector3.up);
            if (horizontalNormal.sqrMagnitude <= 0f)
            {
                continue;
            }

            isTouchingWall = true;
            wallEscapeNormal += horizontalNormal.normalized;
        }
    }

    private bool IsAirborneAgainstWall()
    {
        return !isGrounded && isTouchingWall;
    }

    private static bool IsGroundNormal(Vector3 normal, float threshold)
    {
        return normal.y >= threshold;
    }

    private static bool IsWallNormal(Vector3 normal, float maxNormalY)
    {
        return Mathf.Abs(normal.y) <= maxNormalY;
    }

    private static Vector3 GetWallEscapeDirection(Vector3 inputDirection, Vector3 escapeNormal)
    {
        if (inputDirection.sqrMagnitude <= 0f || escapeNormal.sqrMagnitude <= 0f)
        {
            return Vector3.zero;
        }

        Vector3 normalizedEscapeNormal = escapeNormal.normalized;
        float escapeAmount = Vector3.Dot(inputDirection, normalizedEscapeNormal);

        return escapeAmount > 0f
            ? normalizedEscapeNormal * escapeAmount
            : Vector3.zero;
    }

    private static bool ShouldStartWallEscapeInputLock(bool escapedFromWallInAir, bool justLanded)
    {
        return escapedFromWallInAir && justLanded;
    }

    private static bool ShouldBlockWallEscapeInput(bool inputPendingRelease, bool hasMoveInput)
    {
        return inputPendingRelease && hasMoveInput;
    }

    private static Vector3 StopPlanarVelocity(Vector3 velocity)
    {
        return new Vector3(0f, velocity.y, 0f);
    }

    void OnEnable()
    {
        playerMeltSystem.OnFreezeStateChanged += ApplyFreezeState;
        gameInputReader.JumpPressed += Jump;
    }

    void OnDisable()
    {
        playerMeltSystem.OnFreezeStateChanged -= ApplyFreezeState;
        gameInputReader.JumpPressed -= Jump;
    }

    private void ApplyFreezeState(FreezeState state)
    {
        currentState = state;
        int stateIndex = (int)currentState;
        freezeStateMoveSpeedMultiplier = moveSpeedMultipliers[stateIndex];
        scaledMoveSpeed = baseMoveSpeed * environmentMoveSpeedMultiplier * freezeStateMoveSpeedMultiplier;
        friction = frictionsPerStage[stateIndex];
        extraGravity = extraGravityMultipliers[stateIndex];
    }

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
