using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PlayerMovementTests
{
    [Test]
    public void IsGroundNormal_UpwardSurface_IsGrounded()
    {
        bool isGrounded = InvokeStaticBool("IsGroundNormal", Vector3.up, 0.65f);

        Assert.That(isGrounded, Is.True);
    }

    [Test]
    public void IsGroundNormal_VerticalSurface_IsNotGrounded()
    {
        bool isGrounded = InvokeStaticBool("IsGroundNormal", Vector3.right, 0.65f);

        Assert.That(isGrounded, Is.False);
    }

    [Test]
    public void IsWallNormal_VerticalSurface_IsWall()
    {
        bool isWall = InvokeStaticBool("IsWallNormal", Vector3.forward, 0.2f);

        Assert.That(isWall, Is.True);
    }

    [Test]
    public void GetWallEscapeDirection_OnlyKeepsInputAwayFromWall()
    {
        Vector3 escapeNormal = Vector3.left;

        Vector3 towardWall = InvokeWallEscapeDirection(Vector3.right, escapeNormal);
        Vector3 alongWall = InvokeWallEscapeDirection(Vector3.forward, escapeNormal);
        Vector3 awayFromWall = InvokeWallEscapeDirection(Vector3.left, escapeNormal);

        Assert.That(towardWall, Is.EqualTo(Vector3.zero));
        Assert.That(alongWall, Is.EqualTo(Vector3.zero));
        Assert.That(awayFromWall, Is.EqualTo(Vector3.left));
    }

    [Test]
    public void WallEscapeInputLock_StartsOnlyWhenLandingAfterWallEscape()
    {
        Assert.That(InvokeStaticBool("ShouldStartWallEscapeInputLock", true, true), Is.True);
        Assert.That(InvokeStaticBool("ShouldStartWallEscapeInputLock", false, true), Is.False);
        Assert.That(InvokeStaticBool("ShouldStartWallEscapeInputLock", true, false), Is.False);
    }

    [Test]
    public void WallEscapeInputLock_StaysActiveUntilMovementInputReleased()
    {
        Assert.That(InvokeStaticBool("ShouldBlockWallEscapeInput", true, true), Is.True);
        Assert.That(InvokeStaticBool("ShouldBlockWallEscapeInput", true, false), Is.False);
        Assert.That(InvokeStaticBool("ShouldBlockWallEscapeInput", false, true), Is.False);
    }

    [Test]
    public void StopPlanarVelocity_PreservesVerticalVelocity()
    {
        Vector3 stoppedVelocity = InvokeStopPlanarVelocity(new Vector3(4f, -3f, 2f));

        Assert.That(stoppedVelocity, Is.EqualTo(new Vector3(0f, -3f, 0f)));
    }

    private static bool InvokeStaticBool(string methodName, Vector3 normal, float threshold)
    {
        MethodInfo method = GetStaticMethod(methodName, typeof(Vector3), typeof(float));
        return (bool)method.Invoke(null, new object[] { normal, threshold });
    }

    private static Vector3 InvokeWallEscapeDirection(Vector3 inputDirection, Vector3 escapeNormal)
    {
        MethodInfo method = GetStaticMethod(
            "GetWallEscapeDirection",
            typeof(Vector3),
            typeof(Vector3));
        return (Vector3)method.Invoke(null, new object[] { inputDirection, escapeNormal });
    }

    private static bool InvokeStaticBool(string methodName, bool firstArgument, bool secondArgument)
    {
        MethodInfo method = GetStaticMethod(methodName, typeof(bool), typeof(bool));
        return (bool)method.Invoke(null, new object[] { firstArgument, secondArgument });
    }

    private static Vector3 InvokeStopPlanarVelocity(Vector3 velocity)
    {
        MethodInfo method = GetStaticMethod("StopPlanarVelocity", typeof(Vector3));
        return (Vector3)method.Invoke(null, new object[] { velocity });
    }

    private static MethodInfo GetStaticMethod(string methodName, params System.Type[] parameterTypes)
    {
        MethodInfo method = typeof(PlayerMovement).GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic,
            null,
            parameterTypes,
            null);

        Assert.That(method, Is.Not.Null, $"PlayerMovement에 {methodName} 판정 함수가 필요합니다.");
        return method;
    }
}
