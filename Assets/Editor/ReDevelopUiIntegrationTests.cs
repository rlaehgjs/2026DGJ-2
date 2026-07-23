using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ReDevelopUiIntegrationTests
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";
    private const string MainGameScenePath = "Assets/Scenes/MainGameScene.unity";
    private const string UiSandboxScenePath = "Assets/Scenes/Sandbox/UISandbox.unity";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";

    [Test]
    public void MainMenuScene_HasConnectedMenuUiAndManagers()
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

        MainMenuController menuUi = RequireSingle<MainMenuController>(scene);
        RequireSingle<SoundManager>(scene);
        RequireSingle<LocalizationManager>(scene);
        AssertEventSystemUsesCurrentInput(scene);

        AssertReference(menuUi, "gameManager");
        AssertReference(menuUi, "saveManager");
        AssertReference(menuUi, "soundManager");
    }

    [Test]
    public void MainGameScene_HasConnectedInGameUiAndManagers()
    {
        Scene scene = EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Single);

        InGameUIController inGameUi = RequireSingle<InGameUIController>(scene);
        RequireSingle<SoundManager>(scene);
        RequireSingle<LocalizationManager>(scene);
        AssertEventSystemUsesCurrentInput(scene);

        foreach (string propertyName in new[]
                 {
                     "gameManager", "inputReader", "saveManager", "gameProgressManager",
                     "playerMeltSystem", "playerInventory", "playerLook", "soundManager"
                 })
        {
            AssertReference(inGameUi, propertyName);
        }
    }

    [Test]
    public void UiSandbox_UsesCurrentPlayerAndUiManagers()
    {
        Scene scene = EditorSceneManager.OpenScene(UiSandboxScenePath, OpenSceneMode.Single);

        PlayerMovement playerMovement = RequireSingle<PlayerMovement>(scene);
        GameObject playerRoot = PrefabUtility.GetNearestPrefabInstanceRoot(playerMovement.gameObject);
        Assert.That(playerRoot, Is.Not.Null);
        Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(playerRoot), Is.EqualTo(PlayerPrefabPath));

        InGameUIController inGameUi = RequireSingle<InGameUIController>(scene);
        RequireSingle<SoundManager>(scene);
        RequireSingle<LocalizationManager>(scene);
        AssertEventSystemUsesCurrentInput(scene);

        foreach (string propertyName in new[]
                 {
                     "gameManager", "inputReader", "saveManager", "gameProgressManager",
                     "playerMeltSystem", "playerInventory", "playerLook", "soundManager"
                 })
        {
            AssertReference(inGameUi, propertyName);
        }
    }

    private static T RequireSingle<T>(Scene scene) where T : Component
    {
        T[] components = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true))
            .ToArray();

        Assert.That(components.Length, Is.EqualTo(1), $"{scene.path} must contain exactly one {typeof(T).Name}.");
        return components[0];
    }

    private static void AssertEventSystemUsesCurrentInput(Scene scene)
    {
        EventSystem eventSystem = RequireSingle<EventSystem>(scene);
        BaseInputModule[] inputModules = eventSystem.GetComponents<BaseInputModule>();

        Assert.That(inputModules.Any(module => module.enabled), Is.True,
            "The UI EventSystem needs an enabled input module.");
    }

    private static void AssertReference(Object target, string propertyName)
    {
        SerializedProperty property = new SerializedObject(target).FindProperty(propertyName);
        Assert.That(property, Is.Not.Null, $"{target.GetType().Name}.{propertyName} is missing.");
        Assert.That(property.objectReferenceValue, Is.Not.Null,
            $"{target.GetType().Name}.{propertyName} is not connected.");
    }
}
