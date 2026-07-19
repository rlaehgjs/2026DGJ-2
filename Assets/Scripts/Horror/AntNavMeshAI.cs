using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; // ★ 리스트 사용을 위해 필수 추가

public class AntNavMeshAI : MonoBehaviour
{
    // ★ [핵심 1] 유니티 물리 엔진을 믿지 않고, 개미들을 수학적으로 감시할 마스터 리스트
    public static List<AntNavMeshAI> AllAnts = new List<AntNavMeshAI>();

    private Transform playerTarget;
    private NavMeshAgent agent;
    private bool isChasing = false;

    private Vector3 personalOffset;
    private float targetUpdateTimer = 0f;
    private float targetUpdateInterval = 0.2f;

    void OnEnable()
    {
        // 게임 창에 개미가 켜지면 마스터 리스트에 자동 등록
        if (!AllAnts.Contains(this)) AllAnts.Add(this);
    }

    void OnDisable()
    {
        // 개미가 사라지면 리스트에서 안전하게 제거
        if (AllAnts.Contains(this)) AllAnts.Remove(this);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;

        // 개미마다 플레이어 주변에 가질 고유 목표 지점 분산
        float angle = Random.Range(0f, 360f);
        float radius = Random.Range(0.4f, 0.8f);
        personalOffset = Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;

        if (agent != null)
        {
            // ★ 유니티 순정 시스템 원복 (벽을 절대로 뚫지 못하게 안전장치 작동)
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.radius = 0.2f; 
        }
    }

    public void StartChase()
    {
        isChasing = true;
        if (agent != null) agent.isStopped = false;
    }

    public void StopChase()
    {
        isChasing = false;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    void Update()
    {
        if (!isChasing || playerTarget == null || agent == null || !agent.isOnNavMesh) return;

        // 1. 0.2초마다 목적지 부드럽게 새로고침
        targetUpdateTimer += Time.deltaTime;
        if (targetUpdateTimer >= targetUpdateInterval)
        {
            targetUpdateTimer = 0f;
            agent.SetDestination(playerTarget.position + personalOffset);
        }

        // 2. ★ [핵심 2] 물리 엔진 버그 없는 100% 정확한 개미 간 거리 연산
        Vector3 separationForce = Vector3.zero;
        int overlapCount = 0;

        for (int i = 0; i < AllAnts.Count; i++)
        {
            AntNavMeshAI otherAnt = AllAnts[i];
            
            // 나 자신이거나 이미 사라진 개미는 계산에서 제외
            if (otherAnt == this || otherAnt == null) continue;

            // 상대방 개미와의 순수 수학적 거리 계산
            Vector3 diff = transform.position - otherAnt.transform.position;
            diff.y = 0;
            float dist = diff.magnitude;

            // 45cm 안으로 들어와 겹치려고 하면 즉시 레이더 가동
            if (dist < 0.45f)
            {
                // 🔥 [유령 합체 완전 저격] 좌표가 완벽히 일치해서 거리가 0이 된 최악의 경우
                if (dist < 0.02f)
                {
                    // 개미들의 고유 고정 ID 번호를 대조해서 서로 정반대 방향으로 찢어버립니다.
                    float sign = (GetInstanceID() > otherAnt.GetInstanceID()) ? 1f : -1f;
                    diff = transform.right * sign * 0.45f;
                    dist = 0.45f;
                }

                // 가까울수록 서로를 밀어내는 힘을 기하급수적으로 증가시킵니다.
                separationForce += diff.normalized * (0.45f - dist);
                overlapCount++;
            }
        }

        // 3. ★ 계산된 힘을 유니티 순정 내비메쉬 벽 뚫기 방지 시스템(agent.Move)에 주입
        if (overlapCount > 0)
        {
            separationForce /= overlapCount;
            
            // 밀어내는 힘을 강력하게 주입합니다. 
            // 튕겨 나가더라도 updatePosition이 켜져 있어 벽 밖으로는 절대 안 나가고 복도 옆면을 타고 미끄러집니다.
            agent.Move(separationForce * Time.deltaTime * 6.5f);
        }
    }
}