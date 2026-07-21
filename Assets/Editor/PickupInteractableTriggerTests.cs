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
    private GameObject progressManagerObject;

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

        if (progressManagerObject != null)
        {
            Object.DestroyImmediate(progressManagerObject);
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
        ItemData collectedItem = null;
        int collectedAmount = 0;
        pickup.ItemCollected += (item, amount) =>
        {
            collectedItem = item;
            collectedAmount = amount;
        };

        MethodInfo triggerEnterMethod = typeof(PickupInteractable).GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        triggerEnterMethod.Invoke(pickup, new object[] { playerCollider });

        Assert.That(inventory.GetItemAmount(itemData.ItemId), Is.EqualTo(1));
        Assert.That(saveManager.IsItemCollected(TestSaveId), Is.True);
        Assert.That(collectedItem, Is.EqualTo(itemData));
        Assert.That(collectedAmount, Is.EqualTo(1));
        Assert.That(pickupObject.activeSelf, Is.False);

        Object.DestroyImmediate(itemData);
    }

    [Test]
    public void Awake_HidesSavedPickupBeforeItCanBeEnabledForCollection()
    {
        ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
        SetItemDataId(itemData, "test_item");

        saveManagerObject = new GameObject("SaveManager");
        SaveManager saveManager = saveManagerObject.AddComponent<SaveManager>();
        Assert.That(saveManager.RegisterCollectedItem(TestSaveId), Is.True);

        pickupObject = new GameObject("Pickup");
        PickupInteractable pickup = pickupObject.AddComponent<PickupInteractable>();
        SetPickupData(pickup, itemData, TestSaveId, saveManager);

        InvokeAwake(pickup);

        Assert.That(pickupObject.activeSelf, Is.False);

        Object.DestroyImmediate(itemData);
    }

    [Test]
    public void TriggerEnter_WhenSaveManagerIsInactive_AddsItemWithoutSaving()
    {
        ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
        SetItemDataId(itemData, "test_item");

        playerObject = new GameObject("Player");
        PlayerInventory inventory = playerObject.AddComponent<PlayerInventory>();
        BoxCollider playerCollider = playerObject.AddComponent<BoxCollider>();

        saveManagerObject = new GameObject("SaveManager");
        SaveManager saveManager = saveManagerObject.AddComponent<SaveManager>();
        saveManagerObject.SetActive(false);

        pickupObject = new GameObject("Pickup");
        PickupInteractable pickup = pickupObject.AddComponent<PickupInteractable>();
        SetPickupData(pickup, itemData, TestSaveId, saveManager);

        MethodInfo triggerEnterMethod = typeof(PickupInteractable).GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        triggerEnterMethod.Invoke(pickup, new object[] { playerCollider });

        Assert.That(inventory.GetItemAmount(itemData.ItemId), Is.EqualTo(1));
        Assert.That(saveManager.HasGameSave(), Is.False);
        Assert.That(pickupObject.activeSelf, Is.False);

        Object.DestroyImmediate(itemData);
    }

    [Test]
    public void TriggerEnter_WhenRequiredProgressDoesNotMatch_DoesNotCollectUntilObjectiveMatches()
    {
        ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
        SetItemDataId(itemData, "test_item");

        playerObject = new GameObject("Player");
        PlayerInventory inventory = playerObject.AddComponent<PlayerInventory>();
        BoxCollider playerCollider = playerObject.AddComponent<BoxCollider>();

        saveManagerObject = new GameObject("SaveManager");
        SaveManager saveManager = saveManagerObject.AddComponent<SaveManager>();

        progressManagerObject = new GameObject("GameProgressManager");
        GameProgressManager progressManager = progressManagerObject.AddComponent<GameProgressManager>();

        pickupObject = new GameObject("Pickup");
        PickupInteractable pickup = pickupObject.AddComponent<PickupInteractable>();
        SetPickupData(pickup, itemData, TestSaveId, saveManager);
        SetProgressRequirement(pickup, progressManager, GameProgressState.FindNails);

        MethodInfo triggerEnterMethod = typeof(PickupInteractable).GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(pickup.IsCollectionAvailable, Is.False);
        triggerEnterMethod.Invoke(pickup, new object[] { playerCollider });

        Assert.That(inventory.GetItemAmount(itemData.ItemId), Is.EqualTo(0));
        Assert.That(saveManager.IsItemCollected(TestSaveId), Is.False);
        Assert.That(pickupObject.activeSelf, Is.True);

        progressManager.RestoreState(GameProgressState.FindNails);

        Assert.That(pickup.IsCollectionAvailable, Is.True);
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

    private static void SetProgressRequirement(
        PickupInteractable pickup,
        GameProgressManager progressManager,
        GameProgressState requiredState)
    {
        SerializedObject serializedPickup = new SerializedObject(pickup);
        serializedPickup.FindProperty("requiresProgressState").boolValue = true;
        serializedPickup.FindProperty("requiredProgressState").enumValueIndex = (int)requiredState;
        serializedPickup.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void InvokeAwake(PickupInteractable pickup)
    {
        MethodInfo awakeMethod = typeof(PickupInteractable).GetMethod(
            "Awake",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(awakeMethod, Is.Not.Null);
        awakeMethod.Invoke(pickup, null);
    }
}
