using System.Collections.Generic;
using UnityEngine;

public class FreezerEntranceTrigger : MonoBehaviour
{
    [SerializeField] private Collider triggerCollider;
    [Min(0.1f)][SerializeField] private float requiredStaySeconds = 2f;
    [SerializeField] private GameProgressManager gameProgressManager;

    private readonly HashSet<Collider> playerCollidersInside = new HashSet<Collider>();
    private float currentStaySeconds;

    private void OnEnable()
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged += HandleProgressChanged;
            gameProgressManager.ProgressRestored += HandleProgressRestored;
        }

        RefreshTriggerAvailability();
    }

    private void OnDisable()
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged -= HandleProgressChanged;
            gameProgressManager.ProgressRestored -= HandleProgressRestored;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TrackPlayerCollider(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TrackPlayerCollider(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerInventory>() == null)
        {
            return;
        }

        playerCollidersInside.Remove(other);

        if (playerCollidersInside.Count == 0)
        {
            currentStaySeconds = 0f;
        }
    }

    private void Update()
    {
        AdvanceStayTimer(Time.deltaTime);
    }

    private void TrackPlayerCollider(Collider other)
    {
        if (gameProgressManager == null
            || gameProgressManager.CurrentState != GameProgressState.EnterFreezer)
        {
            return;
        }

        if (other.GetComponentInParent<PlayerInventory>() == null)
        {
            return;
        }

        playerCollidersInside.Add(other);
    }

    private void AdvanceStayTimer(float deltaTime)
    {
        if (gameProgressManager == null
            || gameProgressManager.CurrentState != GameProgressState.EnterFreezer
            || playerCollidersInside.Count == 0)
        {
            ResetStayTimer();
            return;
        }

        currentStaySeconds += deltaTime;

        if (currentStaySeconds >= requiredStaySeconds)
        {
            gameProgressManager.TryEnterFreezer();
        }
    }

    private void HandleProgressChanged(GameProgressState _)
    {
        RefreshTriggerAvailability();
    }

    private void HandleProgressRestored(GameProgressState _)
    {
        RefreshTriggerAvailability();
    }

    private void RefreshTriggerAvailability()
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = gameProgressManager != null
                && gameProgressManager.CurrentState == GameProgressState.EnterFreezer;
        }

        if (gameProgressManager == null
            || gameProgressManager.CurrentState != GameProgressState.EnterFreezer)
        {
            ResetStayTimer();
        }
    }

    private void ResetStayTimer()
    {
        playerCollidersInside.Clear();
        currentStaySeconds = 0f;
    }
}
