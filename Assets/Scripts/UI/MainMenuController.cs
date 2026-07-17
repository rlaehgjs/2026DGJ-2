using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settings_panel;
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        settings_panel.SetActive(false);

        if (continueButton != null)
            continueButton.interactable = false;
    }

    public void OpenSettings()
    {
        settings_panel.SetActive(true);
    }

    public void CloseSettings()
    {
        settings_panel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
