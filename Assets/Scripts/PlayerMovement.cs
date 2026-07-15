using UnityEngine;

public class PlayerMovement : MonoBehaviour
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
        if (move_direction != Vector3.zero) // 움직일 방향이 설정됐다면
        {
            Move();
        }
    }

    public void Move()
    {
        rigid_body.linearVelocity = move_speed * move_direction.normalized;
    }

    public void Stop()
    {
        move_direction = Vector3.zero;
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
    public void SetForward()
    {
        // 방향 벡터 바라보는 기준으로 바꿔야 됨
        move_direction.z = 1;
    }

    public void SetBackward()
    {
        move_direction.z = -1;
    }

    public void SetLeft()
    {
        move_direction.x = -1;
    }

    public void SetRight()
    {
        move_direction.x = 1;
    }
}
