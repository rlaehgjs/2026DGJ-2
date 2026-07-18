using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PickupInteractableTriggerTests
{
    private const string TestSaveId = "TestPickup_01";

    private GameObject playerObject;
    private GameObject pickupObject;
    private GameObject saveManagerObject;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(SaveManager.GameSaveKey);
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        if (playerObject != null)
        {
            Object.DestroyImmediate(playerObject);
        }

        if (pickupObject != null)
        {
            Object.DestroyImmediate(pickupObject);
        }

        if (saveManagerObject != null)
        {
            Object.DestroyImmediate(saveManagerObject);
        }

        PlayerPrefs.DeleteKey(SaveManager.GameSaveKey);
        PlayerPrefs.Save();
    }

    [Test]
    public void TriggerEnter_AddsItem_SavesId_AndDisablesPickup()
    {
        ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
        SetItemDataId(itemData, "test_item");

        playerObject = new GameObject("Player");
        PlayerInventory inventory = playerObject.AddComponent<PlayerInventory>();
        BoxCollider playerCollider = playerObject.AddComponent<BoxCollider>();

        saveManagerObject = new GameObject("SaveManager");
        SaveManager saveManager = saveManagerObject.AddComponent<SaveManager>();

        pickupObject = new GameObject("Pickup");
        PickupInteractable pickup = pickupObject.AddComponent<PickupInteractable>();
        SetPickupData(pickup, itemData, TestSaveId, saveManager);

        MethodInfo triggerEnterMethod = typeof(PickupInteractable).GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        triggerEnterMethod.Invoke(pickup, new object[] { playerCollider });

        Assert.That(inventory.GetItemAmount(itemData.ItemId), Is.EqualTo(1));
        Assert.That(saveManager.IsItemCollected(TestSaveId), Is.True);
        Assert.That(pickupObject.activeSelf, Is.False);

        Object.DestroyImmediate(itemData);
    }

    private static void SetItemDataId(ItemData itemData, string itemId)
    {
        SerializedObject serializedItemData = new SerializedObject(itemData);
        serializedItemData.FindProperty("itemId").stringValue = itemId;
        serializedItemData.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetPickupData(PickupInteractable pickup, ItemData itemData, string saveId, SaveManager saveManager)
    {
        SerializedObject serializedPickup = new SerializedObject(pickup);
        serializedPickup.FindProperty("itemData").objectReferenceValue = itemData;
        serializedPickup.FindProperty("amount").intValue = 1;
        serializedPickup.FindProperty("saveId").stringValue = saveId;
        serializedPickup.FindProperty("saveManager").objectReferenceValue = saveManager;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();
    }
}
