using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    [SerializeField] private ItemData itemData;
    [Min(0)][SerializeField] private int amount;

    public ItemData ItemData => itemData;
    public int Amount => amount;

    public InventoryItem(ItemData itemData, int amount)
    {
        this.itemData = itemData;
        this.amount = amount;
    }

    public void AddAmount(int value)
    {
        amount += value;
    }

    public void RemoveAmount(int value)
    {
        amount -= value;
    }
}
