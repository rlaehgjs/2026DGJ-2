using UnityEngine;

public class RefrigeratorWallRepairInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData requiredNails;
    [Min(1)][SerializeField] private int requiredNailsAmount = 1;
    [SerializeField] private ItemData requiredHammer;
    [Min(1)][SerializeField] private int requiredHammerAmount = 1;
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
            && gameProgressManager.CurrentState == GameProgressState.RepairRefrigeratorWall
            && inventory != null
            && requiredNails != null
            && requiredHammer != null
            && inventory.HasItem(requiredNails.ItemId, requiredNailsAmount)
            && inventory.HasItem(requiredHammer.ItemId, requiredHammerAmount);
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            return;
        }

        if (inventory.TryConsumeItem(requiredNails.ItemId, requiredNailsAmount)
            && inventory.TryConsumeItem(requiredHammer.ItemId, requiredHammerAmount))
        {
            gameProgressManager.TryRepairRefrigeratorWall();
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
                && gameProgressManager.CurrentState == GameProgressState.RepairRefrigeratorWall;
        }
    }
}
