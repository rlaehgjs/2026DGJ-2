using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public const string GameSaveKey = "game-save";

    [SerializeField] private GameProgressManager gameProgressManager;

    private void OnEnable()
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged += HandleProgressChanged;
        }
    }

    private void Start()
    {
        LoadCurrentProgress();
    }

    private void OnDisable()
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged -= HandleProgressChanged;
        }
    }

    public void SaveGame(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new System.ArgumentNullException(nameof(saveData));
        }

        saveData.EnsureCollections();
        PlayerPrefs.SetString(GameSaveKey, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }

    public bool TryLoadGame(out GameSaveData saveData)
    {
        saveData = null;

        if (!HasGameSave())
        {
            return false;
        }

        try
        {
            saveData = JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(GameSaveKey));
            saveData?.EnsureCollections();
            return saveData != null;
        }
        catch (System.ArgumentException)
        {
            return false;
        }
    }

    public bool HasGameSave()
    {
        return PlayerPrefs.HasKey(GameSaveKey) && !string.IsNullOrWhiteSpace(PlayerPrefs.GetString(GameSaveKey));
    }

    public void ClearGameSave()
    {
        PlayerPrefs.DeleteKey(GameSaveKey);
        PlayerPrefs.Save();
    }

    public void SaveCurrentProgress()
    {
        if (gameProgressManager == null)
        {
            return;
        }

        SaveGame(new GameSaveData
        {
            ProgressState = gameProgressManager.CurrentState
        });
    }

    public bool LoadCurrentProgress()
    {
        if (gameProgressManager == null || !TryLoadGame(out GameSaveData saveData))
        {
            return false;
        }

        return gameProgressManager.RestoreState(saveData.ProgressState);
    }

    private void HandleProgressChanged(GameProgressState _)
    {
        SaveCurrentProgress();
    }
}
