using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AntNavMeshAI : MonoBehaviour
{
    public static System.Collections.Generic.List<AntNavMeshAI> AllAnts = new System.Collections.Generic.List<AntNavMeshAI>();

    private Transform playerTarget;
    private NavMeshAgent agent;
    private bool isChasing = false;

    // 1. 차선 유지 변수 (기차놀이 방지)
    private float myLaneOffset; 
    
    // 2. 개미 간 겹침 방지 거리 (다리 겹침 방지)
    private float separationRadius = 0.6f; 

    void OnEnable() { if (!AllAnts.Contains(this)) AllAnts.Add(this); }
    void OnDisable() { if (AllAnts.Contains(this)) AllAnts.Remove(this); }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;

        // 개미마다 고유한 차선을 배정해서 단일 대열을 깨뜨립니다.
        myLaneOffset = Random.Range(-0.5f, 0.5f);

        if (agent != null)
        {
            agent.updatePosition = true;
            agent.updateRotation = true;
            // 내비메쉬 반경은 작게 해서 벽에 붙는 유연함을 확보합니다.
            agent.radius = 0.3f; 
        }
    }

    public void StartChase() { isChasing = true; if (agent != null) agent.isStopped = false; }
    public void StopChase() { isChasing = false; if (agent != null && agent.isOnNavMesh) { agent.isStopped = true; agent.ResetPath(); } }

    void Update()
    {
        if (!isChasing || playerTarget == null || agent == null || !agent.isOnNavMesh) return;

        // 목적지 설정 (내비메쉬가 벽을 피해 알아서 찾아가게 둡니다.)
        agent.SetDestination(playerTarget.position);

        // 개미끼리 서로 밀어내는 힘 (이건 벽과 상관없으므로 슬라이딩을 안 일으킵니다.)
        Vector3 separationForce = Vector3.zero;
        int count = 0;

        foreach (var other in AllAnts)
        {
            if (other == this || other == null) continue;

            Vector3 diff = transform.position - other.transform.position;
            diff.y = 0;
            float dist = diff.magnitude;

            if (dist < separationRadius)
            {
                separationForce += diff.normalized * (separationRadius - dist);
                count++;
            }
        }

        // 최종 속도 결정
        // 1. 순정 내비메쉬의 이동 방향
        Vector3 moveDir = agent.desiredVelocity.normalized;
        
        // 2. 차선 유지 (진행 방향에 직각으로 밀기)
        Vector3 lanePush = Vector3.Cross(moveDir, Vector3.up) * myLaneOffset;

        // 3. 개미끼리 밀어내기 힘 반영
        if (count > 0)
        {
            agent.velocity = moveDir * agent.speed + (separationForce * 2.0f) + lanePush;
        }
    }
}