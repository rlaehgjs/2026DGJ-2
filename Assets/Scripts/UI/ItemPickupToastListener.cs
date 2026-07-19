using UnityEngine;

public class ItemPickupToastListener : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemPickupToastUI toastUI;

    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.ItemAcquired += ShowPickupToast;
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.ItemAcquired -= ShowPickupToast;
    }

    private void ShowPickupToast(ItemData itemData, int amount)
    {
        if (toastUI == null || itemData == null)
            return;

        string itemName = itemData.DisplayNameKey;
        if (LocalizationManager.Instance != null && !string.IsNullOrWhiteSpace(itemName))
            itemName = LocalizationManager.Instance.Get(itemName);

        if (string.IsNullOrWhiteSpace(itemName))
            itemName = itemData.ItemId;

        string message = LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == 1
            ? $"{itemName} x{amount} acquired"
            : $"{itemName} x{amount} 획득";

        toastUI.Show(message);
    }
}
