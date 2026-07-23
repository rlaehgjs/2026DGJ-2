using UnityEngine;

public class FloorTransitionTeleportTrigger : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;
    [Min(0f)][SerializeField] private float reentryBlockSeconds = 0.25f;

    private static Rigidbody blockedPlayer;
    private static float blockedUntil;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody playerBody = other.attachedRigidbody;

        if (playerBody == null
            || !playerBody.CompareTag("Player")
            || targetPoint == null
            || IsReentryBlocked(playerBody))
        {
            return;
        }

        playerBody.position = targetPoint.position;
        playerBody.linearVelocity = Vector3.zero;
        playerBody.angularVelocity = Vector3.zero;

        blockedPlayer = playerBody;
        blockedUntil = Time.time + reentryBlockSeconds;
    }

    private static bool IsReentryBlocked(Rigidbody playerBody)
    {
        return blockedPlayer == playerBody && Time.time < blockedUntil;
    }
}
