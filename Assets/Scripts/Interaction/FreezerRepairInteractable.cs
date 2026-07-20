using UnityEngine;

public class FreezerRepairInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData requiredCoolantCapsule;
    [Min(1)][SerializeField] private int requiredCoolantCapsuleAmount = 1;
    [SerializeField] private Collider interactionCollider;
    [SerializeField] private GameProgressManager gameProgressManager;

    private void OnEnable()
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged += HandleProgressChanged;
            gameProgressManager.ProgressRestored += HandleProgressRestored;
        }

        RefreshInteractionAvailability();
    }

    private void OnDisable()
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged -= HandleProgressChanged;
            gameProgressManager.ProgressRestored -= HandleProgressRestored;
        }
    }

    public bool CanInteract(PlayerInventory inventory)
    {
        return gameProgressManager != null
            && gameProgressManager.CurrentState == GameProgressState.RepairFreezer
            && inventory != null
            && requiredCoolantCapsule != null
            && inventory.HasItem(requiredCoolantCapsule.ItemId, requiredCoolantCapsuleAmount);
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            return;
        }

        if (inventory.TryConsumeItem(requiredCoolantCapsule.ItemId, requiredCoolantCapsuleAmount))
        {
            gameProgressManager.TryRepairFreezer();
        }
    }

    private void HandleProgressChanged(GameProgressState _)
    {
        RefreshInteractionAvailability();
    }

    private void HandleProgressRestored(GameProgressState _)
    {
        RefreshInteractionAvailability();
    }

    private void RefreshInteractionAvailability()
    {
        if (interactionCollider != null)
        {
            interactionCollider.enabled = gameProgressManager != null
                && gameProgressManager.CurrentState == GameProgressState.RepairFreezer;
        }
    }
}
