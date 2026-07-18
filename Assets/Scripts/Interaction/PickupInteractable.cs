using UnityEngine;

public class PickupInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    [Min(1)][SerializeField] private int amount = 1;
    [SerializeField] private string saveId;
    [SerializeField] private SaveManager saveManager;

    public string SaveId => saveId;

    private void Start()
    {
        if (saveManager != null
            && !string.IsNullOrWhiteSpace(saveId)
            && saveManager.IsItemCollected(saveId))
        {
            gameObject.SetActive(false);
        }
    }

    public bool CanInteract(PlayerInventory inventory)
    {
        return inventory != null
            && itemData != null
            && amount > 0
            && !string.IsNullOrWhiteSpace(saveId)
            && saveManager != null;
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            return;
        }

        if (inventory.TryAddItem(itemData, amount))
        {
            saveManager.RegisterCollectedItem(saveId);
            gameObject.SetActive(false);
        }
    }
}
