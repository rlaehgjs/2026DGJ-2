using System;

public class GameManager
{
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    public event Action<GameState> StateChanged;

    public void SetState(GameState nextState)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        CurrentState = nextState;
        StateChanged?.Invoke(CurrentState);
    }
}
