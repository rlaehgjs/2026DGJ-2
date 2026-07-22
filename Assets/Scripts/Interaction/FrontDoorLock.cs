using UnityEngine;

public class FrontDoorLock : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData requiredKey;
    [Min(1)][SerializeField] private int requiredAmount = 1;
    [SerializeField] private GameProgressManager gameProgressManager;

    private void Awake()
    {
        if (gameProgressManager == null)
        {
            gameProgressManager = FindFirstObjectByType<GameProgressManager>();
        }
    }

    public bool IsUnlocked => gameProgressManager != null
        && gameProgressManager.CurrentState >= GameProgressState.FindGenerator;

    public bool IsLocked => !IsUnlocked;

    public bool CanInteract(PlayerInventory inventory)
    {
        return IsLocked
            && inventory != null
            && requiredKey != null
            && gameProgressManager != null
            && gameProgressManager.CurrentState == GameProgressState.FindFrontDoorKey
            && inventory.HasItem(requiredKey.ItemId, requiredAmount);
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            return;
        }

        gameProgressManager.TryCompleteFrontDoorKeyCollection();
    }
}
