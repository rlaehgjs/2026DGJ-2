using UnityEngine;

public class PickupInteractable : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [Min(1)][SerializeField] private int amount = 1;
    [SerializeField] private string saveId;
    [SerializeField] private SaveManager saveManager;

    private bool isCollected;

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

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        TryCollect(inventory);
    }

    private void TryCollect(PlayerInventory inventory)
    {
        if (isCollected
            || inventory == null
            || itemData == null
            || amount <= 0
            || string.IsNullOrWhiteSpace(saveId)
            || saveManager == null)
        {
            return;
        }

        if (inventory.TryAddItem(itemData, amount))
        {
            isCollected = true;
            saveManager.RegisterCollectedItem(saveId);
            gameObject.SetActive(false);
        }
    }
}
