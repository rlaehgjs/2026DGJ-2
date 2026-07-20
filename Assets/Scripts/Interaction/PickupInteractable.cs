using System;
using UnityEngine;

public class PickupInteractable : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [Min(1)][SerializeField] private int amount = 1;
    [SerializeField] private string saveId;
    [SerializeField] private SaveManager saveManager;

    private bool isCollected; //획독흔 아이템인가?

    public string SaveId => saveId;
    public event Action<ItemData, int> ItemCollected; //획든한 아이템들을 확인

    private void Awake()
    {
        if (saveManager != null
            && saveManager.isActiveAndEnabled
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
            || string.IsNullOrWhiteSpace(saveId))
        {
            return;
        }

        if (inventory.TryAddItem(itemData, amount))
        {
            isCollected = true;

            if (saveManager != null && saveManager.isActiveAndEnabled)
            {
                saveManager.RegisterCollectedItem(saveId);
            }

            ItemCollected?.Invoke(itemData, amount);
            gameObject.SetActive(false);
        }
    }
}
