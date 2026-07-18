using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class HealingPickupTriggerTests
{
    private const string TestSaveId = "TestHealingPickup_01";

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
        UnityEngine.Object.DestroyImmediate(playerObject);
        UnityEngine.Object.DestroyImmediate(pickupObject);
        UnityEngine.Object.DestroyImmediate(saveManagerObject);

        PlayerPrefs.DeleteKey(SaveManager.GameSaveKey);
        PlayerPrefs.Save();
    }

    [Test]
    public void TriggerEnter_DamagedPlayer_HealsSavesAndDisablesPickup()
    {
        PlayerMeltSystem meltSystem = CreatePlayer(damage: 40f);
        SaveManager saveManager = CreateSaveManager();
        Component healingPickup = CreateHealingPickup(saveManager, healAmount: 25f);

        InvokeTriggerEnter(healingPickup, playerObject.GetComponent<BoxCollider>());

        Assert.That(GetCurrentHp(meltSystem), Is.EqualTo(85f).Within(0.001f));
        Assert.That(saveManager.IsItemCollected(TestSaveId), Is.True);
        Assert.That(pickupObject.activeSelf, Is.False);
    }

    [Test]
    public void TriggerEnter_FullHpPlayer_DoesNotConsumePickup()
    {
        CreatePlayer(damage: 0f);
        SaveManager saveManager = CreateSaveManager();
        Component healingPickup = CreateHealingPickup(saveManager, healAmount: 25f);

        InvokeTriggerEnter(healingPickup, playerObject.GetComponent<BoxCollider>());

        Assert.That(saveManager.IsItemCollected(TestSaveId), Is.False);
        Assert.That(pickupObject.activeSelf, Is.True);
    }

    private PlayerMeltSystem CreatePlayer(float damage)
    {
        playerObject = new GameObject("Player");
        PlayerMeltSystem meltSystem = playerObject.AddComponent<PlayerMeltSystem>();
        playerObject.AddComponent<BoxCollider>();

        InvokeStart(meltSystem);
        meltSystem.Damage(damage);

        return meltSystem;
    }

    private SaveManager CreateSaveManager()
    {
        saveManagerObject = new GameObject("SaveManager");
        return saveManagerObject.AddComponent<SaveManager>();
    }

    private Component CreateHealingPickup(SaveManager saveManager, float healAmount)
    {
        Type healingPickupType = typeof(PlayerMeltSystem).Assembly.GetType("HealingPickup");
        Assert.That(healingPickupType, Is.Not.Null, "HealingPickup 스크립트가 필요합니다.");

        pickupObject = new GameObject("HealingPickup");
        Component healingPickup = pickupObject.AddComponent(healingPickupType);

        SerializedObject serializedPickup = new SerializedObject(healingPickup);
        serializedPickup.FindProperty("healAmount").floatValue = healAmount;
        serializedPickup.FindProperty("saveId").stringValue = TestSaveId;
        serializedPickup.FindProperty("saveManager").objectReferenceValue = saveManager;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();

        return healingPickup;
    }

    private static void InvokeTriggerEnter(Component healingPickup, Collider playerCollider)
    {
        MethodInfo triggerEnterMethod = healingPickup.GetType().GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(triggerEnterMethod, Is.Not.Null, "HealingPickup은 Trigger 진입을 처리해야 합니다.");
        triggerEnterMethod.Invoke(healingPickup, new object[] { playerCollider });
    }

    private static void InvokeStart(PlayerMeltSystem meltSystem)
    {
        MethodInfo startMethod = typeof(PlayerMeltSystem).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        startMethod.Invoke(meltSystem, null);
    }

    private static float GetCurrentHp(PlayerMeltSystem meltSystem)
    {
        FieldInfo currentHpField = typeof(PlayerMeltSystem).GetField("currentHp", BindingFlags.Instance | BindingFlags.NonPublic);
        return (float)currentHpField.GetValue(meltSystem);
    }
}
