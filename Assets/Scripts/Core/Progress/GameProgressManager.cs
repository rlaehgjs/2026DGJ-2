using System;

public class GameProgressManager
{
    public GameProgressState CurrentState { get; private set; } = GameProgressState.NotStarted;

    public event Action<GameProgressState> StateChanged;

    public void SetState(GameProgressState nextState)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        CurrentState = nextState;
        StateChanged?.Invoke(CurrentState);
    }
}
