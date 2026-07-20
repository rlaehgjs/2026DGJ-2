using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BgmVolumeSlider : MonoBehaviour
{
    private const string BgmVolumeKey = "BgmVolume";

    [SerializeField] private AudioSource titleBgm;
    [SerializeField] private TMP_Text percentText;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        if (slider == null || titleBgm == null || percentText == null)
        {
            Debug.LogWarning("BgmVolumeSlider: Slider, Title BGM, Percent Text를 모두 연결해야 합니다.", this);
            enabled = false;
            return;
        }

        float savedVolume = PlayerPrefs.GetFloat(BgmVolumeKey, titleBgm.volume);

        slider.SetValueWithoutNotify(savedVolume);
        SetVolume(savedVolume);

        slider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float volume)
    {
        titleBgm.volume = volume;
        SoundManager.Instance?.SetBgmVolume(volume);
        percentText.text = $"{Mathf.RoundToInt(volume * 100)}%";

        PlayerPrefs.SetFloat(BgmVolumeKey, volume);
        PlayerPrefs.Save();
    }
}
