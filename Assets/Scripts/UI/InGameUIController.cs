using UnityEngine;

[DefaultExecutionOrder(-100)]
public class InGameUIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameInputReader inputReader;
    [Header("Player and Managers")]
    [SerializeField] private GameProgressManager gameProgressManager;
    [SerializeField] private PlayerMeltSystem playerMeltSystem;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerLook playerLook;

    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject endingPanel;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (inputReader == null)
            inputReader = FindAnyObjectByType<GameInputReader>();

        if (gameProgressManager == null)
            gameProgressManager = FindAnyObjectByType<GameProgressManager>();

        if (playerMeltSystem == null)
            playerMeltSystem = FindAnyObjectByType<PlayerMeltSystem>();

        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();

        if (playerLook == null)
            playerLook = FindAnyObjectByType<PlayerLook>();

        ConfigureChildUI();
    }

    private void ConfigureChildUI()
    {
        GetComponentInChildren<ObjectiveUI>(true)?.Configure(gameProgressManager);
        GetComponentInChildren<MeltGaugeUI>(true)?.Configure(playerMeltSystem, gameManager);
        GetComponentInChildren<InventoryUI>(true)?.Configure(playerInventory);
        GetComponentInChildren<ItemPickupToastListener>(true)?.Configure(playerInventory);
        GetComponentInChildren<MouseSensitivitySlider>(true)?.Configure(playerLook);
    }

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

    public void ResumeGame()
    {
        gameManager?.ResumeGame();
    }

    public void RestartGame()
    {
        gameManager?.RestartGame();
    }

    public void ReturnToMainMenu()
    {
        gameManager?.ReturnToMainMenu();
    }

    public void QuitGame()
    {
        gameManager?.QuitGame();
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
