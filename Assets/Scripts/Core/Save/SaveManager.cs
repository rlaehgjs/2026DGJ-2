using System;
using System.IO;
using UnityEngine;

public class SaveManager
{
    public const string DefaultFileName = "game-save.json";

    private readonly string savePath;

    public SaveManager()
        : this(Path.Combine(Application.persistentDataPath, DefaultFileName))
    {
    }

    public SaveManager(string savePath)
    {
        if (string.IsNullOrWhiteSpace(savePath))
        {
            throw new ArgumentException("A save path is required.", nameof(savePath));
        }

        this.savePath = savePath;
    }

    public void Save(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        string directoryPath = Path.GetDirectoryName(savePath);

        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
    }

    public bool TryLoad(out GameSaveData saveData)
    {
        saveData = null;

        if (!File.Exists(savePath))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(savePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            saveData = JsonUtility.FromJson<GameSaveData>(json);
            return saveData != null;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }
}
