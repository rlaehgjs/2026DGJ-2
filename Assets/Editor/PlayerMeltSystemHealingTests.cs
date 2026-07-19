using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PlayerMeltSystemHealingTests
{
    [Test]
    public void TryHeal_AtFullHp_ReturnsFalse()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerMeltSystem meltSystem = playerObject.AddComponent<PlayerMeltSystem>();

        InvokeStart(meltSystem);

        MethodInfo tryHealMethod = typeof(PlayerMeltSystem).GetMethod("TryHeal");

        Assert.That(tryHealMethod, Is.Not.Null, "PlayerMeltSystem은 회복 성공 여부를 알려주는 TryHeal 함수를 제공해야 합니다.");

        bool healed = (bool)tryHealMethod.Invoke(meltSystem, new object[] { 25f });

        Assert.That(healed, Is.False);

        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void TryHeal_BelowMaxHp_ReturnsTrueAndRestoresHp()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerMeltSystem meltSystem = playerObject.AddComponent<PlayerMeltSystem>();

        InvokeStart(meltSystem);
        meltSystem.Damage(40f);

        float hpBeforeHeal = GetCurrentHp(meltSystem);
        MethodInfo tryHealMethod = typeof(PlayerMeltSystem).GetMethod("TryHeal");

        bool healed = (bool)tryHealMethod.Invoke(meltSystem, new object[] { 25f });

        Assert.That(healed, Is.True);
        Assert.That(GetCurrentHp(meltSystem), Is.EqualTo(hpBeforeHeal + 25f).Within(0.001f));

        Object.DestroyImmediate(playerObject);
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
