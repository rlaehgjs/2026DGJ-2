using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AntNavMeshAI : MonoBehaviour
{
    public static System.Collections.Generic.List<AntNavMeshAI> AllAnts = new System.Collections.Generic.List<AntNavMeshAI>();

    private Transform playerTarget;
    private NavMeshAgent agent;
    private bool isChasing = false;
    private Vector3 laneOffset;

    void OnEnable() { if (!AllAnts.Contains(this)) AllAnts.Add(this); }
    void OnDisable() { if (AllAnts.Contains(this)) AllAnts.Remove(this); }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // 개미마다 목적지를 0.5m 반경 내에서 살짝 비트게 설정
        laneOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        
        // 우선순위를 랜덤하게 줘서 서로 비키게 만듦
        if (agent != null)
        {
            agent.avoidancePriority = Random.Range(10, 90);
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.radius = 0.6f; // 벽 뚫기 방지용 반경
        }
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;
    }

    // ★ [복구 완료] AntManger에서 호출하는 핵심 기능들
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

        // 플레이어 위치 + 랜덤 오프셋으로 목적지 설정
        agent.SetDestination(playerTarget.position + laneOffset);
    }
}