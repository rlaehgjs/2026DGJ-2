using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Rigidbody rigid_body;
    private Vector3 move_direction;
    private float move_speed = 5f;
    private float jump_force = 10f;
    private bool is_grounded = false;

    void Start()
    {
        rigid_body = GetComponent<Rigidbody>();
    }


    // 수평 이동 처리
    void Update()
    {
        if (!move_direction.Equals(Vector3.zero)) // 움직일 방향이 설정됐다면
        {
            Debug.Log("움직임 시도! " + move_direction);
            Move();
        }
    }

    public void Move()
    {
        Vector3 original_velocity = rigid_body.linearVelocity;
        Vector3 new_velocity = move_speed * move_direction;
        rigid_body.linearVelocity = new Vector3(new_velocity.x, original_velocity.y, new_velocity.z);
    }


    // 녹은 정도 받아와서 마찰력 변경하는 코드 적을 곳


    // 점프 처리
    public void Jump()
    {
        if (is_grounded)
        {
            Debug.Log("점프 성공!");
            is_grounded = false;
            rigid_body.AddForce(Vector3.up * jump_force, ForceMode.Impulse);
            // 녹은 정도에 따라
            // rb.gravityScale = 2f; // 녹은 정도에 따라 중력 크기 조절
        }
    }

    // 착지 처리
    private void OnCollisionEnter(Collision collision)
    {
        // 땅(Ground) 태그를 가진 오브젝트와 부딪혔을 때
        if (collision.gameObject.CompareTag("Ground"))
        {
            is_grounded = true;
            Debug.Log("착지 완료!");
        }
    }


    // 수평 이동 방향 설정
    public void SetMoveDirection(Vector3 direction)
    {
        // 보는 방향을 기준으로 이동
        ApplyLookDirection(direction);
    }

    private void ApplyLookDirection(Vector3 direction)
    {
        // 위아래로 보고 있을 때 이동 방향이 잘못되지 않도록 방지
        Vector3 look_direction = transform.forward;
        look_direction.y = 0;
        look_direction.Normalize(); // 크기를 다시 1로 맞춤
        // 수평 회전량만 남김
        Quaternion horizontalRotation = Quaternion.LookRotation(look_direction);
        move_direction = horizontalRotation * direction.normalized;
    }
}
