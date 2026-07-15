using UnityEngine;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private GameObject settings_panel;

    private void Awake()
    {
        settings_panel.SetActive(false);
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