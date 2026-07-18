using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameInputReader inputReader;

    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject endingPanel;

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.GameStateChanged += ApplyGameState;

        if (inputReader != null)
            inputReader.PausePressed += TogglePause;
    }

    private void Start()
    {
        if (gameManager != null)
            ApplyGameState(gameManager.CurrentState);
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.GameStateChanged -= ApplyGameState;

        if (inputReader != null)
            inputReader.PausePressed -= TogglePause;
    }

    private void TogglePause()
    {
        if (gameManager == null)
            return;

        if (gameManager.CurrentState == GameState.Playing)
            gameManager.PauseGame();
        else if (gameManager.CurrentState == GameState.Paused)
            gameManager.ResumeGame();
    }

    private void ApplyGameState(GameState state)
    {
        if (pausePanel != null)
            pausePanel.SetActive(state == GameState.Paused);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(state == GameState.GameOver);
        if (endingPanel != null)
            endingPanel.SetActive(state == GameState.Ending);

        if (state == GameState.Playing && settingsPanel != null)
            settingsPanel.SetActive(false);

        if (hudPanel != null)
        {
            hudPanel.SetActive(
                state != GameState.GameOver &&
                state != GameState.Ending);
        }
    }
}
