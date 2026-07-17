using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public const string GameSaveKey = "game-save";

    [SerializeField] private GameProgressManager gameProgressManager;

    private GameSaveData currentSaveData;
    private bool isRestoringProgress;

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
        currentSaveData = saveData;
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
            currentSaveData = JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(GameSaveKey));
            currentSaveData?.EnsureCollections();
            saveData = currentSaveData;
            return currentSaveData != null;
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
        currentSaveData = null;
        PlayerPrefs.DeleteKey(GameSaveKey);
        PlayerPrefs.Save();
    }

    public void SaveCurrentProgress()
    {
        if (gameProgressManager == null)
        {
            return;
        }

        GameSaveData saveData = GetOrCreateSaveData();
        saveData.ProgressState = gameProgressManager.CurrentState;
        SaveGame(saveData);
    }

    public bool LoadCurrentProgress()
    {
        if (gameProgressManager == null || !TryLoadGame(out GameSaveData saveData))
        {
            return false;
        }

        isRestoringProgress = true;

        try
        {
            return gameProgressManager.RestoreState(saveData.ProgressState);
        }
        finally
        {
            isRestoringProgress = false;
        }
    }

    private void HandleProgressChanged(GameProgressState _)
    {
        if (!isRestoringProgress)
        {
            SaveCurrentProgress();
        }
    }

    private GameSaveData GetOrCreateSaveData()
    {
        if (currentSaveData != null)
        {
            return currentSaveData;
        }

        if (TryLoadGame(out GameSaveData saveData))
        {
            return saveData;
        }

        currentSaveData = new GameSaveData();
        return currentSaveData;
    }
}
