using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class Hous1FUpdateInteractionTests
{
    private const string PrefabPath = "Assets/Hous_1F_Update.prefab";

    [Test]
    public void Hous1FUpdate_HasMigratedOpeningInteractables()
    {
        GameObject house = PrefabUtility.LoadPrefabContents(PrefabPath);

        try
        {
            DoorInteractable[] doors = house.GetComponentsInChildren<DoorInteractable>(true);
            DrawerInteractable[] drawers = house.GetComponentsInChildren<DrawerInteractable>(true);
            FrontDoorLock[] locks = house.GetComponentsInChildren<FrontDoorLock>(true);

            Assert.That(doors, Has.Length.EqualTo(15));
            Assert.That(drawers, Has.Length.EqualTo(12));
            Assert.That(locks, Has.Length.EqualTo(1));
            Assert.That(doors.Count(door => door.gameObject.name == "DoorHinge"), Is.EqualTo(2));

            foreach (DoorInteractable door in doors)
            {
                Assert.That(door.gameObject.isStatic, Is.False);
                typeof(DoorInteractable)
                    .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(door, null);
                Assert.That(
                    door.GetComponentInChildren<Collider>(true),
                    Is.Not.Null,
                    GetHierarchyPath(door.transform));
            }

            foreach (DrawerInteractable drawer in drawers)
            {
                Assert.That(drawer.gameObject.isStatic, Is.False);
                typeof(DrawerInteractable)
                    .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(drawer, null);
                Assert.That(
                    drawer.GetComponentInChildren<Collider>(true),
                    Is.Not.Null,
                    GetHierarchyPath(drawer.transform));
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(house);
        }
    }

    private static string GetHierarchyPath(Transform transform)
    {
        return transform.parent == null
            ? transform.name
            : GetHierarchyPath(transform.parent) + "/" + transform.name;
    }
}
