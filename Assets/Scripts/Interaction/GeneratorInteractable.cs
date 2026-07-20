using UnityEngine;

public class GeneratorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData requiredWire;
    [Min(1)][SerializeField] private int requiredWireAmount = 1;
    [SerializeField] private GameProgressManager gameProgressManager;

    public bool CanInteract(PlayerInventory inventory)
    {
        if (gameProgressManager == null)
        {
            return false;
        }

        if (gameProgressManager.CurrentState == GameProgressState.FindGenerator)
        {
            return true;
        }

        return gameProgressManager.CurrentState == GameProgressState.RepairGenerator
            && inventory != null
            && requiredWire != null
            && inventory.HasItem(requiredWire.ItemId, requiredWireAmount);
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            return;
        }

        if (gameProgressManager.CurrentState == GameProgressState.FindGenerator)
        {
            gameProgressManager.TryCompleteGeneratorInspection();
            return;
        }

        if (inventory.TryConsumeItem(requiredWire.ItemId, requiredWireAmount))
        {
            gameProgressManager.TryRepairGenerator();
        }
    }
}
