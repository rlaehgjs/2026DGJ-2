using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private List<Image> slotIcons;
    [SerializeField] private List<TMP_Text> amountTexts;

    public void Configure(PlayerInventory inventory)
    {
        playerInventory = inventory;
    }

    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.InventoryChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.InventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        for (int i = 0; i < slotIcons.Count; i++)
        {
            bool hasItem = playerInventory != null
                && i < playerInventory.Items.Count
                && playerInventory.Items[i].ItemData != null;

            slotIcons[i].gameObject.SetActive(hasItem);

            if (hasItem)
            {
                InventoryItem item = playerInventory.Items[i];
                slotIcons[i].sprite = item.ItemData.Icon;
                amountTexts[i].text = $"x{item.Amount}";
            }
            else
            {
                amountTexts[i].text = "";
            }
        }
    }
}
