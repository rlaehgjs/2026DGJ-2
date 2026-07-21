using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SfxVolumeSlider : MonoBehaviour
{
    [SerializeField] private TMP_Text percentText;

    private Slider slider;
    private SoundManager soundManager;
    private SaveManager saveManager;

    public void Configure(SoundManager manager, SaveManager settingsSaveManager)
    {
        soundManager = manager;
        saveManager = settingsSaveManager;
    }

    private void Start()
    {
        slider = GetComponent<Slider>();
        if (percentText == null)
        {
            Debug.LogWarning("SfxVolumeSlider: Percent Text를 연결해야 합니다.", this);
            enabled = false;
            return;
        }

        float savedVolume = saveManager != null ? saveManager.LoadSettings().SfxVolume : 1f;
        slider.SetValueWithoutNotify(savedVolume);
        ApplyVolume(savedVolume);
        slider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float volume)
    {
        ApplyVolume(volume);

        if (saveManager == null)
            return;

        GameSettingsData settings = saveManager.LoadSettings();
        settings.SfxVolume = volume;
        saveManager.SaveSettings(settings);
    }

    private void ApplyVolume(float volume)
    {
        (soundManager ?? SoundManager.Instance)?.SetSfxVolume(volume);
        percentText.text = $"{Mathf.RoundToInt(volume * 100)}%";
    }
}
