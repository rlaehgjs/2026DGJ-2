using UnityEngine;

public class RefrigeratorWallRepairInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData requiredNails;
    [Min(1)][SerializeField] private int requiredNailsAmount = 1;
    [SerializeField] private ItemData requiredHammer;
    [Min(1)][SerializeField] private int requiredHammerAmount = 1;
    [SerializeField] private Collider interactionCollider;
    [SerializeField] private GameProgressManager gameProgressManager;
    [SerializeField] private Transform repairedDoor;
    [SerializeField] private Vector3 repairedLocalPosition = new Vector3(-0.59819299f, 1.71812499f, 0.365508914f);

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
            RestoreDoorPosition();
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

    private void RestoreDoorPosition()
    {
        if (repairedDoor != null)
        {
            repairedDoor.localPosition = repairedLocalPosition;
        }
    }
}
