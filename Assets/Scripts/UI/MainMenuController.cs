using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settings_panel;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SaveManager saveManager;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (saveManager == null)
            saveManager = FindAnyObjectByType<SaveManager>();

        if (settings_panel != null)
            settings_panel.SetActive(false);

        if (continueButton != null)
            continueButton.interactable = saveManager != null && saveManager.HasGameSave();
    }

    public void StartNewGame()
    {
        gameManager?.StartNewGame();
    }

    public void OpenSettings()
    {
        if (settings_panel != null)
            settings_panel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settings_panel != null)
            settings_panel.SetActive(false);
    }

    public void ContinueGame()
    {
        gameManager?.ContinueGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
