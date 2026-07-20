using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public const int CurrentVersion = 2;

    public int Version = CurrentVersion;
    public GameProgressState ProgressState = GameProgressState.FindKitchen;
    public List<ItemSaveEntry> Inventory = new List<ItemSaveEntry>();
    public List<string> CollectedItemIds = new List<string>(); //획득한 아이템 리스트
    public List<string> ChangedWorldObjectIds = new List<string>();

    public void EnsureCollections()
    {
        Inventory ??= new List<ItemSaveEntry>();
        CollectedItemIds ??= new List<string>();
        ChangedWorldObjectIds ??= new List<string>();
    }
}

[Serializable]
public class ItemSaveEntry
{
    public string ItemId;
    public int Amount;
}
