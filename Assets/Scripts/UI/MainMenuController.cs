using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settings_panel;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private AudioSource titleBgm;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (saveManager == null)
            saveManager = FindAnyObjectByType<SaveManager>();

        if (soundManager == null)
            soundManager = FindAnyObjectByType<SoundManager>();

        soundManager?.Configure(saveManager);

        if (settings_panel != null)
            settings_panel.SetActive(false);

        if (continueButton != null)
            continueButton.interactable = saveManager != null && saveManager.HasGameSave();

        foreach (MasterVolumeSlider slider in GetComponentsInChildren<MasterVolumeSlider>(true))
            slider.Configure(soundManager, saveManager);

        foreach (BgmVolumeSlider slider in GetComponentsInChildren<BgmVolumeSlider>(true))
            slider.Configure(soundManager, saveManager);

        foreach (SfxVolumeSlider slider in GetComponentsInChildren<SfxVolumeSlider>(true))
            slider.Configure(soundManager, saveManager);

        foreach (BrightnessSlider slider in GetComponentsInChildren<BrightnessSlider>(true))
            slider.Configure(saveManager);

        foreach (MouseSensitivitySlider slider in GetComponentsInChildren<MouseSensitivitySlider>(true))
            slider.Configure(null, saveManager);

        FindAnyObjectByType<LocalizationManager>()?.Configure(saveManager);

    }

    private void Start()
    {
        PlayTitleBgm();
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

    private void OnDisable()
    {
        soundManager?.StopTitleBgm();
    }

    private void PlayTitleBgm()
    {
        if (soundManager == null || titleBgm == null || titleBgm.clip == null)
            return;

        titleBgm.Stop();
        titleBgm.enabled = false;
        soundManager.PlayTitleBgm(titleBgm.clip, titleBgm.loop);
    }
}
