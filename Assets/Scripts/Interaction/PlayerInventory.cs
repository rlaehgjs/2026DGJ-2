using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<ItemData> itemCatalog = new List<ItemData>();
    [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();

    public IReadOnlyList<InventoryItem> Items => items;

    public event Action InventoryChanged;
    public event Action<ItemData, int> ItemAcquired;

    public bool TryAddItem(ItemData itemData, int amount)
    {
        if (itemData == null || string.IsNullOrWhiteSpace(itemData.ItemId) || amount <= 0 || itemData.MaxStack < 1)
        {
            return false;
        }

        InventoryItem inventoryItem = FindItem(itemData.ItemId);

        if (inventoryItem == null)
        {
            if (amount > itemData.MaxStack)
            {
                return false;
            }

            items.Add(new InventoryItem(itemData, amount));
            ItemAcquired?.Invoke(itemData, amount);
            InventoryChanged?.Invoke();
            return true;
        }

        if (inventoryItem.Amount + amount > inventoryItem.ItemData.MaxStack)
        {
            return false;
        }

        inventoryItem.AddAmount(amount);
        ItemAcquired?.Invoke(itemData, amount);
        InventoryChanged?.Invoke();
        return true;
    }

    public int GetItemAmount(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return 0;
        }

        InventoryItem inventoryItem = FindItem(itemId);
        return inventoryItem == null ? 0 : inventoryItem.Amount;
    }

    public bool HasItem(string itemId, int amount)
    {
        return !string.IsNullOrWhiteSpace(itemId) && amount > 0 && GetItemAmount(itemId) >= amount;
    }

    public bool TryConsumeItem(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return false;
        }

        InventoryItem inventoryItem = FindItem(itemId);

        if (inventoryItem == null || inventoryItem.Amount < amount)
        {
            return false;
        }

        inventoryItem.RemoveAmount(amount);

        if (inventoryItem.Amount == 0)
        {
            items.Remove(inventoryItem);
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public List<ItemSaveEntry> CreateSaveEntries()
    {
        List<ItemSaveEntry> saveEntries = new List<ItemSaveEntry>();

        foreach (InventoryItem item in items)
        {
            if (item.ItemData == null || string.IsNullOrWhiteSpace(item.ItemData.ItemId) || item.Amount <= 0)
            {
                continue;
            }

            saveEntries.Add(new ItemSaveEntry
            {
                ItemId = item.ItemData.ItemId,
                Amount = item.Amount
            });
        }

        return saveEntries;
    }

    public void RestoreFromSaveEntries(IReadOnlyList<ItemSaveEntry> saveEntries)
    {
        items.Clear();

        if (saveEntries != null)
        {
            foreach (ItemSaveEntry saveEntry in saveEntries)
            {
                if (saveEntry == null || string.IsNullOrWhiteSpace(saveEntry.ItemId) || saveEntry.Amount <= 0)
                {
                    continue;
                }

                ItemData itemData = FindCatalogItem(saveEntry.ItemId);

                if (itemData == null)
                {
                    continue;
                }

                int restoredAmount = Mathf.Min(saveEntry.Amount, itemData.MaxStack);
                InventoryItem inventoryItem = FindItem(saveEntry.ItemId);

                if (inventoryItem == null)
                {
                    items.Add(new InventoryItem(itemData, restoredAmount));
                }
                else
                {
                    int availableAmount = itemData.MaxStack - inventoryItem.Amount;
                    inventoryItem.AddAmount(Mathf.Min(restoredAmount, availableAmount));
                }
            }
        }

        InventoryChanged?.Invoke();
    }

    private InventoryItem FindItem(string itemId)
    {
        foreach (InventoryItem item in items)
        {
            if (item.ItemData != null && item.ItemData.ItemId == itemId)
            {
                return item;
            }
        }

        return null;
    }

    private ItemData FindCatalogItem(string itemId)
    {
        foreach (ItemData itemData in itemCatalog)
        {
            if (itemData != null && itemData.ItemId == itemId)
            {
                return itemData;
            }
        }

        return null;
    }
}
