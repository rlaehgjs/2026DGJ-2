using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public const string GameSaveKey = "game-save";
    public const string GameSettingsKey = "game-settings";

    [SerializeField] private GameProgressManager gameProgressManager;
    [SerializeField] private PlayerInventory playerInventory;

    private GameSaveData currentSaveData;
    private GameSettingsData currentSettings;
    private bool isRestoringSaveData;

    private void OnEnable()
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged += HandleProgressChanged;
        }

        if (playerInventory != null)
        {
            playerInventory.InventoryChanged += HandleInventoryChanged;
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

        if (playerInventory != null)
        {
            playerInventory.InventoryChanged -= HandleInventoryChanged;
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

    public void SaveSettings(GameSettingsData settings)
    {
        if (settings == null)
        {
            throw new System.ArgumentNullException(nameof(settings));
        }

        settings.ClampValues();
        currentSettings = settings;
        PlayerPrefs.SetString(GameSettingsKey, JsonUtility.ToJson(settings));
        PlayerPrefs.Save();
    }

    public GameSettingsData LoadSettings()
    {
        if (currentSettings != null)
        {
            return currentSettings;
        }

        if (!HasSettings())
        {
            currentSettings = new GameSettingsData();
            return currentSettings;
        }

        try
        {
            currentSettings = JsonUtility.FromJson<GameSettingsData>(PlayerPrefs.GetString(GameSettingsKey));

            if (currentSettings == null)
            {
                currentSettings = new GameSettingsData();
            }

            currentSettings.ClampValues();
            return currentSettings;
        }
        catch (System.ArgumentException)
        {
            currentSettings = new GameSettingsData();
            return currentSettings;
        }
    }

    public bool RegisterCollectedItem(string saveId)
    {
        if (string.IsNullOrWhiteSpace(saveId))
        {
            return false;
        }

        GameSaveData saveData = GetOrCreateSaveData();

        if (saveData.CollectedItemIds.Contains(saveId))
        {
            return false;
        }

        saveData.CollectedItemIds.Add(saveId);
        SaveGame(saveData);
        return true;
    }

    public bool IsItemCollected(string saveId) //playerPrefs에 저장된 데이터 확인
    {
        if (string.IsNullOrWhiteSpace(saveId))
        {
            return false;
        }

        if (currentSaveData == null && !TryLoadGame(out _))
        {
            return false;
        }

        return currentSaveData.CollectedItemIds.Contains(saveId); //있으면 가져오기
    }

    public void SaveCurrentProgress()
    {
        if (gameProgressManager == null && playerInventory == null)
        {
            return;
        }

        GameSaveData saveData = GetOrCreateSaveData();

        if (gameProgressManager != null)
        {
            saveData.ProgressState = gameProgressManager.CurrentState;
        }

        if (playerInventory != null)
        {
            saveData.Inventory = playerInventory.CreateSaveEntries();
        }

        SaveGame(saveData);
    }

    public bool LoadCurrentProgress()
    {
        if (!TryLoadGame(out GameSaveData saveData))
        {
            return false;
        }

        isRestoringSaveData = true;

        try
        {
            bool restoredAnyData = false;

            if (gameProgressManager != null)
            {
                restoredAnyData = gameProgressManager.RestoreState(saveData.ProgressState);
            }

            if (playerInventory != null)
            {
                playerInventory.RestoreFromSaveEntries(saveData.Inventory);
                restoredAnyData = true;
            }

            return restoredAnyData;
        }
        finally
        {
            isRestoringSaveData = false;
        }
    }

    private void HandleProgressChanged(GameProgressState _)
    {
        if (!isRestoringSaveData)
        {
            SaveCurrentProgress();
        }
    }

    private void HandleInventoryChanged()
    {
        if (!isRestoringSaveData)
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

    private static bool HasSettings()
    {
        return PlayerPrefs.HasKey(GameSettingsKey) && !string.IsNullOrWhiteSpace(PlayerPrefs.GetString(GameSettingsKey));
    }
}
