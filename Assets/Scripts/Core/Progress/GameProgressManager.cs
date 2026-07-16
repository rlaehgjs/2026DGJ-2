using System;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    [SerializeField] private GameProgressState initialState = GameProgressState.FindKitchenKey;

    public GameProgressState CurrentState { get; private set; }

    public bool IsPowerRestored => CurrentState >= GameProgressState.FindPlywood;
    public bool IsRefrigeratorWallRepaired => CurrentState >= GameProgressState.FindCoolantCapsule;
    public bool IsFreezerRepaired => CurrentState >= GameProgressState.EnterFreezer;
    public bool CanEnterFreezer => CurrentState == GameProgressState.EnterFreezer;

    public event Action<GameProgressState> ProgressChanged;
    public event Action PowerRestored;
    public event Action RefrigeratorWallRepaired;
    public event Action FreezerRepaired;
    public event Action ProgressCompleted;

    private void Awake()
    {
        SetState(initialState, false);
    }

    public bool TryCompleteCurrentObjective()
    {
        switch (CurrentState)
        {
            case GameProgressState.FindKitchenKey:
                SetState(GameProgressState.InspectRefrigerator, true);
                return true;
            case GameProgressState.InspectRefrigerator:
                SetState(GameProgressState.FindGenerator, true);
                return true;
            case GameProgressState.FindGenerator:
                SetState(GameProgressState.FindGeneratorWire, true);
                return true;
            case GameProgressState.FindGeneratorWire:
                SetState(GameProgressState.RepairGenerator, true);
                return true;
            case GameProgressState.RepairGenerator:
                SetState(GameProgressState.FindPlywood, true);
                return true;
            case GameProgressState.FindPlywood:
                SetState(GameProgressState.FindHammer, true);
                return true;
            case GameProgressState.FindHammer:
                SetState(GameProgressState.RepairRefrigeratorWall, true);
                return true;
            case GameProgressState.RepairRefrigeratorWall:
                SetState(GameProgressState.FindCoolantCapsule, true);
                return true;
            case GameProgressState.FindCoolantCapsule:
                SetState(GameProgressState.RepairFreezer, true);
                return true;
            case GameProgressState.RepairFreezer:
                SetState(GameProgressState.EnterFreezer, true);
                return true;
            case GameProgressState.EnterFreezer:
                ProgressCompleted?.Invoke();
                return true;
            default:
                return false;
        }
    }

    public bool TryCompleteKitchenKey()
    {
        return TryCompleteExpectedObjective(GameProgressState.FindKitchenKey);
    }

    public bool TryCompleteRefrigeratorInspection()
    {
        return TryCompleteExpectedObjective(GameProgressState.InspectRefrigerator);
    }

    public bool TryCompleteGeneratorInspection()
    {
        return TryCompleteExpectedObjective(GameProgressState.FindGenerator);
    }

    public bool TryCompleteGeneratorWire()
    {
        return TryCompleteExpectedObjective(GameProgressState.FindGeneratorWire);
    }

    public bool TryRepairGenerator()
    {
        return TryCompleteExpectedObjective(GameProgressState.RepairGenerator);
    }

    public bool TryCompletePlywood()
    {
        return TryCompleteExpectedObjective(GameProgressState.FindPlywood);
    }

    public bool TryCompleteHammer()
    {
        return TryCompleteExpectedObjective(GameProgressState.FindHammer);
    }

    public bool TryRepairRefrigeratorWall()
    {
        return TryCompleteExpectedObjective(GameProgressState.RepairRefrigeratorWall);
    }

    public bool TryCompleteCoolantCapsule()
    {
        return TryCompleteExpectedObjective(GameProgressState.FindCoolantCapsule);
    }

    public bool TryRepairFreezer()
    {
        return TryCompleteExpectedObjective(GameProgressState.RepairFreezer);
    }

    public bool TryEnterFreezer()
    {
        return TryCompleteExpectedObjective(GameProgressState.EnterFreezer);
    }

    public bool RestoreState(GameProgressState savedState)
    {
        if (!Enum.IsDefined(typeof(GameProgressState), savedState))
        {
            return false;
        }

        CurrentState = savedState;
        ProgressChanged?.Invoke(CurrentState);
        NotifyRestoredWorldState();
        return true;
    }

    private bool TryCompleteExpectedObjective(GameProgressState expectedState)
    {
        return CurrentState == expectedState && TryCompleteCurrentObjective();
    }

    private void SetState(GameProgressState nextState, bool notify)
    {
        CurrentState = nextState;

        if (!notify)
        {
            return;
        }

        ProgressChanged?.Invoke(CurrentState);

        if (nextState == GameProgressState.FindPlywood)
        {
            PowerRestored?.Invoke();
        }

        if (nextState == GameProgressState.FindCoolantCapsule)
        {
            RefrigeratorWallRepaired?.Invoke();
        }

        if (nextState == GameProgressState.EnterFreezer)
        {
            FreezerRepaired?.Invoke();
        }
    }

    private void NotifyRestoredWorldState()
    {
        if (IsPowerRestored)
        {
            PowerRestored?.Invoke();
        }

        if (IsRefrigeratorWallRepaired)
        {
            RefrigeratorWallRepaired?.Invoke();
        }

        if (IsFreezerRepaired)
        {
            FreezerRepaired?.Invoke();
        }
    }
}
