using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private GameProgressManager gameProgressManager;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private GameObject objectiveRoot;

    private LocalizationManager localizationManager;
    private GameProgressState displayedState;
    private bool hasDisplayedState;

    public void Configure(GameProgressManager manager)
    {
        gameProgressManager = manager;
    }

    private void Start()
    {
        if (gameProgressManager == null || objectiveText == null || objectiveRoot == null)
        {
            Debug.LogWarning("ObjectiveUI: GameProgressManager, ObjectiveText, ObjectiveRoot을 연결해야 합니다.", this);
            enabled = false;
            return;
        }

        gameProgressManager.ProgressChanged += Refresh;
        gameProgressManager.ProgressRestored += Refresh;

        localizationManager = LocalizationManager.Instance;
        if (localizationManager != null)
            localizationManager.LanguageChanged += RefreshCurrentObjective;

        Refresh(gameProgressManager.CurrentState);
    }

    private void OnDestroy()
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged -= Refresh;
            gameProgressManager.ProgressRestored -= Refresh;
        }

        if (localizationManager != null)
            localizationManager.LanguageChanged -= RefreshCurrentObjective;
    }

    private void LateUpdate()
    {
        if (gameProgressManager != null
            && (!hasDisplayedState || displayedState != gameProgressManager.CurrentState))
        {
            // ponytail: per-frame comparison is a UI-only fallback for missed progress events.
            Refresh(gameProgressManager.CurrentState);
        }
    }

    private void Refresh(GameProgressState state)
    {
        displayedState = state;
        hasDisplayedState = true;
        string key = GetObjectiveKey(state);
        objectiveRoot.SetActive(!string.IsNullOrEmpty(key));

        if (!string.IsNullOrEmpty(key))
            objectiveText.text = localizationManager != null ? localizationManager.Get(key) : key;
    }

    private void RefreshCurrentObjective()
    {
        Refresh(gameProgressManager.CurrentState);
    }

    private static string GetObjectiveKey(GameProgressState state)
    {
        return state switch
        {
            GameProgressState.FindKitchen => "objective_find_kitchen_key",
            GameProgressState.InspectRefrigerator => "objective_inspect_refrigerator",
            GameProgressState.FindFrontDoorKey => "objective_find_front_door_key",
            GameProgressState.FindGenerator => "objective_find_generator",
            GameProgressState.FindGeneratorWire => "objective_find_generator_wire",
            GameProgressState.RepairGenerator => "objective_repair_generator",
            GameProgressState.FindNails => "objective_find_plywood",
            GameProgressState.FindHammer => "objective_find_hammer",
            GameProgressState.RepairRefrigeratorWall => "objective_repair_refrigerator_wall",
            GameProgressState.FindCoolantCapsule => "objective_find_coolant_capsule",
            GameProgressState.RepairFreezer => "objective_repair_freezer",
            GameProgressState.EnterFreezer => "objective_enter_freezer",
            _ => null
        };
    }
}
