using System;
using UnityEngine;

[Serializable]
public class GameSettingsData
{
    public const int KoreanLanguage = 0;
    public const int EnglishLanguage = 1;

    public float MasterVolume = 1f;
    public float TitleVolume = 1f;
    public float BgmVolume = 1f;
    public float SfxVolume = 1f;
    public float Brightness = 0.5f;
    public float MouseSensitivity = 0.5f;
    public int Language = KoreanLanguage;

    public void ClampValues()
    {
        MasterVolume = Mathf.Clamp01(MasterVolume);
        TitleVolume = Mathf.Clamp01(TitleVolume);
        BgmVolume = Mathf.Clamp01(BgmVolume);
        SfxVolume = Mathf.Clamp01(SfxVolume);
        Brightness = Mathf.Clamp01(Brightness);
        MouseSensitivity = Mathf.Clamp01(MouseSensitivity);
        Language = Mathf.Clamp(Language, KoreanLanguage, EnglishLanguage);
    }
}
