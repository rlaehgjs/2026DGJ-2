using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float titleVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    private AudioSource titleBgmSource;
    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private SaveManager saveManager;

    public float MasterVolume => masterVolume;
    public float TitleVolume => titleVolume;
    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        titleBgmSource = CreateSource("Title BGM", true);
        bgmSource = CreateSource("BGM", true);
        sfxSource = CreateSource("SFX", false);
        ApplyVolumes();
    }

    private void Start()
    {
        ApplySavedSettings();
    }

    public void Configure(SaveManager settingsSaveManager)
    {
        saveManager = settingsSaveManager;
        ApplySavedSettings();
    }

    public void PlayBgm(AudioClip clip, bool loop = true)
    {
        if (clip == null || bgmSource == null)
        {
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayTitleBgm(AudioClip clip, bool loop = true)
    {
        if (clip == null || titleBgmSource == null)
        {
            return;
        }

        if (titleBgmSource.clip == clip && titleBgmSource.isPlaying)
        {
            return;
        }

        titleBgmSource.clip = clip;
        titleBgmSource.loop = loop;
        titleBgmSource.Play();
    }

    public void StopBgm()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    public void StopTitleBgm()
    {
        if (titleBgmSource != null)
            titleBgmSource.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySfxAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetTitleVolume(float value)
    {
        titleVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    private AudioSource CreateSource(string sourceName, bool loop)
    {
        GameObject sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        return source;
    }

    private void ApplyVolumes()
    {
        AudioListener.volume = masterVolume;

        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }

        if (titleBgmSource != null)
        {
            titleBgmSource.volume = titleVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    private void ApplySavedSettings()
    {
        if (saveManager == null)
            return;

        GameSettingsData settings = saveManager.LoadSettings();
        masterVolume = settings.MasterVolume;
        titleVolume = settings.TitleVolume;
        bgmVolume = settings.BgmVolume;
        sfxVolume = settings.SfxVolume;
        ApplyVolumes();
    }
}
