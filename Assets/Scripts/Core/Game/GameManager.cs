using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenuScene";
    private const string StartCutSceneName = "StartCutScene";
    private const string MainGameSceneName = "MainGameScene";

    [SerializeField] private GameState initialState = GameState.MainMenu;
    [SerializeField] private SaveManager saveManager;

    public GameState CurrentState { get; private set; }

    public event Action<GameState> StateChanged;

    private void Awake()
    {
        ApplyState(initialState, false);
    }

    public void SetState(GameState nextState)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        ApplyState(nextState, true);
    }

    public void StartNewGame()
    {
        saveManager?.ClearGameSave();
        LoadScene(StartCutSceneName);
    }

    public bool ContinueGame()
    {
        if (saveManager == null || !saveManager.HasGameSave())
        {
            return false;
        }

        LoadScene(MainGameSceneName);
        return true;
    }

    public void CompleteCutscene()
    {
        LoadScene(MainGameSceneName);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            SetState(GameState.Playing);
        }
    }

    public void TriggerGameOver()
    {
        SetState(GameState.GameOver);
    }

    public void StartEnding()
    {
        SetState(GameState.Ending);
    }

    public void RestartGame()
    {
        LoadScene(MainGameSceneName);
    }

    public void ReturnToMainMenu()
    {
        saveManager?.SaveCurrentProgress();
        LoadScene(MainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void ApplyState(GameState nextState, bool notify)
    {
        CurrentState = nextState;
        Time.timeScale = IsPausedState(nextState) ? 0f : 1f;

        bool lockCursor = nextState == GameState.Playing || nextState == GameState.Cutscene;
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;

        if (notify)
        {
            StateChanged?.Invoke(CurrentState);
        }
    }

    private void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private static bool IsPausedState(GameState state)
    {
        return state == GameState.Paused || state == GameState.GameOver;
    }
}
